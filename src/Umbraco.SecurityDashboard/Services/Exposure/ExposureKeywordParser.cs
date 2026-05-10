using System.Text.RegularExpressions;

namespace Umbraco.SecurityDashboard.Services.Exposure;

public static class ExposureKeywordParser
{
    private static readonly Regex KeywordPattern = new(@"^\*\s+\*(.+?)\*\s*$", RegexOptions.Compiled);

    public static IReadOnlyList<string> ParseKeywords(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return [];

        var inExposureSection = false;
        var keywords = new List<string>();

        foreach (var line in description.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');

            if (trimmed.StartsWith("### "))
            {
                inExposureSection = trimmed == "### Exposure";
                continue;
            }

            if (!inExposureSection)
                continue;

            var match = KeywordPattern.Match(trimmed);
            if (match.Success)
                keywords.Add(match.Groups[1].Value);
        }

        return keywords;
    }
}
