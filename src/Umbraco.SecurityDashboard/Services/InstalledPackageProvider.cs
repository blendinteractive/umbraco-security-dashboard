using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Umbraco.SecurityDashboard.Services;

public class InstalledPackageProvider : IInstalledPackageProvider
{
    private static readonly string[] SuffixesToStrip = [".Core", ".Web", ".Infrastructure"];

    public IReadOnlyDictionary<string, string> GetInstalledUmbracoPackages()
    {
        var ctx = DependencyContext.Default;
        if (ctx != null)
            return GetFromDependencyContext(ctx);

        // Fallback for single-file publish or unusual host scenarios
        return GetFromAssemblies();
    }

    private static Dictionary<string, string> GetFromDependencyContext(DependencyContext ctx)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var lib in ctx.RuntimeLibraries)
        {
            if (!lib.Name.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrEmpty(lib.Version))
                continue;

            // Strip build metadata (+...) that can appear in informational versions
            var version = lib.Version;
            var plusIndex = version.IndexOf('+');
            if (plusIndex >= 0)
                version = version[..plusIndex];

            result[lib.Name] = version;
        }

        return result;
    }

    private static Dictionary<string, string> GetFromAssemblies()
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

            if (!result.TryGetValue(rootName, out var existing) || IsHigherVersion(version, existing))
                result[rootName] = version;

            if (!rootName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (!result.TryGetValue(name, out var existingOrig) || IsHigherVersion(version, existingOrig))
                    result[name] = version;
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
