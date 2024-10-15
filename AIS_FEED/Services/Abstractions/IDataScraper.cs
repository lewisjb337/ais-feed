namespace AIS_FEED.Services.Abstractions;

public interface IDataScraper
{
    Task<string> ScrapeVesselData(string url);
    Task<string> ScrapeVoyageData(string url);
}