namespace Umbraco.SecurityDashboard.Services;

public class DuplicateMitigationException : Exception
{
    public string GhsaId { get; }

    public DuplicateMitigationException(string ghsaId)
        : base($"Advisory {ghsaId} is already manually mitigated.")
    {
        GhsaId = ghsaId;
    }
}
