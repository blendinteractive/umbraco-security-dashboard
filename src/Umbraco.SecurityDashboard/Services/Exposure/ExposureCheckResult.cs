namespace Umbraco.SecurityDashboard.Services.Exposure;

public record ExposureCheckResult(ExposureVerdict Verdict, string? MitigationDescription = null);
