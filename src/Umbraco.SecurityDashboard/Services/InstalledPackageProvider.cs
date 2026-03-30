using System.Reflection;

namespace Umbraco.SecurityDashboard.Services;

public class InstalledPackageProvider : IInstalledPackageProvider
{
    private static readonly string[] SuffixesToStrip = [".Core", ".Web", ".Infrastructure"];

    public IReadOnlyDictionary<string, string> GetInstalledUmbracoPackages()
    {
        var packages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name is null || !name.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase))
                continue;

            var version = GetVersion(assembly);
            if (version is null)
                continue;

            var rootName = GetRootPackageName(name);

            if (!packages.TryGetValue(rootName, out var existing) ||
                IsHigherVersion(version, existing))
            {
                packages[rootName] = version;
            }
        }

        return packages;
    }

    private static string? GetVersion(Assembly assembly)
    {
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            // Strip build metadata suffix (e.g. "+abc123") for clean version strings
            var plusIndex = informational.IndexOf('+');
            return plusIndex >= 0 ? informational[..plusIndex] : informational;
        }

        return assembly.GetName().Version?.ToString();
    }

    private static string GetRootPackageName(string assemblyName)
    {
        foreach (var suffix in SuffixesToStrip)
        {
            if (assemblyName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return assemblyName[..^suffix.Length];
        }
        return assemblyName;
    }

    private static bool IsHigherVersion(string candidate, string existing)
    {
        if (NuGet.Versioning.NuGetVersion.TryParse(candidate, out var candidateVer) &&
            NuGet.Versioning.NuGetVersion.TryParse(existing, out var existingVer))
        {
            return candidateVer > existingVer;
        }
        return false;
    }
}
