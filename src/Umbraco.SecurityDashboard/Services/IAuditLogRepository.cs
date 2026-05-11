using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public interface IAuditLogRepository
{
    Task AppendAsync(AuditLogRecord record);
    Task<AuditLogPage> GetPagedAsync(int skip, int take);
    Task<AuditLogSummary> GetWebhookSummaryAsync();
}

public record AuditLogPage(IReadOnlyList<AuditLogRecord> Entries, int TotalCount);

public record AuditLogSummary(
    DateTime? LastNotice,
    DateTime? LastResolved,
    string? ResolutionType,
    string? ResolvedBy);
