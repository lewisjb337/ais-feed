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
    [Column("IMO")]
    public long IMO { get; set; }
    [Column("ShipName")]
    public string ShipName { get; set; } = string.Empty;
    [Column("Callsign")]
    public string Callsign { get; set; } = string.Empty;
    [Column("ShipType")]
    public string ShipType { get; set; } = string.Empty;
    [Column("Status")]
    public string Status { get; set; } = string.Empty;
    [Column("Flag")]
    public string Flag { get; set; } = string.Empty;
    [Column("YearBuilt")]
    public string YearBuilt { get; set; } = string.Empty;
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