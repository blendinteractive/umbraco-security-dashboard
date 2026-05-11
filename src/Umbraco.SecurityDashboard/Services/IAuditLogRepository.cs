using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public interface IAuditLogRepository
{
    Task AppendAsync(AuditLogRecord record);
    Task<AuditLogPage> GetPagedAsync(int skip, int take);
}

public record AuditLogPage(IReadOnlyList<AuditLogRecord> Entries, int TotalCount);
