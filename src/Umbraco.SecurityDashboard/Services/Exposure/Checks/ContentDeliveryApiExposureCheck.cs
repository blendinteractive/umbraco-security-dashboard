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

    public Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_settings.Value.Enabled ? ExposureVerdict.Vulnerable : ExposureVerdict.Mitigated);
}
