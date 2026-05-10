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

    public async Task<string> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default)
    {
        var keywordSet = new HashSet<string>(keywords);
        if (keywordSet.Count == 0)
            return nameof(ExposureVerdict.Vulnerable);

        var matchingChecks = _checks.Where(c => keywordSet.Contains(c.Keyword)).ToList();
        if (matchingChecks.Count == 0)
            return nameof(ExposureVerdict.Vulnerable);

        var verdictTasks = matchingChecks.Select(check => RunSafeAsync(check, cancellationToken));
        var verdicts = await Task.WhenAll(verdictTasks);

        var worst = verdicts.Max();
        return worst.ToString();
    }

    private async Task<ExposureVerdict> RunSafeAsync(IExposureCheck check, CancellationToken cancellationToken)
    {
        try
        {
            return await check.CheckAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exposure check '{Keyword}' threw an exception; treating as Vulnerable.", check.Keyword);
            return ExposureVerdict.Vulnerable;
        }
    }
}
