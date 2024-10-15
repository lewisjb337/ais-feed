using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AIS_Feed.Models.AIS;

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