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
}
