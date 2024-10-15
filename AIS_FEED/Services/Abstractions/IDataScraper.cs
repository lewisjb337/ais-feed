using AIS_Feed.Models.Scraper;

namespace AIS_FEED.Services.Abstractions;

public interface IDataScraper
{
    Task<VesselData> ScrapeVesselData(string url);
    Task<VoyageData> ScrapeVoyageData(string url);
}