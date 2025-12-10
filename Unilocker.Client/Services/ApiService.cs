using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _bearerToken;

    public ApiService(string baseUrl)
    {
        // Configurar para aceptar certificados SSL auto-firmados (solo para desarrollo)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    /// <summary>
    /// Configura el token de autenticación Bearer
    /// </summary>
    public void SetBearerToken(string token)
    {
        _bearerToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Limpia el token de autenticación
    /// </summary>
    public void ClearBearerToken()
    {
        _bearerToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Login - Autenticación de usuario
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                // Manejar errores de autenticación de forma amigable
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Usuario o contraseña incorrectos");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception("Por favor verifica los datos ingresados");
                }
                else
                {
                    throw new Exception("No se pudo conectar con el servidor. Intenta nuevamente.");
                }
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null)
            {
                throw new Exception("Error al procesar la respuesta del servidor");
            }

            // Configurar el token automáticamente
            SetBearerToken(result.Token);

            return result;
        }
        catch (HttpRequestException)
        {
            throw new Exception("No se pudo conectar con el servidor. Verifica tu conexión.");
        }
        catch (Exception ex) when (ex.Message.Contains("Usuario o contraseña") || 
                                    ex.Message.Contains("datos ingresados") || 
                                    ex.Message.Contains("conectar con el servidor"))
        {
            // Re-lanzar mensajes amigables sin modificar
            throw;
        }
        catch (Exception)
        {
            throw new Exception("Ocurrió un error inesperado. Intenta nuevamente.");
        }
    }

    /// <summary>
    /// Inicia una nueva sesión
    /// </summary>
    public async Task<SessionResponse> StartSessionAsync(StartSessionRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/sessions/start", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SessionResponse>();

            if (result == null)
            {
                throw new Exception("La respuesta del servidor está vacía");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error de conexión al iniciar sesión: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al iniciar sesión: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Envía un heartbeat para mantener la sesión activa
    /// </summary>
    public async Task<bool> SendHeartbeatAsync(int sessionId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/sessions/{sessionId}/heartbeat", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finaliza una sesión
    /// </summary>
    public async Task<SessionResponse> EndSessionAsync(int sessionId, EndSessionRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/sessions/{sessionId}/end", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SessionResponse>();

            if (result == null)
            {
                throw new Exception("La respuesta del servidor está vacía");
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al finalizar sesión: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Forzar cierre de todas las sesiones activas de un usuario
    /// </summary>
    public async Task<bool> ForceCloseUserSessionsAsync(int userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/sessions/user/{userId}/force-close", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene la lista de tipos de problema
    /// </summary>
    public async Task<List<ProblemType>> GetProblemTypesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/problemtypes");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<ProblemType>>();
            return result ?? new List<ProblemType>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener tipos de problema: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Crea un reporte
    /// </summary>
    public async Task<bool> CreateReportAsync(CreateReportRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/reports", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene la lista de aulas disponibles
    /// </summary>
    public async Task<List<ClassroomInfo>> GetAvailableClassroomsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/computers/classrooms");
            response.EnsureSuccessStatusCode();

            var classrooms = await response.Content.ReadFromJsonAsync<List<ClassroomInfo>>();
            return classrooms ?? new List<ClassroomInfo>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener aulas: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Registra una computadora en el sistema
    /// </summary>
    public async Task<ComputerResponse> RegisterComputerAsync(RegisterComputerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/computers/register", request);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception("Datos incompletos o inválidos. Verifica la información.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new Exception("Este equipo ya está registrado en el sistema.");
                }
                else
                {
                    throw new Exception("No se pudo completar el registro. Intenta nuevamente.");
                }
            }

            var result = await response.Content.ReadFromJsonAsync<ComputerResponse>();

            if (result == null)
            {
                throw new Exception("Error al procesar la respuesta del servidor");
            }

            return result;
        }
        catch (HttpRequestException)
        {
            throw new Exception("No se pudo conectar con el servidor. Verifica tu conexión.");
        }
        catch (Exception ex) when (ex.Message.Contains("Datos incompletos") || 
                                    ex.Message.Contains("ya está registrado") || 
                                    ex.Message.Contains("completar el registro"))
        {
            // Re-lanzar mensajes amigables sin modificar
            throw;
        }
        catch (Exception)
        {
            throw new Exception("Ocurrió un error inesperado al registrar el equipo.");
        }
    }

    /// <summary>
    /// Verifica la conectividad con el API
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Verifica el código 2FA
    /// </summary>
    public async Task<LoginResponse> VerifyCodeAsync(VerifyCodeRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/verify-code", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                // Intentar extraer el mensaje de error
                try
                {
                    var errorJson = System.Text.Json.JsonDocument.Parse(errorContent);
                    if (errorJson.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        throw new Exception(messageElement.GetString() ?? "Código inválido");
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Si no es JSON, usar el contenido completo
                }

                throw new Exception($"Código inválido: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                throw new Exception("La respuesta del servidor está vacía");
            }

            // Configurar el token automáticamente
            SetBearerToken(result.Token);

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error de conexión con el servidor: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al verificar código: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Desregistra una computadora (cambia Status a false en la BD)
    /// </summary>
    public async Task<bool> UnregisterComputerAsync(int computerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/computers/{computerId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}