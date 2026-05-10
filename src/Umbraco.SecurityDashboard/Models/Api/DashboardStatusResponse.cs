namespace Umbraco.SecurityDashboard.Models.Api;

public class DashboardStatusResponse
{
    /// <summary>Safe | Mitigated | Vulnerable | NeverChecked</summary>
    public string OverallStatus { get; set; } = string.Empty;

    /// <summary>True when last successful check is older than 48 hours.</summary>
    public bool IsStale { get; set; }

    public DateTime? LastSuccessfulCheckAt { get; set; }
    public DateTime? LastCheckAttemptAt { get; set; }
    public bool? LastCheckSucceeded { get; set; }
    public string? LastCheckError { get; set; }
    public DateTime NextScheduledCheckAt { get; set; }
    public int AffectedAdvisoryCount { get; set; }
    public int MitigatedAdvisoryCount { get; set; }

    /// <summary>
    /// Ordered for display: Affected/Unknown entries first,
    /// then NotAffected entries ordered by PublishedAt descending.
    /// </summary>
    public IReadOnlyList<AdvisoryDto> Advisories { get; set; } = [];
}
