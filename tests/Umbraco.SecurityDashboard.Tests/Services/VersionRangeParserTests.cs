using NuGet.Versioning;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class VersionRangeParserTests
{
    [Fact]
    public void Parse_LessThan_ReturnsExclusiveUpperBound()
    {
        var range = VersionRangeParser.Parse("< 10.6.1");
        Assert.NotNull(range);
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.6.1")));
    }

    [Fact]
    public void Parse_LessThanOrEqual_ReturnsInclusiveUpperBound()
    {
        var range = VersionRangeParser.Parse("<= 10.6.1");
        Assert.NotNull(range);
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.6.2")));
    }

    [Fact]
    public void Parse_GreaterThanOrEqual_ReturnsInclusiveLowerBound()
    {
        var range = VersionRangeParser.Parse(">= 10.0.0");
        Assert.NotNull(range);
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(range.Satisfies(NuGetVersion.Parse("11.0.0")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("9.9.9")));
    }

    [Fact]
    public void Parse_GreaterThan_ReturnsExclusiveLowerBound()
    {
        var range = VersionRangeParser.Parse("> 10.0.0");
        Assert.NotNull(range);
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.0.1")));
    }

    [Fact]
    public void Parse_Equals_ReturnsExactVersion()
    {
        var range = VersionRangeParser.Parse("= 10.6.1");
        Assert.NotNull(range);
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.6.0")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.6.2")));
    }

    [Fact]
    public void Parse_CompoundRange_ReturnsBoundedRange()
    {
        var range = VersionRangeParser.Parse(">= 10.0.0, < 10.6.1");
        Assert.NotNull(range);
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.0.0")));
        Assert.True(range.Satisfies(NuGetVersion.Parse("10.5.0")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("10.6.1")));
        Assert.False(range.Satisfies(NuGetVersion.Parse("9.9.9")));
    }

    [Fact]
    public void Parse_NullInput_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse(null));
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse(""));
    }

    [Fact]
    public void Parse_InvalidInput_ReturnsNull()
    {
        Assert.Null(VersionRangeParser.Parse("not a version range"));
    }
}
