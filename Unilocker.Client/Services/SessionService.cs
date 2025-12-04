using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims; // ← AGREGAR ESTA LÍNEA
using System.Threading.Tasks;
using System.Windows.Threading;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class SessionService
{
    private readonly ApiService _apiService;
    private readonly ConfigService _configService;
    private SessionResponse? _currentSession;
    private DispatcherTimer? _heartbeatTimer;
    private bool _isHeartbeatActive = false;

    public SessionService(ApiService apiService, ConfigService configService)
    {
        _apiService = apiService;
        _configService = configService;
    }

    /// <summary>
    /// Sesión actual activa
    /// </summary>
    public SessionResponse? CurrentSession => _currentSession;

    /// <summary>
    /// ID de la sesión activa
    /// </summary>
    public int? CurrentSessionId => _currentSession?.Id;

    /// <summary>
    /// Verifica si hay una sesión activa
    /// </summary>
    public bool HasActiveSession => _currentSession != null && _currentSession.IsActive;

    /// <summary>
    /// Extrae el UserId del token JWT
    /// </summary>
    public int ExtractUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Buscar el claim "userId" (exactamente como lo genera el backend)
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Si no se encontró con "userId", intentar con otros nombres comunes
            userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type.EndsWith("userId", StringComparison.OrdinalIgnoreCase) ||
                c.Type == "sub" ||
                c.Type == "nameid" ||
                c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                return userId;
            }

            // Si aún no se encontró, listar todos los claims disponibles en el error
            string allClaims = string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}"));
            throw new Exception($"No se encontró el claim 'userId' en el token JWT. Claims disponibles: {allClaims}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al decodificar token JWT: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Inicia una nueva sesión
    /// </summary>
    public async Task<SessionResponse> StartSessionAsync(int userId, int computerId)
    {
        try
        {
            var request = new StartSessionRequest
            {
                UserId = userId,
                ComputerId = computerId
            };

            var response = await _apiService.StartSessionAsync(request);
            _currentSession = response;

            // Iniciar el timer de heartbeat automáticamente
            StartHeartbeatTimer();

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al iniciar sesión: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Inicia el timer de heartbeat (envía señal cada 30 segundos)
    /// </summary>
    public void StartHeartbeatTimer()
    {
        if (_currentSession == null || _isHeartbeatActive)
            return;

        _heartbeatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };

        _heartbeatTimer.Tick += async (sender, e) => await SendHeartbeatAsync();
        _heartbeatTimer.Start();
        _isHeartbeatActive = true;
    }

    /// <summary>
    /// Detiene el timer de heartbeat
    /// </summary>
    public void StopHeartbeatTimer()
    {
        if (_heartbeatTimer != null)
        {
            _heartbeatTimer.Stop();
            _heartbeatTimer = null;
            _isHeartbeatActive = false;
        }
    }

    /// <summary>
    /// Envía un heartbeat al servidor para mantener la sesión activa
    /// </summary>
    public async Task<bool> SendHeartbeatAsync()
    {
        if (_currentSession == null || !_currentSession.IsActive)
            return false;

        try
        {
            bool success = await _apiService.SendHeartbeatAsync(_currentSession.Id);

            if (success)
            {
                // Actualizar el LastHeartbeat local
                _currentSession.LastHeartbeat = DateTime.Now;
            }

            return success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finaliza la sesión actual
    /// </summary>
    public async Task<SessionResponse> EndSessionAsync(string endMethod = "Normal")
    {
        if (_currentSession == null)
        {
            throw new Exception("No hay sesión activa para finalizar");
        }

        try
        {
            // Detener el heartbeat timer
            StopHeartbeatTimer();

            var request = new EndSessionRequest
            {
                EndMethod = endMethod
            };

            var response = await _apiService.EndSessionAsync(_currentSession.Id, request);

            // Limpiar sesión actual
            _currentSession = null;

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al finalizar sesión: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene el ComputerId desde ConfigService
    /// </summary>
    public int GetStoredComputerId()
    {
        var computerId = _configService.GetStoredComputerId();

        if (!computerId.HasValue)
        {
            throw new Exception("No se encontró el ID de la computadora registrada. " +
                "Por favor, registre la computadora primero.");
        }

        return computerId.Value;
    }
}