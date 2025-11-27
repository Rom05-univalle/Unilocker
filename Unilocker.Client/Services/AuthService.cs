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
    /// </summary>
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = await _apiService.LoginAsync(request);

        // Guardar información del usuario
        _currentUser = response;

        // Guardar token en disco
        _configService.SaveToken(response.Token);

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