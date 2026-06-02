using Microsoft.Extensions.Options;
using Umbraco.SecurityDashboard.Configuration;

namespace Umbraco.SecurityDashboard.Tests.Configuration;

public class SecurityDashboardSettingsValidatorTests
{
    private static ValidateOptionsResult Validate(SecurityDashboardSettings settings)
    {
        var validator = new SecurityDashboardSettingsValidator();
        return validator.Validate(null, settings);
    }

    // ── Valid defaults ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_DefaultSettings_Succeeds()
    {
        var result = Validate(new SecurityDashboardSettings());
        Assert.True(result.Succeeded);
    }

    // ── Hour validation ────────────────────────────────────────────────────

    [Theory]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Validate_InvalidHour_Fails(int hour)
    {
        var settings = new SecurityDashboardSettings
        {
            ScanSchedule = new ScanScheduleSettings { Hour = hour }
        };

        var result = Validate(settings);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, f => f.Contains("Hour"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(23)]
    public void Validate_ValidHour_Succeeds(int hour)
    {
        var settings = new SecurityDashboardSettings
        {
            ScanSchedule = new ScanScheduleSettings { Hour = hour }
        };

        var result = Validate(settings);
        Assert.True(result.Succeeded);
    }

    // ── Minute validation ──────────────────────────────────────────────────

    [Theory]
    [InlineData(60)]
    [InlineData(61)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Validate_InvalidMinute_Fails(int minute)
    {
        var settings = new SecurityDashboardSettings
        {
            ScanSchedule = new ScanScheduleSettings { Minute = minute }
        };

        var result = Validate(settings);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, f => f.Contains("Minute"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(59)]
    public void Validate_ValidMinute_Succeeds(int minute)
    {
        var settings = new SecurityDashboardSettings
        {
            ScanSchedule = new ScanScheduleSettings { Minute = minute }
        };

        var result = Validate(settings);
        Assert.True(result.Succeeded);
    }

    // ── Multiple errors at once ────────────────────────────────────────────

    [Fact]
    public void Validate_BothHourAndMinuteInvalid_ReturnsBothErrors()
    {
        var settings = new SecurityDashboardSettings
        {
            ScanSchedule = new ScanScheduleSettings { Hour = 25, Minute = 61 }
        };

        var result = Validate(settings);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, f => f.Contains("Hour"));
        Assert.Contains(result.Failures!, f => f.Contains("Minute"));
    }
}
