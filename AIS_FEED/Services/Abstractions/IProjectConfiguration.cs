using AIS_Feed.Models.Configuration;

namespace AIS_FEED.Services.Abstractions;

public interface IProjectConfiguration
{
    SupabaseConfig GetSupabaseConfig();
    AisStreamConfig GetAisStreamConfig();
}