namespace AIS_Feed.Models.AIS;

public class Root
{
    public Message Message { get; set; }
    public string MessageType { get; set; }
    public MetaData MetaData { get; set; }
}