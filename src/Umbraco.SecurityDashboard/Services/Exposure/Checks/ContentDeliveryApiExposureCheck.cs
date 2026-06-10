using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.SecurityDashboard.Services.Exposure.Checks;

public class ContentDeliveryApiExposureCheck : IExposureCheck
{
    private readonly IOptions<DeliveryApiSettings> _settings;

    public ContentDeliveryApiExposureCheck(IOptions<DeliveryApiSettings> settings)
    {
        _settings = settings;
    }

    public string Keyword => "Content Delivery API";

    public Task<ExposureCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var verdict = _settings.Value.Enabled ? ExposureVerdict.Vulnerable : ExposureVerdict.Mitigated;
        return Task.FromResult(new ExposureCheckResult(verdict, null));
    }
}
