using Newtonsoft.Json;

namespace AIS_Feed.Models.Scraper
{
    public class VesselData
    {
        [JsonProperty("IMO number")]
        public long IMO { get; set; }
        
        [JsonProperty("Vessel Name")]
        public string VesselName { get; set; } = string.Empty;
        
        [JsonProperty("Ship Type")]
        public string ShipType { get; set; } = string.Empty;
        
        [JsonProperty("Flag")]
        public string Flag { get; set; } = string.Empty;
        
        [JsonProperty("Year of Build")]
        public string YearBuilt { get; set; } = string.Empty;
    }
}