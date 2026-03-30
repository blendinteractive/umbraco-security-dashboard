using NuGet.Versioning;

namespace Umbraco.SecurityDashboard.Services;

public static class VersionRangeParser
{
    /// <summary>
    /// Converts a GitHub advisory version range string to a NuGet <see cref="VersionRange"/>.
    /// Returns <c>null</c> if the string cannot be parsed.
    /// </summary>
    /// <remarks>
    /// GitHub range format mapping (per research.md Decision 3):
    /// <list type="table">
    ///   <item><term>&lt; X.Y.Z</term><description>(, X.Y.Z)</description></item>
    ///   <item><term>&lt;= X.Y.Z</term><description>(, X.Y.Z]</description></item>
    ///   <item><term>&gt;= X.Y.Z</term><description>[X.Y.Z, )</description></item>
    ///   <item><term>&gt; X.Y.Z</term><description>(X.Y.Z, )</description></item>
    ///   <item><term>= X.Y.Z</term><description>[X.Y.Z]</description></item>
    ///   <item><term>&gt;= A, &lt; B</term><description>[A, B)</description></item>
    /// </list>
    /// </remarks>
    public static VersionRange? Parse(string? rangeString)
    {
        if (string.IsNullOrWhiteSpace(rangeString))
            return null;

        var trimmed = rangeString.Trim();

        // Try compound range first: ">= A, < B" or ">= A, <= B"
        if (trimmed.Contains(','))
        {
            return ParseCompoundRange(trimmed);
        }

        // Single operator ranges
        if (trimmed.StartsWith("<="))
        {
            var version = trimmed[2..].Trim();
            return NuGetVersion.TryParse(version, out var v)
                ? new VersionRange(minVersion: null, includeMinVersion: false, maxVersion: v, includeMaxVersion: true)
                : null;
        }

        if (trimmed.StartsWith(">="))
        {
            var version = trimmed[2..].Trim();
            return NuGetVersion.TryParse(version, out var v)
                ? new VersionRange(v, true)
                : null;
        }

        if (trimmed.StartsWith('<'))
        {
            var version = trimmed[1..].Trim();
            return NuGetVersion.TryParse(version, out var v)
                ? new VersionRange(minVersion: null, includeMinVersion: false, maxVersion: v, includeMaxVersion: false)
                : null;
        }

        if (trimmed.StartsWith('>'))
        {
            var version = trimmed[1..].Trim();
            return NuGetVersion.TryParse(version, out var v)
                ? new VersionRange(v, false)
                : null;
        }

        if (trimmed.StartsWith('='))
        {
            var version = trimmed[1..].Trim();
            return NuGetVersion.TryParse(version, out var v)
                ? new VersionRange(v, true, v, true)
                : null;
        }

        // Try direct NuGet range notation as fallback
        return VersionRange.TryParse(trimmed, out var parsed) ? parsed : null;
    }

    private static VersionRange? ParseCompoundRange(string trimmed)
    {
        var parts = trimmed.Split(',', 2);
        if (parts.Length != 2)
            return null;

        var left = parts[0].Trim();
        var right = parts[1].Trim();

        NuGetVersion? minVersion = null;
        bool includeMin = false;
        NuGetVersion? maxVersion = null;
        bool includeMax = false;

        foreach (var part in new[] { left, right })
        {
            if (part.StartsWith(">="))
            {
                if (!NuGetVersion.TryParse(part[2..].Trim(), out var v)) return null;
                minVersion = v;
                includeMin = true;
            }
            else if (part.StartsWith('>'))
            {
                if (!NuGetVersion.TryParse(part[1..].Trim(), out var v)) return null;
                minVersion = v;
                includeMin = false;
            }
            else if (part.StartsWith("<="))
            {
                if (!NuGetVersion.TryParse(part[2..].Trim(), out var v)) return null;
                maxVersion = v;
                includeMax = true;
            }
            else if (part.StartsWith('<'))
            {
                if (!NuGetVersion.TryParse(part[1..].Trim(), out var v)) return null;
                maxVersion = v;
                includeMax = false;
            }
            else
            {
                return null;
            }
        }

        if (minVersion is null && maxVersion is null)
            return null;

        return new VersionRange(minVersion, includeMin, maxVersion, includeMax);
    }
}
