namespace Umbraco.SecurityDashboard.Services;

public interface IInstalledPackageProvider
{
    IReadOnlyDictionary<string, string> GetInstalledUmbracoPackages();
}
