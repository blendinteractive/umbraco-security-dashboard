using Umbraco.SecurityDashboard.Models.Db;

namespace Umbraco.SecurityDashboard.Services;

public interface IMitigationRepository
{
    Task<IReadOnlyList<ManualMitigationRecord>> GetAllMitigationsAsync();
    Task CreateMitigationAsync(ManualMitigationRecord record);
    Task<bool> DeleteMitigationAsync(string ghsaId);
}
