using System.Net.WebSockets;
using System.Text;
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
    private static IDataScraper? dataScraper { get; set; }
    public static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration config = builder.Build();

        configuration = new ProjectConfiguration(config);
        dataScraper = new DataScraper();
        
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

                        if (!content.Contains("PositionReport")) continue;

                        var deserializedObject = JsonConvert.DeserializeObject<Root>(content);
                        var metaData = deserializedObject?.MetaData;
                        var positionReport = deserializedObject?.Message?.PositionReport;

                        if (metaData == null || positionReport == null) continue;
                        
                        var vesselResult = await client.From<MetaData>().Get(token);
                        var existingVessels = vesselResult.Models;

                        var existingVessel = existingVessels.FirstOrDefault(v => v.MMSI == metaData.MMSI);

                        // Below seems to hit block after given time, likely IP blacklist due to spam...
                        var voyageData = await dataScraper!.ScrapeVoyageData($"https://www.vesselfinder.com/vessels/details/{metaData.MMSI}");
                        var vesselData = await dataScraper!.ScrapeVesselData($"https://www.vesselfinder.com/vessels/details/{metaData.MMSI}");
                            
                        var vessel = new MetaData
                        {
                            MMSI = metaData.MMSI,
                            IMO = vesselData.IMO,
                            ShipName = metaData.ShipName,
                            Callsign = voyageData.Callsign,
                            ShipType = vesselData.ShipType,
                            Status = voyageData.Status,
                            Flag = vesselData.Flag,
                            YearBuilt = vesselData.YearBuilt,
                            Latitude = metaData.Latitude,
                            Longitude = metaData.Longitude,
                            TimeUtc = DateTime.UtcNow,
                            Cog = positionReport.Cog,
                            Raim = positionReport.Raim,
                            RateOfTurn = positionReport.RateOfTurn,
                            Sog = positionReport.Sog,
                            TrueHeading = positionReport.TrueHeading
                        };

                        var allowedVesselTypes = new List<string>()
                        {
                            // Cargo
                            "Bulk Carrier",
                            "General Cargo Ship",
                            "Container Ship",
                            "Refrigerated Cargo Ship",
                            "Ro-Ro Cargo Ship",
                            "Vehicles Carrier",
                            "Cement Carrier",
                            "Wood Chips Carrier",
                            "Urea Carrier",
                            "Aggregates Carrier",
                            "Limestone Carrier",
                            "Ore Carrier",
                            "Landing Craft",
                            "Livestock Carrier",
                            "Heavy Load Carrier",
                            "Utility Vessel",
                            "Deck Cargo Ship",
                            "Fish Carrier",
                            "Self-Discharging Bulk Carrier",
                            "Container Ro-Ro Cargo Ship",
                            "Palletised Cargo Ship",
                            "Bulk/Oil/Chemical Carrier",
                            "Bulk/Caustic Soda Carrier",
                            "Bulk/Sulphuric Acid Carrier",
                            "Bulk/Oil Carrier",
                            "Ore/Oil Carrier",
                            "Refined Sugar Carrier",
                            "Powder Carrier",
                            "Nuclear Fuel Carrier",
                            "Barge Carrier",
                            "Trans-Shipment Vessel",
                            // Tanker
                            "Crude Oil Tanker",
                            "Oil Products Tanker",
                            "Chemical/Oil Products Tanker",
                            "LNG Tanker",
                            "LPG Tanker",
                            "Asphalt/Bitumen",
                            "Bunkering Tanker",
                            "FSO/FPSO Oil",
                            "FSO/FPSO Gas",
                            "Chemical Tanker",
                            "Bunkering Tanker (LNG)",
                            "Bunkering Tanker (LNG/OIL)",
                            "Combination Gas Tanker (LNG/LPG)",
                            "Edible Oil Tanker",
                            "Vegetable Oil Tanker",
                            "Wine Tanker",
                            "Fruit juice carrier- refrigerated",
                            "Water Tanker",
                            "CO2 Tanker",
                            "Caprolactam Tanker",
                            "Molasses Tanker",
                            "Waste Disposal Vessel",
                            "Coal/Oil Mixture Tanker",
                            // Other
                            "Crane Ship",
                            "Drilling Ship",
                            "Work/Repair Vessel",
                            "Icebreaker",
                            "Pipe-Layer",
                            "Pipe Burying Vessel",
                            "Cable-Layer",
                            "Cable Repair Ship",
                            "Research Vessel",
                            "Power Station Vessel",
                            "Wind Turbine Installation Vessel",
                            "Rocket Launch Support Ship",
                            "Training Ship",
                            "Fishing Support Vessel",
                            "Supply Vessel",
                            "Offshore Support Vessel",
                            "Offshore Tug/Supply Ship",
                            "Offshore Supply Ship",
                            "Offshore Processing Ship",
                            "Buoy/Lighthouse Vessel",
                            "Production Testing Vessel",
                            "Well-Stimulation Vessel",
                            "Exhibition Vessel",
                            "Wing In Ground"
                        };

                        if (allowedVesselTypes.Contains(vessel.ShipType))
                        {
                            if (existingVessel != null)
                            {
                                vessel.Id = existingVessel.Id;
                                await client.From<MetaData>().Update(vessel, cancellationToken: token);
                                Console.WriteLine($"Updated Ship: {metaData.ShipName} with new position and details.");
                            }
                            else
                            {
                                await client.From<MetaData>().Insert(vessel, cancellationToken: token);
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
