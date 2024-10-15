using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AIS_Feed.Models;

[Table("Vessels")]
public class Vessel : BaseModel
{
    [PrimaryKey("Id")]
    public int Id { get; set; }
    [Column("MMSI")]
    public long MMSI { get; set; }
    [Column("IMO")]
    public long IMO { get; set; }
    [Column("ShipName")]
    public string ShipName { get; set; } = string.Empty;
    [Column("CallSign")]
    public string CallSign { get; set; } = string.Empty;
    [Column("Latitude")]
    public double Latitude { get; set; }
    [Column("Longitude")]
    public double Longitude { get; set; }
    [Column("Received")]
    public DateTime Received { get; set; }
    [Column("TypeId")]
    public int TypeId { get; set; }
    [Column("Type")]
    public string Type { get; set; } = string.Empty;
}