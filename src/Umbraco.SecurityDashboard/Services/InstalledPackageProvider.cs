using System.Reflection;

namespace Umbraco.SecurityDashboard.Services;

public class InstalledPackageProvider : IInstalledPackageProvider
{
    private static readonly string[] SuffixesToStrip = [".Core", ".Web", ".Infrastructure"];

    public IReadOnlyDictionary<string, string> GetInstalledUmbracoPackages()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name == null || !name.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase))
                continue;

            var version = GetVersion(assembly);
            if (version == null)
                continue;

            var rootName = StripSuffix(name);

            // Index by root name (e.g. "Umbraco.Cms") so advisories for the
            // meta-package are found even though only sub-package DLLs are loaded.
            if (!result.TryGetValue(rootName, out var existing) ||
                IsHigherVersion(version, existing))
            {
                result[rootName] = version;
            }

            // Also index by the original assembly name (e.g. "Umbraco.Cms.Core")
            // so advisories that reference the specific sub-package are also found.
            if (!rootName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (!result.TryGetValue(name, out var existingOrig) ||
                    IsHigherVersion(version, existingOrig))
                {
                    result[name] = version;
                }
            }
        }

        return result;
    }

    private static string? GetVersion(Assembly assembly)
    {
        var infoVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrEmpty(infoVersion))
        {
            // Strip build metadata (+...) for clean version string
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        return assembly.GetName().Version?.ToString();
    }

    private static string StripSuffix(string name)
    {
        foreach (var suffix in SuffixesToStrip)
        {
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return name[..^suffix.Length];
        }
        return name;
    }

    private static bool IsHigherVersion(string candidate, string existing)
    {
        if (!NuGet.Versioning.NuGetVersion.TryParse(candidate, out var candidateVersion))
            return false;
        if (!NuGet.Versioning.NuGetVersion.TryParse(existing, out var existingVersion))
            return true;
        return candidateVersion > existingVersion;
    }
}
