using System.ComponentModel.DataAnnotations;

namespace Umbraco.SecurityDashboard.Models.Api;

public class CreateMitigationRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
}
