using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.SecurityDashboard.Scheduling;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Composers;

public class SecurityDashboardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Foundational services
        builder.Services.AddScoped<IVulnerabilityCheckRepository, VulnerabilityCheckRepository>();
        builder.Services.AddSingleton<IGitHubAdvisoryClient, GitHubAdvisoryClient>();
        builder.Services.AddSingleton<IInstalledPackageProvider, InstalledPackageProvider>();

        // SecurityDashboardMigrationPlan is IDiscoverable — Umbraco auto-discovers PackageMigrationPlans.

        // Named HTTP client for GitHub Advisory API
        builder.Services.AddHttpClient("SecurityDashboard.GitHub", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Umbraco-SecurityDashboard/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });

        // Phase 3 services
        builder.Services.AddScoped<IVulnerabilityService, VulnerabilityService>();
        builder.Services.AddRecurringBackgroundJob<VulnerabilityCheckTask>();
    }
}
