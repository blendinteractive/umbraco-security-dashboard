namespace Umbraco.SecurityDashboard.Configuration;

public class ScanScheduleSettings
{
    public ScanFrequency Frequency { get; set; } = ScanFrequency.Daily;

    /// <summary>Hour of day for the scan (0–23). Default: 4.</summary>
    public int Hour { get; set; } = 4;

    /// <summary>Minute of the hour for the scan (0–59). Default: 0.</summary>
    public int Minute { get; set; } = 0;

    /// <summary>Day of week for weekly scans. Default: Monday. Ignored for Daily/Disabled.</summary>
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
}
