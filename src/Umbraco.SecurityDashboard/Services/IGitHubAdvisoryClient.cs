namespace Umbraco.SecurityDashboard.Services;

public interface IGitHubAdvisoryClient
{
    Task<IReadOnlyList<GitHubAdvisory>> GetUmbracoAdvisoriesAsync(CancellationToken cancellationToken);
}
