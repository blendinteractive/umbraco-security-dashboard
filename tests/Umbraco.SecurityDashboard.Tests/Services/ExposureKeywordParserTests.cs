using Umbraco.SecurityDashboard.Services.Exposure;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class ExposureKeywordParserTests
{
    [Fact]
    public void ParseKeywords_NullDescription_ReturnsEmpty()
    {
        var result = ExposureKeywordParser.ParseKeywords(null);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseKeywords_WhitespaceDescription_ReturnsEmpty()
    {
        var result = ExposureKeywordParser.ParseKeywords("   ");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseKeywords_NoExposureHeading_ReturnsEmpty()
    {
        var description = "## Introduction\n\nSome text\n\n* *Keyword*\n";
        var result = ExposureKeywordParser.ParseKeywords(description);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseKeywords_ExposureSectionWithNoBullets_ReturnsEmpty()
    {
        var description = "### Exposure\n\nNo bullets here.\n";
        var result = ExposureKeywordParser.ParseKeywords(description);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseKeywords_ValidExposureSection_ReturnsSingleKeyword()
    {
        var description = "### Exposure\n* *Non-Admin Backoffice Users*\n";
        var result = ExposureKeywordParser.ParseKeywords(description);
        Assert.Single(result);
        Assert.Equal("Non-Admin Backoffice Users", result[0]);
    }

    [Fact]
    public void ParseKeywords_MultipleKeywords_ReturnsAll()
    {
        var description = "### Exposure\n* *Non-Admin Backoffice Users*\n* *Content Delivery API*\n";
        var result = ExposureKeywordParser.ParseKeywords(description);
        Assert.Equal(2, result.Count);
        Assert.Contains("Non-Admin Backoffice Users", result);
        Assert.Contains("Content Delivery API", result);
    }

    [Fact]
    public void ParseKeywords_KeywordsOutsideExposureSection_NotIncluded()
    {
        var description = "* *Before Section*\n\n### Exposure\n* *In Section*\n\n### Other\n* *After Section*\n";
        var result = ExposureKeywordParser.ParseKeywords(description);
        Assert.Single(result);
        Assert.Equal("In Section", result[0]);
    }
}
