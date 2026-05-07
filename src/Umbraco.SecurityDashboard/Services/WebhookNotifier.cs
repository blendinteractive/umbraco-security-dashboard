using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.SecurityDashboard.Configuration;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Models.Webhook;

namespace Umbraco.SecurityDashboard.Services;

public class WebhookNotifier : IWebhookNotifier
{
    private readonly WebhookSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookNotifier> _logger;
    private readonly bool _dispatchEnabled;

    public WebhookNotifier(
        IOptions<SecurityDashboardSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookNotifier> logger)
    {
        _settings = settings.Value.Webhook;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        if (_settings.TimeoutSeconds <= 0)
            _logger.LogWarning(
                "WebhookSettings.TimeoutSeconds is {TimeoutSeconds}, which is invalid; defaulting to 10 seconds.",
                _settings.TimeoutSeconds);

        var endpointValid = !string.IsNullOrWhiteSpace(_settings.EndpointUrl)
            && Uri.TryCreate(_settings.EndpointUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        var siteUrlValid = !string.IsNullOrWhiteSpace(_settings.SiteUrl);

        if (!endpointValid)
        {
            _logger.LogWarning("Webhook dispatch disabled: EndpointUrl is absent or not a valid absolute http/https URL.");
        }
        else if (!siteUrlValid)
        {
            _logger.LogWarning("Webhook dispatch disabled: SiteUrl is absent or empty.");
        }
        else
        {
            _dispatchEnabled = true;
        }
    }

    public async Task NotifyAsync(
        string overallStatus,
        DateTime checkedAt,
        IReadOnlyList<AdvisoryRecord> affectedAdvisories,
        CancellationToken cancellationToken = default)
    {
        if (!_dispatchEnabled)
            return;

        var packages = affectedAdvisories
            .Select(a => new WebhookAffectedPackage(a.PackageName, a.InstalledVersion, a.AdvisoryUrl, a.Severity))
            .ToArray();

        var payload = new WebhookPayload(_settings.SiteUrl!, overallStatus, checkedAt, packages);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var client = _httpClientFactory.CreateClient("WebhookNotifier");

        var request = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointUrl)
        {
            Content = JsonContent.Create(payload, options: options)
        };

        if (!string.IsNullOrEmpty(_settings.Secret))
            request.Headers.Add("X-Webhook-Secret", _settings.Secret);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Webhook delivered successfully to {EndpointUrl}.", _settings.EndpointUrl);
            else
                _logger.LogError("Webhook delivery failed. Endpoint: {EndpointUrl}, Status: {StatusCode}.", _settings.EndpointUrl, (int)response.StatusCode);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Webhook request timed out. Endpoint: {EndpointUrl}.", _settings.EndpointUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Webhook request failed due to a network error. Endpoint: {EndpointUrl}.", _settings.EndpointUrl);
        }
    }
}
