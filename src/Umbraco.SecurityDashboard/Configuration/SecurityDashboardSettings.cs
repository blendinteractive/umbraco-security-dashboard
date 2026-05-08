namespace Umbraco.SecurityDashboard.Configuration;

public class SecurityDashboardSettings
{
    public const string SectionName = "Umbraco:SecurityDashboard";

    /// <summary>
    /// Override installed package versions for testing. Only applied in development environments.
    /// Keys are NuGet package names; values are version strings that replace the detected version.
    /// </summary>
    public Dictionary<string, string> PackageVersionOverrides { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Additional NuGet package IDs (non-Umbraco) to include in vulnerability checks.
    /// Versions are detected automatically from the runtime dependency graph.
    /// </summary>
    public List<string> AdditionalPackageIds { get; set; } = [];

    public WebhookSettings Webhook { get; set; } = new();
}
