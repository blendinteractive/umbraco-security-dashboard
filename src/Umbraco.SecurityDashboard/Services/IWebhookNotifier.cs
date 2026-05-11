using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public interface IWebhookNotifier
{
    Task NotifyAsync(
        string overallStatus,
        DateTime checkedAt,
        IReadOnlyList<AdvisoryRecord> affectedAdvisories,
        string? previousStatus = null,
        CancellationToken cancellationToken = default);
}
