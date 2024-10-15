namespace AIS_Feed.Models.AIS;

public class MetaData
{
    public long MMSI { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime TimeUtc { get; set; }
}