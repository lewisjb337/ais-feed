using Newtonsoft.Json;

namespace AIS_Feed.Models.Scraper;

public class VoyageData
{
    [JsonProperty("Navigation Status")]
    public string Status { get; set; } = string.Empty;
        
    [JsonProperty("Callsign")]
    public string Callsign { get; set; } = string.Empty;
}