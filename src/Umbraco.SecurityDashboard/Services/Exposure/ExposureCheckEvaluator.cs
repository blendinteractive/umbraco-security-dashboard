using Microsoft.Extensions.Logging;

namespace Umbraco.SecurityDashboard.Services.Exposure;

public class ExposureCheckEvaluator : IExposureCheckEvaluator
{
    private readonly IEnumerable<IExposureCheck> _checks;
    private readonly ILogger<ExposureCheckEvaluator> _logger;

    public ExposureCheckEvaluator(IEnumerable<IExposureCheck> checks, ILogger<ExposureCheckEvaluator> logger)
    {
        _checks = checks;
        _logger = logger;
    }

    public async Task<ExposureEvaluationResult> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default)
    {
        var keywordSet = new HashSet<string>(keywords);
        if (keywordSet.Count == 0)
            return new ExposureEvaluationResult(nameof(ExposureVerdict.Vulnerable), null);

        var matchingChecks = _checks.Where(c => keywordSet.Contains(c.Keyword)).ToList();
        if (matchingChecks.Count == 0)
            return new ExposureEvaluationResult(nameof(ExposureVerdict.Vulnerable), null);

        var resultTasks = matchingChecks.Select(check => RunSafeAsync(check, cancellationToken));
        var results = await Task.WhenAll(resultTasks);

        var worst = results.Max(r => r.Verdict);

        string? description = null;
        if (worst == ExposureVerdict.Mitigated)
        {
            var parts = results
                .Where(r => r.Verdict == ExposureVerdict.Mitigated && r.MitigationDescription != null)
                .Select(r => r.MitigationDescription!);
            var joined = string.Join("; ", parts);
            description = joined.Length > 0 ? joined : "Mitigated by exposure check";
        }

        return new ExposureEvaluationResult(worst.ToString(), description);
    }

    private async Task<ExposureCheckResult> RunSafeAsync(IExposureCheck check, CancellationToken cancellationToken)
    {
        try
        {
            return await check.CheckAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exposure check '{Keyword}' threw an exception; treating as Vulnerable.", check.Keyword);
            return new ExposureCheckResult(ExposureVerdict.Vulnerable);
        }
    }
}
