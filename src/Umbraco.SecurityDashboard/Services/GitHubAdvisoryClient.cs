using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Umbraco.SecurityDashboard.Services;

public class GitHubAdvisoryClient : IGitHubAdvisoryClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubAdvisoryClient> _logger;

    public GitHubAdvisoryClient(IHttpClientFactory httpClientFactory, ILogger<GitHubAdvisoryClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GitHubAdvisory>> GetUmbracoAdvisoriesAsync(IEnumerable<string> packageNames, CancellationToken cancellationToken = default)
    {
        var names = packageNames.ToList();
        if (names.Count == 0)
            return [];

        var affects = string.Join("&", names.Select(p => $"affects[]={Uri.EscapeDataString(p)}"));
        var url = $"https://api.github.com/advisories?ecosystem=nuget&per_page=100&{affects}";

        var client = _httpClientFactory.CreateClient("GitHubAdvisories");
        var allAdvisories = new List<GitHubAdvisory>();

        while (url != null)
        {
            _logger.LogDebug("Fetching GitHub advisories page: {Url}", url);

            using var response = await client.GetAsync(url, cancellationToken);

            if ((int)response.StatusCode == 429)
            {
                _logger.LogWarning("GitHub API rate limit exceeded when fetching advisories.");
                break;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub API returned {StatusCode} when fetching advisories.", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var advisories = await response.Content.ReadFromJsonAsync<List<GitHubAdvisory>>(cancellationToken: cancellationToken);
            if (advisories != null)
                allAdvisories.AddRange(advisories);

            url = ParseNextLink(response.Headers.TryGetValues("Link", out var linkHeaders)
                ? string.Join(", ", linkHeaders)
                : null);
        }

        return allAdvisories;
    }

    private static string? ParseNextLink(string? linkHeader)
    {
        if (string.IsNullOrEmpty(linkHeader))
            return null;

        var match = Regex.Match(linkHeader, @"<([^>]+)>;\s*rel=""next""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
