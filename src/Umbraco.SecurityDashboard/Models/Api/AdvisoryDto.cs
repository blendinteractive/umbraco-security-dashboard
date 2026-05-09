namespace Umbraco.SecurityDashboard.Models.Api;

public class AdvisoryPackageDto
{
    public string PackageName { get; set; } = string.Empty;
    public string AffectedVersionRange { get; set; } = string.Empty;
    public string? InstalledVersion { get; set; }

    /// <summary>Affected | NotAffected | Unknown</summary>
    public string AffectedStatus { get; set; } = string.Empty;
}

public class AdvisoryDto
{
    public string GhsaId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>Critical | High | Moderate | Low</summary>
    public string Severity { get; set; } = string.Empty;

    public string AdvisoryUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }

    /// <summary>Consolidated status: Affected | NotAffected | Unknown</summary>
    public string AffectedStatus { get; set; } = string.Empty;

    public List<AdvisoryPackageDto> Packages { get; set; } = [];
}
