using NuGet.Versioning;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class VersionRangeParserTests
{
    [Fact]
    public void Parse_LessThan_ReturnsUpperExclusiveRange()
    {
        var result = VersionRangeParser.Parse("< 10.6.1");

        Assert.NotNull(result);
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.2")));
    }

    [Fact]
    public void Parse_LessThanOrEqual_ReturnsUpperInclusiveRange()
    {
        var result = VersionRangeParser.Parse("<= 10.6.1");

        Assert.NotNull(result);
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.2")));
    }

    [Fact]
    public void Parse_GreaterThanOrEqual_ReturnsLowerInclusiveRange()
    {
        var result = VersionRangeParser.Parse(">= 10.0.0");

        Assert.NotNull(result);
        Assert.False(result.Satisfies(NuGetVersion.Parse("9.9.9")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("11.0.0")));
    }

    [Fact]
    public void Parse_GreaterThan_ReturnsLowerExclusiveRange()
    {
        var result = VersionRangeParser.Parse("> 10.0.0");

        Assert.NotNull(result);
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.0.1")));
    }

    [Fact]
    public void Parse_Equals_ReturnsSingleVersionRange()
    {
        var result = VersionRangeParser.Parse("= 10.6.1");

        Assert.NotNull(result);
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.2")));
    }

    [Fact]
    public void Parse_Compound_GreaterThanOrEqualAndLessThan_ReturnsBoundedRange()
    {
        var result = VersionRangeParser.Parse(">= 10.0.0, < 10.6.1");

        Assert.NotNull(result);
        Assert.False(result.Satisfies(NuGetVersion.Parse("9.9.9")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.1")));
    }

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse(null));
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse(string.Empty));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse("   "));
    }

    [Fact]
    public void Parse_InvalidInput_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse("not a version range"));
    }

    [Fact]
    public void Parse_HandlesLeadingAndTrailingWhitespace()
    {
        var result = VersionRangeParser.Parse("  < 10.6.1  ");

        Assert.NotNull(result);
        Assert.True(result.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.False(result.Satisfies(NuGetVersion.Parse("10.6.1")));
    }
}
