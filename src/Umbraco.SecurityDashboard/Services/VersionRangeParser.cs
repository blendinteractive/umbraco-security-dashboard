using NuGet.Versioning;

namespace Umbraco.SecurityDashboard.Services;

public static class VersionRangeParser
{
    /// <summary>
    /// Converts a GitHub version range string into a NuGet <see cref="VersionRange"/>.
    /// Returns <c>null</c> when the input cannot be parsed.
    /// </summary>
    public static VersionRange? Parse(string? rangeString)
    {
        if (string.IsNullOrWhiteSpace(rangeString))
            return null;

        var trimmed = rangeString.Trim();

        // Compound range: ">= A, < B" or ">= A, <= B" etc.
        if (trimmed.Contains(','))
        {
            var parts = trimmed.Split(',', 2);
            if (parts.Length == 2)
            {
                var low = ParseSingle(parts[0].Trim());
                var high = ParseSingle(parts[1].Trim());
                if (low != null && high != null)
                {
                    // Combine: take min version from low, max version from high
                    return new VersionRange(
                        minVersion: low.MinVersion,
                        includeMinVersion: low.IsMinInclusive,
                        maxVersion: high.MaxVersion,
                        includeMaxVersion: high.IsMaxInclusive);
                }
            }
            return null;
        }

        return ParseSingle(trimmed);
    }

    private static VersionRange? ParseSingle(string expr)
    {
        if (expr.StartsWith("<= "))
        {
            if (!NuGetVersion.TryParse(expr[3..].Trim(), out var v)) return null;
            return new VersionRange(maxVersion: v, includeMaxVersion: true);
        }
        if (expr.StartsWith("< "))
        {
            if (!NuGetVersion.TryParse(expr[2..].Trim(), out var v)) return null;
            return new VersionRange(maxVersion: v, includeMaxVersion: false);
        }
        if (expr.StartsWith(">= "))
        {
            if (!NuGetVersion.TryParse(expr[3..].Trim(), out var v)) return null;
            return new VersionRange(minVersion: v, includeMinVersion: true);
        }
        if (expr.StartsWith("> "))
        {
            if (!NuGetVersion.TryParse(expr[2..].Trim(), out var v)) return null;
            return new VersionRange(minVersion: v, includeMinVersion: false);
        }
        if (expr.StartsWith("= "))
        {
            if (!NuGetVersion.TryParse(expr[2..].Trim(), out var v)) return null;
            return new VersionRange(minVersion: v, includeMinVersion: true, maxVersion: v, includeMaxVersion: true);
        }

        // Try parsing as NuGet bracket notation directly as a fallback
        if (VersionRange.TryParse(expr, out var range))
            return range;

        return null;
    }
}
