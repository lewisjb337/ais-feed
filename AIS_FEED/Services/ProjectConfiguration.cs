using AIS_Feed.Models.Configuration;
using AIS_FEED.Services.Abstractions;
using Microsoft.Extensions.Configuration;

namespace AIS_FEED.Services;

public class ProjectConfiguration(IConfiguration configuration) : IProjectConfiguration
{
    public SupabaseConfig GetSupabaseConfig()
    {
        return new SupabaseConfig
        {
            Url = configuration.GetSection("Supabase")["Url"]!,
            Key = configuration.GetSection("Supabase")["Key"]!
        };
    }   
    
    public AisStreamConfig GetAisStreamConfig()
    {
        return new AisStreamConfig
        {
            Url = configuration.GetSection("AisStream")["Url"]!,
            Key = configuration.GetSection("AisStream")["ApiKey"]!
        };
    }  
}