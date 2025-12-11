using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Unilocker.Api.Extensions;

/// <summary>
/// Extensiones para controladores que facilitan tareas comunes
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
    /// <param name="controller">Controlador actual</param>
    /// <returns>ID del usuario o null si no está autenticado</returns>
    public static int? GetCurrentUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst("userId")?.Value 
                       ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return null;
        
        return int.TryParse(userIdClaim, out int userId) ? userId : null;
    }

    /// <summary>
    /// Obtiene el username del usuario autenticado desde el token JWT
    /// </summary>
    /// <param name="controller">Controlador actual</param>
    /// <returns>Username del usuario o null si no está autenticado</returns>
    public static string? GetCurrentUsername(this ControllerBase controller)
    {
        return controller.User.FindFirst("username")?.Value 
            ?? controller.User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
