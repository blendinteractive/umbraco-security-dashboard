namespace Umbraco.SecurityDashboard.Configuration;

public class SecurityDashboardSettings
{
    public const string SectionName = "Umbraco:SecurityDashboard";

    /// <summary>
    /// Additional NuGet package IDs (non-Umbraco) to include in vulnerability checks.
    /// Versions are detected automatically from the runtime dependency graph.
    /// </summary>
    public List<string> AdditionalPackageIds { get; set; } = [];

    public WebhookSettings Webhook { get; set; } = new();

    /// <summary>
    /// Settings that only apply in development environments.
    /// </summary>
    public DevelopmentSettings Development { get; set; } = new();
}
