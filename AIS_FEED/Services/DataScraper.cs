using AIS_Feed.Models.Scraper;
using AIS_FEED.Services.Abstractions;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AIS_FEED.Services;

public class DataScraper : IDataScraper
{
    public async Task<VesselData> ScrapeVesselData(string url)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        var response = await client.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        var tableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'tpt1')]");

        if (tableNode == null)
        {
            return new VesselData();
        }

        var rows = tableNode.SelectNodes(".//tr");
        if (rows == null || rows.Count == 0)
        {
            throw new Exception("No rows found in the Vessel Particulars table.");
        }

        var particulars = new Dictionary<string, string>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            
            if (cells is not { Count: 2 }) continue;
            
            var key = cells[0].InnerText.Trim();
            var value = cells[1].InnerText.Trim();
            particulars[key] = value;
        }

        var jsonParticulars = JsonConvert.SerializeObject(particulars);
    
        var vesselData = JsonConvert.DeserializeObject<VesselData>(jsonParticulars);

        return vesselData!;
    }

    public async Task<VoyageData> ScrapeVoyageData(string url)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        var response = await client.GetStringAsync(url);
    
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        var voyageDataSection = htmlDoc.DocumentNode.SelectSingleNode("//h2[text()='Voyage Data']/following-sibling::div//table[contains(@class, 'aparams')]");

        if (voyageDataSection == null)
        {
            throw new Exception("Voyage Data table not found.");
        }

        var rows = voyageDataSection.SelectNodes(".//tr");
        if (rows == null || rows.Count == 0)
        {
            throw new Exception("No rows found in the Voyage Data table.");
        }

        var voyageData = new VoyageData();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells is { Count: 2 })
            {
                var label = cells[0].InnerText.Trim();
                var value = cells[1].InnerText.Trim();

                switch (label)
                {
                    case "Navigation Status":
                        voyageData.Status = value;
                        break;
                    case "Callsign":
                        voyageData.Callsign = value;
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(voyageData.Status) || string.IsNullOrEmpty(voyageData.Callsign))
        {
            throw new Exception("Required data (Navigation Status or Callsign) not found.");
        }

        return voyageData;
    }
}