using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

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
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ComputerResponse>();
            if (result == null)
            {
                throw new Exception("La respuesta del servidor está vacía");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error de conexión con el servidor: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al registrar computadora: {ex.Message}", ex);
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
}