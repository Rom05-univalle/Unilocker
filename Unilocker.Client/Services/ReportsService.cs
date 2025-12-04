using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class ReportsService
{
    private readonly ApiService _apiService;

    public ReportsService(ApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Obtiene la lista de tipos de problema disponibles
    /// </summary>
    public async Task<List<ProblemType>> GetProblemTypesAsync()
    {
        try
        {
            return await _apiService.GetProblemTypesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener tipos de problema: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Crea un nuevo reporte
    /// </summary>
    public async Task<bool> CreateReportAsync(int sessionId, int problemTypeId, string description)
    {
        try
        {
            var request = new CreateReportRequest
            {
                SessionId = sessionId,
                ProblemTypeId = problemTypeId,
                Description = description
            };

            return await _apiService.CreateReportAsync(request);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al crear reporte: {ex.Message}", ex);
        }
    }
}