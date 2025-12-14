using System;
using System.Threading.Tasks;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class AuthService
{
    private readonly ApiService _apiService;
    private readonly ConfigService _configService;
    private LoginResponse? _currentUser;

    public AuthService(ApiService apiService, ConfigService configService)
    {
        _apiService = apiService;
        _configService = configService;
    }

    /// <summary>
    /// Información del usuario autenticado actual
    /// </summary>
    public LoginResponse? CurrentUser => _currentUser;

    /// <summary>
    /// Verifica si hay un usuario autenticado
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;

    /// <summary>
    /// Intenta iniciar sesión con las credenciales proporcionadas
    /// Retorna LoginResponse que puede requerir verificación 2FA
    /// </summary>
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password,
            ComputerId = _configService.GetStoredComputerId()
        };

        var response = await _apiService.LoginAsync(request);

        // Si NO requiere verificación (login exitoso directo)
        if (!response.RequiresVerification && !string.IsNullOrEmpty(response.Token))
        {
            _currentUser = response;
            _configService.SaveToken(response.Token);
            
            // Guardar rol del usuario
            if (!string.IsNullOrEmpty(response.RoleName))
            {
                _configService.SaveUserRole(response.RoleName);
            }
        }

        return response;
    }

    /// <summary>
    /// Verifica el código 2FA y completa el login
    /// </summary>
    public async Task<LoginResponse> VerifyCodeAsync(int userId, string code)
    {
        var request = new VerifyCodeRequest
        {
            UserId = userId,
            Code = code
        };

        var response = await _apiService.VerifyCodeAsync(request);

        // Guardar información del usuario
        _currentUser = response;

        // Guardar token en disco
        if (!string.IsNullOrEmpty(response.Token))
        {
            _configService.SaveToken(response.Token);
        }

        // Guardar rol del usuario
        if (!string.IsNullOrEmpty(response.RoleName))
        {
            _configService.SaveUserRole(response.RoleName);
        }

        return response;
    }

    /// <summary>
    /// Intenta restaurar la sesión desde el token guardado
    /// </summary>
    public bool TryRestoreSession()
    {
        var token = _configService.GetStoredToken();

        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            // Configurar el token en el ApiService
            _apiService.SetBearerToken(token);

            // En una implementación completa, deberías verificar el token con el backend
            // Por ahora, asumimos que si existe es válido
            // TODO: Implementar verificación con endpoint /api/auth/verify

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
        _configService.ClearToken();
        _apiService.ClearBearerToken();
    }
}