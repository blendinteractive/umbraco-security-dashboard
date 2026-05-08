namespace Umbraco.SecurityDashboard.Services;

public interface IGitHubAdvisoryClient
{
    Task<IReadOnlyList<GitHubAdvisory>> GetUmbracoAdvisoriesAsync(IEnumerable<string> packageNames, CancellationToken cancellationToken = default);
}
