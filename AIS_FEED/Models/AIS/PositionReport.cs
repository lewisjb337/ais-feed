namespace AIS_Feed.Models.AIS;

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