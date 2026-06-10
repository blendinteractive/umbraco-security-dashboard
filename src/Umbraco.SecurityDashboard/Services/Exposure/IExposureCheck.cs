namespace Umbraco.SecurityDashboard.Services.Exposure;

public enum ExposureVerdict
{
    NotAffected = 0,
    Mitigated = 1,
    Vulnerable = 2
}

public interface IExposureCheck
{
    string Keyword { get; }
    Task<ExposureCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}
