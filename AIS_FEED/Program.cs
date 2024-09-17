using System.Net.WebSockets;
using System.Text;

namespace AIS_FEED;

internal abstract class Program
{
    public static async Task Main(string[] args)
    {
        var source = new CancellationTokenSource();
        var token = source.Token;

        using var ws = new ClientWebSocket();
        
        await ws.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), token);
        await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("{ \"APIKey\": \"24a5b8b04ce3b1c7637fda99cf22271c25a47bb8\", \"BoundingBoxes\": [[[-11.0, 178.0], [30.0, 74.0]]]}")), WebSocketMessageType.Text, true, token);
        
        var buffer = new byte[4096];
        
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
            }
            else
            {
                Console.WriteLine($"Received {Encoding.Default.GetString(buffer, 0, result.Count)}");
            }
        }
    }
}

// Example of position report data
// {
// "Message":{
//     "PositionReport":{
//         "Cog":217.3,
//         "CommunicationState":59916,
//         "Latitude":13.351478333333334,
//         "Longitude":100.601575,
//         "MessageID":3,
//         "NavigationalStatus":1,
//         "PositionAccuracy":false,
//         "Raim":false,
//         "RateOfTurn":0,
//         "RepeatIndicator":0,
//         "Sog":0,
//         "Spare":0,
//         "SpecialManoeuvreIndicator":0,
//         "Timestamp":54,
//         "TrueHeading":338,
//         "UserID":538010505,
//         "Valid":true
//     }
// },
// "MessageType":"PositionReport",
// "MetaData":{
//     "MMSI":538010505,
//     "MMSI_String":538010505,
//     "ShipName":"SAWASDEE ALTAIR",
//     "latitude":13.351478333333334,
//     "longitude":100.601575,
//     "time_utc":"2024-09-17 14:38:55.281038479 +0000 UTC"
// }
// }