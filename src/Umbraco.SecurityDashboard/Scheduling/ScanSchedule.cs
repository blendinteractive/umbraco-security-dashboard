using Umbraco.SecurityDashboard.Configuration;

namespace Umbraco.SecurityDashboard.Scheduling;

public static class ScanSchedule
{
    /// <summary>
    /// Computes the next UTC scan occurrence from <paramref name="from"/> (local time; defaults to now).
    /// Returns DateTime.MaxValue when Disabled.
    /// </summary>
    public static DateTime ComputeNextOccurrence(ScanScheduleSettings settings, DateTime? from = null)
    {
        var now = from ?? DateTime.Now;

        return settings.Frequency switch
        {
            ScanFrequency.Daily => ComputeNextDaily(settings, now),
            ScanFrequency.Weekly => ComputeNextWeekly(settings, now),
            ScanFrequency.Disabled => DateTime.MaxValue,
            _ => DateTime.MaxValue
        };
    }

    /// <summary>Recurrence interval for the background job Period and startup check.</summary>
    public static TimeSpan GetCheckInterval(ScanScheduleSettings settings) =>
        settings.Frequency switch
        {
            ScanFrequency.Daily => TimeSpan.FromDays(1),
            ScanFrequency.Weekly => TimeSpan.FromDays(7),
            ScanFrequency.Disabled => TimeSpan.MaxValue,
            _ => TimeSpan.MaxValue
        };

    /// <summary>Age threshold beyond which a check result is considered stale.</summary>
    public static TimeSpan GetStaleThreshold(ScanScheduleSettings settings) =>
        settings.Frequency switch
        {
            ScanFrequency.Daily => TimeSpan.FromHours(48),
            ScanFrequency.Weekly => TimeSpan.FromDays(9),
            ScanFrequency.Disabled => TimeSpan.FromDays(9),
            _ => TimeSpan.FromDays(9)
        };

    private static DateTime ComputeNextDaily(ScanScheduleSettings settings, DateTime from)
    {
        var candidate = from.Date.AddHours(settings.Hour).AddMinutes(settings.Minute);
        if (from >= candidate)
            candidate = candidate.AddDays(1);
        return candidate.ToUniversalTime();
    }

    private static DateTime ComputeNextWeekly(ScanScheduleSettings settings, DateTime from)
    {
        var candidate = from.Date.AddHours(settings.Hour).AddMinutes(settings.Minute);

        // Walk forward until we land on the configured day of week (0–6 days)
        var daysToAdd = ((int)settings.DayOfWeek - (int)candidate.DayOfWeek + 7) % 7;
        candidate = candidate.AddDays(daysToAdd);

        if (candidate <= from)
            candidate = candidate.AddDays(7);

        return candidate.ToUniversalTime();
    }
}
