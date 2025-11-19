using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Unilocker.Client.Services;

public class ConfigService
{
    private readonly string _dataDirectory;
    private readonly string _machineIdFile;
    private readonly string _registeredFlagFile;

    public ConfigService()
    {
        // Leer configuración desde appsettings.json
        var appSettings = LoadAppSettings();
        _dataDirectory = appSettings["AppSettings"]?["DataDirectory"]?.ToString()
            ?? @"C:\ProgramData\Unilocker";
        _machineIdFile = appSettings["AppSettings"]?["MachineIdFile"]?.ToString()
            ?? "machine.id";
        _registeredFlagFile = appSettings["AppSettings"]?["RegisteredFlagFile"]?.ToString()
            ?? "registered.flag";

        // Asegurar que el directorio existe
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    /// <summary>
    /// Obtiene o crea un UUID único para esta máquina
    /// </summary>
    public Guid GetOrCreateMachineId()
    {
        string filePath = Path.Combine(_dataDirectory, _machineIdFile);

        // Si el archivo existe, leer el UUID
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            if (Guid.TryParse(content, out Guid existingId))
            {
                return existingId;
            }
        }

        // Si no existe o es inválido, crear nuevo UUID
        Guid newId = Guid.NewGuid();
        File.WriteAllText(filePath, newId.ToString());
        return newId;
    }

    /// <summary>
    /// Verifica si la computadora ya está registrada
    /// </summary>
    public bool IsComputerRegistered()
    {
        string filePath = Path.Combine(_dataDirectory, _registeredFlagFile);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Marca la computadora como registrada
    /// </summary>
    public void MarkAsRegistered(int computerId)
    {
        string filePath = Path.Combine(_dataDirectory, _registeredFlagFile);
        string content = $"Registered at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nComputer ID: {computerId}";
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Carga la configuración desde appsettings.json
    /// </summary>
    private JObject LoadAppSettings()
    {
        try
        {
            string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                string json = File.ReadAllText(appSettingsPath);
                return JObject.Parse(json);
            }
        }
        catch
        {
            // Si falla, usar valores por defecto
        }

        return new JObject();
    }

    /// <summary>
    /// Obtiene la URL base del API
    /// </summary>
    public string GetApiBaseUrl()
    {
        var appSettings = LoadAppSettings();
        return appSettings["ApiSettings"]?["BaseUrl"]?.ToString()
            ?? "https://localhost:5001";
    }
}