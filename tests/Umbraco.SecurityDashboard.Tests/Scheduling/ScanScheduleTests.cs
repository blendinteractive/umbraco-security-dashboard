using Umbraco.SecurityDashboard.Configuration;
using Umbraco.SecurityDashboard.Scheduling;

namespace Umbraco.SecurityDashboard.Tests.Scheduling;

public class ScanScheduleTests
{
    // ── Daily scenarios ────────────────────────────────────────────────────

    [Fact]
    public void ComputeNextOccurrence_Daily_Default4Am_WhenCurrentlyBefore4Am_ReturnsTodayAt4Am()
    {
        var settings = new ScanScheduleSettings(); // defaults: Daily, 04:00
        var from = new DateTime(2025, 1, 15, 3, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 15, 4, 0, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Daily_Default4Am_WhenCurrentlyAfter4Am_ReturnsTomorrowAt4Am()
    {
        var settings = new ScanScheduleSettings(); // defaults: Daily, 04:00
        var from = new DateTime(2025, 1, 15, 5, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 16, 4, 0, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Daily_Custom230Am_WhenCurrentlyBefore_ReturnsTodayAt230Am()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily, Hour = 2, Minute = 30 };
        var from = new DateTime(2025, 3, 10, 2, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 3, 10, 2, 30, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Daily_Custom230Am_WhenPastTime_ReturnsNextDayAt230Am()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily, Hour = 2, Minute = 30 };
        var from = new DateTime(2025, 3, 10, 3, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 3, 11, 2, 30, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Daily_Midnight_WhenCurrentlyBefore_ReturnsTodayMidnight()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily, Hour = 0, Minute = 0 };
        // from is technically just before midnight of the same day, which won't happen naturally,
        // but we use a time clearly before midnight of the day in question
        var from = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Local).AddSeconds(-1);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Daily_Midnight_WhenExactlyAtMidnight_ReturnsNextDayMidnight()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily, Hour = 0, Minute = 0 };
        var from = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 5, 2, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        Assert.Equal(expected, result);
    }

    // ── GetCheckInterval / GetStaleThreshold for Daily ────────────────────

    [Fact]
    public void GetCheckInterval_Daily_Returns24Hours()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily };
        Assert.Equal(TimeSpan.FromDays(1), ScanSchedule.GetCheckInterval(settings));
    }

    [Fact]
    public void GetStaleThreshold_Daily_Returns48Hours()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Daily };
        Assert.Equal(TimeSpan.FromHours(48), ScanSchedule.GetStaleThreshold(settings));
    }

    // ── Weekly scenarios ───────────────────────────────────────────────────

    [Fact]
    public void ComputeNextOccurrence_Weekly_Monday3Am_WhenTodayIsTuesdayAfterTime_ReturnsNextMonday()
    {
        // Tuesday Jan 14 10:00 AM — configured Monday 3:00 AM, already missed this week
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly, DayOfWeek = DayOfWeek.Monday, Hour = 3, Minute = 0 };
        var from = new DateTime(2025, 1, 14, 10, 0, 0, DateTimeKind.Local); // Tuesday

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 20, 3, 0, 0, DateTimeKind.Local).ToUniversalTime(); // next Monday
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Weekly_SameDayTimeAlreadyPassed_ReturnsNextWeek()
    {
        // Monday Jan 13 5:00 PM — configured Monday 3:00 AM, already passed today
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly, DayOfWeek = DayOfWeek.Monday, Hour = 3, Minute = 0 };
        var from = new DateTime(2025, 1, 13, 17, 0, 0, DateTimeKind.Local); // Monday 5 PM

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 20, 3, 0, 0, DateTimeKind.Local).ToUniversalTime(); // next Monday
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Weekly_SameDayTimeNotYetPassed_ReturnsToday()
    {
        // Monday Jan 13 1:00 AM — configured Monday 3:00 AM, still ahead today
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly, DayOfWeek = DayOfWeek.Monday, Hour = 3, Minute = 0 };
        var from = new DateTime(2025, 1, 13, 1, 0, 0, DateTimeKind.Local); // Monday 1 AM

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 13, 3, 0, 0, DateTimeKind.Local).ToUniversalTime(); // same Monday
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeNextOccurrence_Weekly_TodayIsTuesdayConfiguredFriday_ReturnsFriday()
    {
        // Tuesday Jan 14 10:00 AM — configured Friday 3:00 AM, 3 days forward
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly, DayOfWeek = DayOfWeek.Friday, Hour = 3, Minute = 0 };
        var from = new DateTime(2025, 1, 14, 10, 0, 0, DateTimeKind.Local); // Tuesday

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        var expected = new DateTime(2025, 1, 17, 3, 0, 0, DateTimeKind.Local).ToUniversalTime(); // Friday
        Assert.Equal(expected, result);
    }

    // ── GetCheckInterval / GetStaleThreshold for Weekly ───────────────────

    [Fact]
    public void GetCheckInterval_Weekly_Returns7Days()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly };
        Assert.Equal(TimeSpan.FromDays(7), ScanSchedule.GetCheckInterval(settings));
    }

    [Fact]
    public void GetStaleThreshold_Weekly_Returns9Days()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Weekly };
        Assert.Equal(TimeSpan.FromDays(9), ScanSchedule.GetStaleThreshold(settings));
    }

    // ── Disabled scenarios ─────────────────────────────────────────────────

    [Fact]
    public void ComputeNextOccurrence_Disabled_ReturnsDateTimeMaxValue()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Disabled };
        var from = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        var result = ScanSchedule.ComputeNextOccurrence(settings, from);

        Assert.Equal(DateTime.MaxValue, result);
    }

    [Fact]
    public void GetCheckInterval_Disabled_ReturnsTimeSpanMaxValue()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Disabled };
        Assert.Equal(TimeSpan.MaxValue, ScanSchedule.GetCheckInterval(settings));
    }

    [Fact]
    public void GetStaleThreshold_Disabled_Returns9Days()
    {
        var settings = new ScanScheduleSettings { Frequency = ScanFrequency.Disabled };
        Assert.Equal(TimeSpan.FromDays(9), ScanSchedule.GetStaleThreshold(settings));
    }
}
