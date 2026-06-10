using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Umbraco.SecurityDashboard.Services.Exposure;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class ExposureCheckEvaluatorTests
{
    private static ExposureCheckEvaluator CreateSut(params IExposureCheck[] checks) =>
        new(checks, NullLogger<ExposureCheckEvaluator>.Instance);

    [Fact]
    public async Task EvaluateAsync_NoKeywords_ReturnsVulnerable()
    {
        var sut = CreateSut();
        var result = await sut.EvaluateAsync([]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_NoMatchingChecksRegistered_ReturnsVulnerable()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Some Other Keyword");
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_SingleMatchReturnsMitigated_ResultIsMitigated()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Non-Admin Backoffice Users");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Mitigated", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_SingleMatchReturnsVulnerable_ResultIsVulnerable()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Non-Admin Backoffice Users");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Vulnerable));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleChecksWithMixedVerdicts_WorstCaseWins()
    {
        var mitigatedCheck = Substitute.For<IExposureCheck>();
        mitigatedCheck.Keyword.Returns("Keyword A");
        mitigatedCheck.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated));

        var vulnerableCheck = Substitute.For<IExposureCheck>();
        vulnerableCheck.Keyword.Returns("Keyword B");
        vulnerableCheck.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Vulnerable));

        var sut = CreateSut(mitigatedCheck, vulnerableCheck);
        var result = await sut.EvaluateAsync(["Keyword A", "Keyword B"]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_CheckThrowsException_TreatsAsVulnerableAndDoesNotPropagate()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Explosive Keyword");
        check.CheckAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Explosive Keyword"]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_AllChecksReturnNotAffected_ReturnsNotAffected()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Safe Keyword");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.NotAffected));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Safe Keyword"]);
        Assert.Equal("NotAffected", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_PartialKeywordMatch_WorstCaseOfMatchingChecksApplies()
    {
        // "Registered Keyword" has a check returning Mitigated.
        // "Unregistered Keyword" has no check — it is ignored (not treated as Vulnerable).
        // Worst-case of matching checks is Mitigated.
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Registered Keyword");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Registered Keyword", "Unregistered Keyword"]);
        Assert.Equal("Mitigated", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_EmptyKeywordList_ReturnsVulnerable()
    {
        // Explicit fail-safe: no keywords parsed from description → default to Vulnerable.
        var sut = CreateSut();
        var result = await sut.EvaluateAsync([]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    [Fact]
    public async Task EvaluateAsync_KeywordsWithNoRegisteredMatch_ReturnsVulnerable()
    {
        // Explicit fail-safe: keywords present but none matches any registered check → Vulnerable.
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Registered Check");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Completely Unknown Keyword"]);
        Assert.Equal("Vulnerable", result.Verdict);
    }

    // --- T014: Description combining tests ---

    [Fact]
    public async Task EvaluateAsync_SingleMitigatedCheckWithDescription_ReturnsThatDescription()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Content Delivery API");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated, "Content Delivery API is disabled"));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Content Delivery API"]);
        Assert.Equal("Mitigated", result.Verdict);
        Assert.Equal("Content Delivery API is disabled", result.MitigationDescription);
    }

    [Fact]
    public async Task EvaluateAsync_TwoMitigatedChecksWithDescriptions_JoinsWithSemicolon()
    {
        var check1 = Substitute.For<IExposureCheck>();
        check1.Keyword.Returns("Content Delivery API");
        check1.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated, "Content Delivery API is disabled"));

        var check2 = Substitute.For<IExposureCheck>();
        check2.Keyword.Returns("Non-Admin Backoffice Users");
        check2.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated, "All backoffice users are administrators"));

        var sut = CreateSut(check1, check2);
        var result = await sut.EvaluateAsync(["Content Delivery API", "Non-Admin Backoffice Users"]);
        Assert.Equal("Mitigated", result.Verdict);
        Assert.Equal("Content Delivery API is disabled; All backoffice users are administrators", result.MitigationDescription);
    }

    [Fact]
    public async Task EvaluateAsync_MitigatedCheckWithNullDescription_UsesFallback()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Non-Admin Backoffice Users");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Mitigated, null));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Mitigated", result.Verdict);
        Assert.Equal("Mitigated by exposure check", result.MitigationDescription);
    }

    [Fact]
    public async Task EvaluateAsync_VulnerableVerdict_MitigationDescriptionIsNull()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Content Delivery API");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(new ExposureCheckResult(ExposureVerdict.Vulnerable, null));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Content Delivery API"]);
        Assert.Equal("Vulnerable", result.Verdict);
        Assert.Null(result.MitigationDescription);
    }
}
