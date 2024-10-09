using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AIS_Feed;

internal abstract class Program
{
    public static async Task Main(string[] args)
    {
        const string url = "https://voszrojknbodyrdwmzab.supabase.co"; 
        const string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZvc3pyb2prbmJvZHlyZHdtemFiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mjc0MjU1NzQsImV4cCI6MjA0MzAwMTU3NH0.sH5x1FP3v1FLxsibPAFHO-nwWwIQ-JU9-nt5ZmId49Y";

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        var client = new Client(url, key, options);
        await client.InitializeAsync();

        var source = new CancellationTokenSource();
        var token = source.Token;

        while (true) // Keep trying to connect
        {
            using var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), token);
                await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"APIKey\": \"24a5b8b04ce3b1c7637fda99cf22271c25a47bb8\", \"BoundingBoxes\": [[[-90.0, -180.0], [90.0, 180.0]]]}")), WebSocketMessageType.Text, true, token);

                var buffer = new byte[4096];
                
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                        break; // Exit the inner loop
                    }
                    else
                    {
                        var content = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        if (!content.Contains("PositionReport")) continue;

                        var deserializedObject = JsonConvert.DeserializeObject<Root>(content);
                        var metaData = deserializedObject?.MetaData;
                        var positionReport = deserializedObject?.Message?.PositionReport;

                        if (metaData != null && positionReport != null)
                        {
                            // Retrieve all records from the Vessels table
                            var vesselResult = await client.From<MetaData>().Get(token);
                            var existingVessels = vesselResult.Models;

                            // Find the existing vessel by MMSI
                            var existingVessel = existingVessels.FirstOrDefault(v => v.MMSI == metaData.MMSI);
                            
                            var vessel = new MetaData
                            {
                                MMSI = metaData.MMSI,
                                MMSI_String = metaData.MMSI_String,
                                ShipName = metaData.ShipName,
                                Latitude = metaData.Latitude,
                                Longitude = metaData.Longitude,
                                TimeUtc = metaData.TimeUtc,
                                Cog = positionReport.Cog,
                                Raim = positionReport.Raim,
                                RateOfTurn = positionReport.RateOfTurn,
                                Sog = positionReport.Sog,
                                TrueHeading = positionReport.TrueHeading
                            };

                            if (existingVessel != null)
                            {
                                // Update the existing vessel record
                                vessel.Id = existingVessel.Id; // Assign the Id for updating
                                await client.From<MetaData>().Update(vessel, cancellationToken: token);
                                Console.WriteLine($"Updated Ship: {metaData.ShipName} with new position and details.");
                            }
                            else
                            {
                                // Insert new vessel record
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
                await Task.Delay(5000); // Wait before reconnecting
            }
        }
    }

    public class PositionReport
    {
        public double Cog { get; set; }
        public int CommunicationState { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MessageID { get; set; }
        public int NavigationalStatus { get; set; }
        public bool PositionAccuracy { get; set; }
        public bool Raim { get; set; }
        public int RateOfTurn { get; set; }
        public int RepeatIndicator { get; set; }
        public double Sog { get; set; }
        public int Spare { get; set; }
        public int SpecialManoeuvreIndicator { get; set; }
        public int Timestamp { get; set; }
        public int TrueHeading { get; set; }
        public long UserID { get; set; }
        public bool Valid { get; set; }
    }

    public class Message
    {
        public PositionReport PositionReport { get; set; }
    }

    [Table("Vessels")]
    public class MetaData : BaseModel
    {
        [PrimaryKey("Id")]
        public int Id { get; set; }
        [Column("MMSI")]
        public long MMSI { get; set; }
        [Column("MMSI_String")]
        public string MMSI_String { get; set; }
        [Column("ShipName")]
        public string ShipName { get; set; }
        [Column("Latitude")]
        public double Latitude { get; set; }
        [Column("Longitude")]
        public double Longitude { get; set; }
        [Column("TimeUtc")]
        public DateTime TimeUtc { get; set; }
        [Column("Cog")]
        public double Cog { get; set; }
        [Column("Raim")]
        public bool Raim { get; set; }
        [Column("RateOfTurn")]
        public int RateOfTurn { get; set; }
        [Column("Sog")]
        public double Sog { get; set; }
        [Column("TrueHeading")]
        public int TrueHeading { get; set; }
    }

    public class Root
    {
        public Message Message { get; set; }
        public string MessageType { get; set; }
        public MetaData MetaData { get; set; }
    }
}
