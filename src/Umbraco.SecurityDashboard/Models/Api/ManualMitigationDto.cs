namespace Umbraco.SecurityDashboard.Models.Api;

public class ManualMitigationDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime MitigatedAt { get; set; }
    public string MitigatedBy { get; set; } = string.Empty;
}
