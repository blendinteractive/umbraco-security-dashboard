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
        if (_settings.Value.Enabled)
            return Task.FromResult(new ExposureCheckResult(ExposureVerdict.Vulnerable, null));

        return Task.FromResult(new ExposureCheckResult(ExposureVerdict.Mitigated, "Content Delivery API is disabled"));
    }
}
