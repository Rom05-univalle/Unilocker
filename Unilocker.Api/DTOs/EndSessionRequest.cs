using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class EndSessionRequest
{
    [Required(ErrorMessage = "El método de cierre es requerido")]
    [RegularExpression("^(Normal|Forced|Timeout|Administrative)$",
        ErrorMessage = "EndMethod debe ser: Normal, Forced, Timeout o Administrative")]
    public string EndMethod { get; set; } = null!;
}