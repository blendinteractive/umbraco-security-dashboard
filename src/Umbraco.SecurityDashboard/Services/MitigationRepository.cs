using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public class MitigationRepository : IMitigationRepository
{
    private readonly IScopeProvider _scopeProvider;

    public MitigationRepository(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public Task<IReadOnlyList<ManualMitigationRecord>> GetAllMitigationsAsync()
    {
        using var scope = _scopeProvider.CreateScope();
        var results = scope.Database.Fetch<ManualMitigationRecord>(
            "SELECT * FROM SecurityDashboard_ManualMitigation");
        scope.Complete();
        return Task.FromResult<IReadOnlyList<ManualMitigationRecord>>(results);
    }

    public Task CreateMitigationAsync(ManualMitigationRecord record)
    {
        using var scope = _scopeProvider.CreateScope();
        scope.Database.Insert(record);
        scope.Complete();
        return Task.CompletedTask;
    }

    public Task<bool> DeleteMitigationAsync(string ghsaId)
    {
        using var scope = _scopeProvider.CreateScope();
        var affected = scope.Database.Execute(
            "DELETE FROM SecurityDashboard_ManualMitigation WHERE GhsaId = @0", ghsaId);
        scope.Complete();
        return Task.FromResult(affected > 0);
    }
}
