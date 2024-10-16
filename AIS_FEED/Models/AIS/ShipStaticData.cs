namespace AIS_Feed.Models.AIS;

public class ShipStaticData
{
    public int AisVersion { get; set; }
    public string Callsign { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public Dimension? Dimension { get; set; }
    public bool Dte { get; set; }
    public Eta? Eta { get; set; }
    public int FixType { get; set; }
    public int ImoNumber { get; set; }
    public double MaximumStaticDraught { get; set; }
    public int MessageID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RepeatIndicator { get; set; }
    public bool Spare { get; set; }
    public int Type { get; set; }
    public long UserID { get; set; }
    public bool Valid { get; set; }
}