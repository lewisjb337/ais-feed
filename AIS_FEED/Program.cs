using System.Net.WebSockets;
using System.Text;
using AIS_Feed.Enums;
using AIS_Feed.Models;
using AIS_Feed.Models.AIS;
using AIS_FEED.Services;
using AIS_FEED.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Supabase;

namespace AIS_Feed;

internal abstract class Program
{
    private static IProjectConfiguration? configuration { get; set; }
    public static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration config = builder.Build();

        configuration = new ProjectConfiguration(config);
        
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        var client = new Client(configuration!.GetSupabaseConfig().Url, configuration.GetSupabaseConfig().Key, options);
        await client.InitializeAsync();

        var source = new CancellationTokenSource();
        var token = source.Token;

        while (true)
        {
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(new Uri(configuration.GetAisStreamConfig().Url), token);
                await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"APIKey\": \"" + configuration.GetAisStreamConfig().Key + "\", \"BoundingBoxes\": [[[-90.0, -180.0], [90.0, 180.0]]]}")), WebSocketMessageType.Text, true, token);

                var buffer = new byte[4096];
                
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                        break;
                    }
                    else
                    {
                        var content = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        if (!content.Contains("ShipStaticData")) continue;

                        var deserializedObject = JsonConvert.DeserializeObject<Root>(content);
                        var metaData = deserializedObject?.MetaData;
                        var shipStaticData = deserializedObject?.Message?.ShipStaticData;

                        if (metaData == null || shipStaticData == null) break;
                        
                        var vesselResult = await client.From<Vessel>().Get(token);
                        var existingVessels = vesselResult.Models;

                        var existingVessel = existingVessels.FirstOrDefault(v => v.MMSI == metaData.MMSI);

                        var shipType = shipStaticData.Type switch
                        {
                            >= 20 and <= 29 => VesselType.WingInGround,
                            30 => VesselType.Fishing,
                            31 or 32 => VesselType.Towing,
                            33 => VesselType.Dredging,
                            34 => VesselType.DivingOps,
                            35 => VesselType.MilitaryOps,
                            36 => VesselType.Sailing,
                            37 => VesselType.PleasureCraft,
                            >= 40 and <= 49 => VesselType.HighSpeedCraft,
                            50 => VesselType.PilotVessel,
                            51 => VesselType.SearchAndRescue,
                            52 => VesselType.Tug,
                            53 => VesselType.PortTender,
                            55 => VesselType.LawEnforcement,
                            58 => VesselType.MedicalTransport,
                            >= 60 and <= 69 => VesselType.Passenger,
                            >= 70 and <= 79 => VesselType.Cargo,
                            >= 80 and <= 89 => VesselType.Tanker,
                            >= 90 and <= 99 => VesselType.Other,
                            _ => VesselType.Unknown
                        };

                        var vessel = new Vessel
                        {
                            MMSI = metaData.MMSI,
                            IMO = shipStaticData.ImoNumber,
                            ShipName = metaData.ShipName,
                            Callsign = shipStaticData.Callsign,
                            Latitude = metaData.Latitude,
                            Longitude = metaData.Longitude,
                            Received = DateTime.UtcNow,
                            TypeId = shipStaticData.Type,
                            Type = shipType.ToString()
                        };

                        var allowedShipTypes = new List<string>()
                        {
                            "HighSpeedCraft",
                            "PilotVessel",
                            "SearchAndRescue",
                            "Tug",
                            "PortTender",
                            "LawEnforcement",
                            "Tanker",
                            "Cargo"
                        };

                        if (allowedShipTypes.Contains(vessel.Type))
                        {
                            if (existingVessel != null)
                            {
                                vessel.Id = existingVessel.Id;
                                await client.From<Vessel>().Update(vessel, cancellationToken: token);
                                Console.WriteLine($"Updated Ship: {metaData.ShipName} with new position and details.");
                            }
                            else
                            {
                                await client.From<Vessel>().Insert(vessel, cancellationToken: token);
                                Console.WriteLine($"New Ship Detected: {metaData.ShipName}");
                            }
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}. Retrying in 5 seconds...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}. Retrying in 5 seconds...");
            }
            finally
            {
                await Task.Delay(5000, token);
            }
        }
    }
}
