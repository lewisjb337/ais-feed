using AIS_FEED.Services.Abstractions;
using HtmlAgilityPack;

namespace AIS_FEED.Services;

public class DataScraper : IDataScraper
{
    public async Task<string> ScrapeVesselData(string url)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        var response = await client.GetStringAsync(url);
            
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        var tableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'tpt1')]");

        if (tableNode == null)
        {
            throw new Exception("Vessel Particulars table not found.");
        }

        var rows = tableNode.SelectNodes(".//tr");
        if (rows == null || rows.Count == 0)
        {
            throw new Exception("No rows found in the Vessel Particulars table.");
        }

        var particulars = rows
            .Select(row => row.SelectNodes("td"))
            .Where(cells => cells is { Count: 2 })
            .Aggregate("", (current, cells) => current + $"{cells[0].InnerText.Trim()}: {cells[1].InnerText.Trim()}\n");

        return particulars.Trim();
    }

    public async Task<string> ScrapeVoyageData(string url)
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

        var navigationStatus = string.Empty;
        var callsign = string.Empty;

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
                        navigationStatus = value;
                        break;
                    case "Callsign":
                        callsign = value;
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(navigationStatus) || string.IsNullOrEmpty(callsign))
        {
            throw new Exception("Required data (Navigation Status or Callsign) not found.");
        }

        return $"Navigation Status: {navigationStatus}\nCallsign: {callsign}";
    }
}