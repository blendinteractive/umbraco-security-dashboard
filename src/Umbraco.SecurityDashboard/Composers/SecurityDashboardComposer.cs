using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using Umbraco.SecurityDashboard.Configuration;
using Umbraco.SecurityDashboard.Extensions;
using Umbraco.SecurityDashboard.Scheduling;
using Umbraco.SecurityDashboard.Services;
using Umbraco.SecurityDashboard.Services.Exposure;
using Umbraco.SecurityDashboard.Services.Exposure.Checks;

namespace Umbraco.SecurityDashboard.Composers;

public class SecurityDashboardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Settings
        builder.Services.Configure<SecurityDashboardSettings>(
            builder.Config.GetSection(SecurityDashboardSettings.SectionName));

        // Data access
        builder.Services.AddSingleton<IVulnerabilityCheckRepository, VulnerabilityCheckRepository>();

        // GitHub advisory client
        builder.Services.AddSingleton<IGitHubAdvisoryClient, GitHubAdvisoryClient>();

        // Installed package provider
        builder.Services.AddSingleton<IInstalledPackageProvider, InstalledPackageProvider>();

        // Exposure checks
        builder.AddExposureCheck<NonAdminUsersExposureCheck>();
        builder.AddExposureCheck<ContentDeliveryApiExposureCheck>();
        builder.Services.AddSingleton<IExposureCheckEvaluator, ExposureCheckEvaluator>();

        // Vulnerability service
        builder.Services.AddSingleton<IVulnerabilityService, VulnerabilityService>();

        // Named HTTP client for GitHub Advisory API
        builder.Services.AddHttpClient("GitHubAdvisories", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Umbraco-SecurityDashboard/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });

        // Webhook notifier
        builder.Services.AddSingleton<IWebhookNotifier, WebhookNotifier>();

        var timeoutStr = builder.Config
            .GetSection(SecurityDashboardSettings.SectionName)["Webhook:TimeoutSeconds"];
        if (!int.TryParse(timeoutStr, out var webhookTimeoutSeconds) || webhookTimeoutSeconds <= 0)
            webhookTimeoutSeconds = 10;

        builder.Services.AddHttpClient("WebhookNotifier", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(webhookTimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false
        });

        // Recurring background task for scheduled vulnerability checks
        builder.Services.AddRecurringBackgroundJob<VulnerabilityCheckTask>();

        // Startup check: run immediately if last successful check is older than CheckInterval
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, StartupVulnerabilityCheckHandler>();

        // Database migration plan
        builder.PackageMigrationPlans().Add<Migrations.SecurityDashboardMigrationPlan>();
    }
}
