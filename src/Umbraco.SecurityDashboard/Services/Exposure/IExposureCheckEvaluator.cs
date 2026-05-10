namespace Umbraco.SecurityDashboard.Services.Exposure;

public interface IExposureCheckEvaluator
{
    Task<string> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);
}
