namespace Umbraco.SecurityDashboard.Models.Webhook;

public record WebhookPayload(
    string SiteUrl,
    string Status,
    string? PreviousStatus,
    DateTime CheckedAt,
    DateTime? NextScheduledCheckAt,
    WebhookAffectedPackage[] AffectedPackages,
    DateTime? LastNotice,
    DateTime? LastResolved,
    string? ResolutionType,
    string? ResolvedBy);
