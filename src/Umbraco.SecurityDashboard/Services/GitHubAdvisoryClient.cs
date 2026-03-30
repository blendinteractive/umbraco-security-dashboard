using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Umbraco.SecurityDashboard.Services;

public class GitHubAdvisoryClient : IGitHubAdvisoryClient
{
    private const string HttpClientName = "SecurityDashboard.GitHub";
    private const string BaseUrl = "https://api.github.com/advisories?ecosystem=nuget&per_page=100";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubAdvisoryClient> _logger;

    public GitHubAdvisoryClient(IHttpClientFactory httpClientFactory, ILogger<GitHubAdvisoryClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GitHubAdvisory>> GetUmbracoAdvisoriesAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var results = new List<GitHubAdvisory>();
        string? url = BaseUrl;
        var pageNumber = 0;

        _logger.LogDebug("Security Dashboard: fetching GitHub advisories from {Url}", url);

        while (url is not null)
        {
            pageNumber++;
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Security Dashboard: fetched page {Page} of GitHub advisories.", pageNumber);

            var page = await response.Content.ReadFromJsonAsync<GitHubAdvisoryApiResponse[]>(
                cancellationToken: cancellationToken) ?? [];

            foreach (var item in page)
            {
                var umbracoVulnerabilities = item.Vulnerabilities
                    .Where(v => v.Package?.Name?.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase) == true)
                    .Select(v => new GitHubAdvisoryVulnerability
                    {
                        PackageName = v.Package!.Name!,
                        VulnerableVersionRange = v.VulnerableVersionRange ?? string.Empty,
                    })
                    .ToList();

                if (umbracoVulnerabilities.Count > 0)
                {
                    results.Add(new GitHubAdvisory
                    {
                        GhsaId = item.GhsaId ?? string.Empty,
                        Summary = item.Summary ?? string.Empty,
                        Severity = CapitalizeSeverity(item.Severity),
                        PublishedAt = item.PublishedAt ?? DateTime.UtcNow,
                        HtmlUrl = item.HtmlUrl ?? string.Empty,
                        Vulnerabilities = umbracoVulnerabilities,
                    });
                }
            }

            url = ParseNextLinkHeader(response.Headers);
        }

        _logger.LogInformation(
            "Security Dashboard: fetched {TotalCount} Umbraco advisories across {Pages} page(s).",
            results.Count, pageNumber);

        return results;
    }

    private static string CapitalizeSeverity(string? severity) =>
        severity switch
        {
            "critical" => "Critical",
            "high" => "High",
            "moderate" => "Moderate",
            "low" => "Low",
            _ => severity ?? string.Empty,
        };

    private static string? ParseNextLinkHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
    {
        if (!headers.TryGetValues("Link", out var linkValues))
            return null;

        foreach (var linkValue in linkValues)
        {
            foreach (var part in linkValue.Split(','))
            {
                var segments = part.Trim().Split(';');
                if (segments.Length != 2)
                    continue;

                var rel = segments[1].Trim();
                if (!rel.Equals("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
                    continue;

                var url = segments[0].Trim().TrimStart('<').TrimEnd('>');
                return url;
            }
        }

        return null;
    }

    // Internal API response shape for JSON deserialization
    private sealed class GitHubAdvisoryApiResponse
    {
        [JsonPropertyName("ghsa_id")]
        public string? GhsaId { get; init; }

        [JsonPropertyName("summary")]
        public string? Summary { get; init; }

        [JsonPropertyName("severity")]
        public string? Severity { get; init; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; init; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; init; }

        [JsonPropertyName("vulnerabilities")]
        public GitHubVulnerabilityApiResponse[] Vulnerabilities { get; init; } = [];
    }

    private sealed class GitHubVulnerabilityApiResponse
    {
        [JsonPropertyName("package")]
        public GitHubPackageApiResponse? Package { get; init; }

        [JsonPropertyName("vulnerable_version_range")]
        public string? VulnerableVersionRange { get; init; }
    }

    private sealed class GitHubPackageApiResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
