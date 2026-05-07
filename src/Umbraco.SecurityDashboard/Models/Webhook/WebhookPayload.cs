namespace Umbraco.SecurityDashboard.Models.Webhook;

public record WebhookPayload(
    string SiteUrl,
    string Status,
    DateTime CheckedAt,
    WebhookAffectedPackage[] AffectedPackages);
