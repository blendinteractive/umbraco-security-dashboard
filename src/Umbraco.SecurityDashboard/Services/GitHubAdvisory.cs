using System.Text.Json.Serialization;

namespace Umbraco.SecurityDashboard.Services;

public class GitHubAdvisory
{
    [JsonPropertyName("ghsa_id")]
    public string GhsaId { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("vulnerabilities")]
    public List<GitHubVulnerability> Vulnerabilities { get; set; } = [];
}

public class GitHubVulnerability
{
    [JsonPropertyName("package")]
    public GitHubPackage Package { get; set; } = new();

    [JsonPropertyName("vulnerable_version_range")]
    public string? VulnerableVersionRange { get; set; }

    [JsonPropertyName("first_patched_version")]
    public string? FirstPatchedVersion { get; set; }
}

public class GitHubPackage
{
    [JsonPropertyName("ecosystem")]
    public string Ecosystem { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
