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
        Assert.Equal("Vulnerable", result);
    }

    [Fact]
    public async Task EvaluateAsync_NoMatchingChecksRegistered_ReturnsVulnerable()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Some Other Keyword");
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Vulnerable", result);
    }

    [Fact]
    public async Task EvaluateAsync_SingleMatchReturnsMitigated_ResultIsMitigated()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Non-Admin Backoffice Users");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(ExposureVerdict.Mitigated);
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Mitigated", result);
    }

    [Fact]
    public async Task EvaluateAsync_SingleMatchReturnsVulnerable_ResultIsVulnerable()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Non-Admin Backoffice Users");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(ExposureVerdict.Vulnerable);
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Non-Admin Backoffice Users"]);
        Assert.Equal("Vulnerable", result);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleChecksWithMixedVerdicts_WorstCaseWins()
    {
        var mitigatedCheck = Substitute.For<IExposureCheck>();
        mitigatedCheck.Keyword.Returns("Keyword A");
        mitigatedCheck.CheckAsync(Arg.Any<CancellationToken>()).Returns(ExposureVerdict.Mitigated);

        var vulnerableCheck = Substitute.For<IExposureCheck>();
        vulnerableCheck.Keyword.Returns("Keyword B");
        vulnerableCheck.CheckAsync(Arg.Any<CancellationToken>()).Returns(ExposureVerdict.Vulnerable);

        var sut = CreateSut(mitigatedCheck, vulnerableCheck);
        var result = await sut.EvaluateAsync(["Keyword A", "Keyword B"]);
        Assert.Equal("Vulnerable", result);
    }

    [Fact]
    public async Task EvaluateAsync_CheckThrowsException_TreatsAsVulnerableAndDoesNotPropagate()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Explosive Keyword");
        check.CheckAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Explosive Keyword"]);
        Assert.Equal("Vulnerable", result);
    }

    [Fact]
    public async Task EvaluateAsync_AllChecksReturnNotAffected_ReturnsNotAffected()
    {
        var check = Substitute.For<IExposureCheck>();
        check.Keyword.Returns("Safe Keyword");
        check.CheckAsync(Arg.Any<CancellationToken>()).Returns(ExposureVerdict.NotAffected);
        var sut = CreateSut(check);
        var result = await sut.EvaluateAsync(["Safe Keyword"]);
        Assert.Equal("NotAffected", result);
    }
}
