using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class UpdateComputerStatusRequest
{
    [Required]
    [RegularExpression("^(Active|Maintenance|Decommissioned)$", ErrorMessage = "Estado inválido")]
    public string ComputerStatus { get; set; } = null!;
}
