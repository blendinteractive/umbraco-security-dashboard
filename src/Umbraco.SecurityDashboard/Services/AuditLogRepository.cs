using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IScopeProvider _scopeProvider;

    public AuditLogRepository(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public Task AppendAsync(AuditLogRecord record)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Insert(record);
        scope.Complete();
        return Task.CompletedTask;
    }

    public Task<AuditLogPage> GetPagedAsync(int skip, int take)
    {
        using var scope = _scopeProvider.CreateScope();
        var entries = scope.Database.Fetch<AuditLogRecord>(
            (long)skip, (long)take,
            "SELECT * FROM SecurityDashboard_AuditLog ORDER BY Timestamp DESC");
        var total = scope.Database.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM SecurityDashboard_AuditLog");
        scope.Complete();
        return Task.FromResult(new AuditLogPage(entries, total));
    }

    public Task<AuditLogSummary> GetWebhookSummaryAsync()
    {
        using var scope = _scopeProvider.CreateScope();

        var noticeRows = scope.Database.Fetch<AuditLogRecord>(
            0L, 1L,
            "SELECT * FROM SecurityDashboard_AuditLog WHERE OverallStatus = 'Vulnerable' ORDER BY Timestamp DESC");
        var lastNotice = noticeRows.FirstOrDefault()?.Timestamp;

        const string resolvedSql = """
            SELECT sub.Timestamp, sub.ActionType, sub.ActorName
            FROM (
                SELECT Timestamp, ActionType, ActorName, OverallStatus,
                       LAG(OverallStatus, 1, NULL) OVER (ORDER BY Timestamp) AS PrevStatus
                FROM SecurityDashboard_AuditLog
            ) sub
            WHERE sub.OverallStatus IN ('Mitigated', 'Safe')
              AND sub.PrevStatus = 'Vulnerable'
            ORDER BY sub.Timestamp DESC
            """;
        var resolvedRows = scope.Database.Fetch<TransitionEntry>(0L, 1L, resolvedSql);
        var resolved = resolvedRows.FirstOrDefault();

        scope.Complete();
        return Task.FromResult(new AuditLogSummary(
            lastNotice,
            resolved?.Timestamp,
            resolved?.ActionType,
            resolved?.ActorName));
    }

    private class TransitionEntry
    {
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? ActorName { get; set; }
    }
}
