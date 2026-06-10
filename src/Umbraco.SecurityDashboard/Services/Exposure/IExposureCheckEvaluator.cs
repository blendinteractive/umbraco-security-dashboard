namespace Umbraco.SecurityDashboard.Services.Exposure;

public interface IExposureCheckEvaluator
{
    Task<ExposureEvaluationResult> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);
}
