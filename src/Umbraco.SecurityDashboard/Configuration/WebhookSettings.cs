namespace Umbraco.SecurityDashboard.Configuration;

public class WebhookSettings
{
    public string? SiteUrl { get; set; }
    public string? EndpointUrl { get; set; }
    public string? Secret { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}
