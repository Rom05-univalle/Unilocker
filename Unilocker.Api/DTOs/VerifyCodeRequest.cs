using System.ComponentModel.DataAnnotations;

namespace Unilocker.Api.DTOs;

public class VerifyCodeRequest
{
    [Required(ErrorMessage = "El UserId es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe contener solo números")]
    public string Code { get; set; } = null!;
}