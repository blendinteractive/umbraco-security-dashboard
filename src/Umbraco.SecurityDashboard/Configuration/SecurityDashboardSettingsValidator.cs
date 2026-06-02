using Microsoft.Extensions.Options;

namespace Umbraco.SecurityDashboard.Configuration;

public class SecurityDashboardSettingsValidator : IValidateOptions<SecurityDashboardSettings>
{
    public ValidateOptionsResult Validate(string? name, SecurityDashboardSettings options)
    {
        var errors = new List<string>();
        var schedule = options.ScanSchedule;

        if (!Enum.IsDefined(typeof(ScanFrequency), schedule.Frequency))
            errors.Add($"ScanSchedule.Frequency '{schedule.Frequency}' is not a valid ScanFrequency value.");

        if (schedule.Hour < 0 || schedule.Hour > 23)
            errors.Add($"ScanSchedule.Hour must be between 0 and 23 (got {schedule.Hour}).");

        if (schedule.Minute < 0 || schedule.Minute > 59)
            errors.Add($"ScanSchedule.Minute must be between 0 and 59 (got {schedule.Minute}).");

        if (!Enum.IsDefined(typeof(DayOfWeek), schedule.DayOfWeek))
            errors.Add($"ScanSchedule.DayOfWeek '{schedule.DayOfWeek}' is not a valid DayOfWeek value.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
