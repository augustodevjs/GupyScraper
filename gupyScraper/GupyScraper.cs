using Newtonsoft.Json.Linq;

namespace gupyScraper;

public class GupyScraper
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _searchLabels;

    public GupyScraper(List<string> searchLabels)
    {
        _httpClient = new HttpClient
        {
            DefaultRequestHeaders =
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 OPR/100.0.0.0 (Edition std-1)" }
                }
        };
        _searchLabels = searchLabels;
    }

    public async Task<List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)>> RequestJobDataAsync()
    {
        var jobData = new List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)>();
        var today = DateTime.UtcNow;
        var sevenDaysAgo = today.AddDays(-7);

        foreach (var label in _searchLabels)
        {
            Console.WriteLine($"Requesting for '{label}'...");
            var url = $"https://portal.api.gupy.io/api/job?name={label}&offset=0&limit=400";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var data = JObject.Parse(response)["data"] as JArray;

                foreach (var job in data)
                {
                    var jobTitle = job["name"]?.ToString();
                    var jobUrl = job["jobUrl"]?.ToString();
                    var datePublished = DateTime.Parse(job["publishedDate"]?.ToString() ?? string.Empty);
                    var workPlaceType = job["workplaceType"]?.ToString();

                    if (datePublished >= sevenDaysAgo && datePublished <= today && !string.IsNullOrEmpty(jobUrl) && !string.IsNullOrEmpty(jobTitle))
                    {
                        jobData.Add((jobTitle, jobUrl, datePublished, workPlaceType));
                    }
                }

                Console.WriteLine($"Found {data.Count} results for '{label}'...");
                await Task.Delay(500);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return jobData;
    }
}
