namespace Umbraco.SecurityDashboard.Services;

public sealed class GitHubAdvisory
{
    public string GhsaId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
    public string HtmlUrl { get; init; } = string.Empty;
    public IReadOnlyList<GitHubAdvisoryVulnerability> Vulnerabilities { get; init; } = [];
}

public sealed class GitHubAdvisoryVulnerability
{
    public string PackageName { get; init; } = string.Empty;
    public string VulnerableVersionRange { get; init; } = string.Empty;
}
