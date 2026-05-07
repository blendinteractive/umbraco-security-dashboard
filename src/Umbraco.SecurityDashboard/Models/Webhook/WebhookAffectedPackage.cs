namespace Umbraco.SecurityDashboard.Models.Webhook;

public record WebhookAffectedPackage(
    string PackageName,
    string? InstalledVersion,
    string AdvisoryUrl,
    string Severity);
