using System;
using System.IO;
using Microsoft.Win32;
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

    private readonly string _tokenFile = "auth_token.dat";
    private readonly string _computerIdFile = "computer.id";
    private readonly string _roleFile = "user_role.dat";
    private readonly string _computerNameFile = "computer_name.dat";

    /// <summary>
    /// Guarda el token JWT de forma cifrada
    /// </summary>
    public void SaveToken(string token)
    {
        string filePath = Path.Combine(_dataDirectory, _tokenFile);
        // Cifrado simple (en producción usar ProtectedData)
        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        string encoded = Convert.ToBase64String(tokenBytes);
        File.WriteAllText(filePath, encoded);
    }

    /// <summary>
    /// Obtiene el token JWT guardado
    /// </summary>
    public string? GetStoredToken()
    {
        string filePath = Path.Combine(_dataDirectory, _tokenFile);
        if (!File.Exists(filePath))
            return null;

        try
        {
            string encoded = File.ReadAllText(filePath);
            byte[] tokenBytes = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Elimina el token guardado
    /// </summary>
    public void ClearToken()
    {
        string filePath = Path.Combine(_dataDirectory, _tokenFile);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Guarda el ID de la computadora registrada
    /// </summary>
    public void SaveComputerId(int computerId)
    {
        string filePath = Path.Combine(_dataDirectory, _computerIdFile);
        File.WriteAllText(filePath, computerId.ToString());
    }

    /// <summary>
    /// Obtiene el ID de la computadora guardado
    /// </summary>
    public int? GetStoredComputerId()
    {
        string filePath = Path.Combine(_dataDirectory, _computerIdFile);
        if (!File.Exists(filePath))
            return null;

        try
        {
            string content = File.ReadAllText(filePath);
            if (int.TryParse(content, out int computerId))
            {
                return computerId;
            }
        }
        catch
        {
            // Ignorar errores
        }

        return null;
    }

    /// <summary>
    /// Guarda el nombre del equipo registrado
    /// </summary>
    public void SaveComputerName(string computerName)
    {
        string filePath = Path.Combine(_dataDirectory, _computerNameFile);
        File.WriteAllText(filePath, computerName);
    }

    /// <summary>
    /// Obtiene el nombre del equipo guardado
    /// </summary>
    public string? GetStoredComputerName()
    {
        string filePath = Path.Combine(_dataDirectory, _computerNameFile);
        if (!File.Exists(filePath))
            return null;

        try
        {
            return File.ReadAllText(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Desregistra la computadora eliminando los archivos de configuración
    /// </summary>
    public bool UnregisterComputer()
    {
        try
        {
            string registeredFlagPath = Path.Combine(_dataDirectory, _registeredFlagFile);
            string computerIdPath = Path.Combine(_dataDirectory, _computerIdFile);
            string computerNamePath = Path.Combine(_dataDirectory, _computerNameFile);
            string tokenPath = Path.Combine(_dataDirectory, _tokenFile);
            string machineIdPath = Path.Combine(_dataDirectory, _machineIdFile);

            // Eliminar archivos
            if (File.Exists(registeredFlagPath))
                File.Delete(registeredFlagPath);

            if (File.Exists(computerIdPath))
                File.Delete(computerIdPath);

            if (File.Exists(computerNamePath))
                File.Delete(computerNamePath);

            if (File.Exists(tokenPath))
                File.Delete(tokenPath);

            if (File.Exists(machineIdPath))
                File.Delete(machineIdPath);

            string rolePath = Path.Combine(_dataDirectory, _roleFile);
            if (File.Exists(rolePath))
                File.Delete(rolePath);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Guarda el rol del usuario
    /// </summary>
    public void SaveUserRole(string roleName)
    {
        string filePath = Path.Combine(_dataDirectory, _roleFile);
        File.WriteAllText(filePath, roleName);
    }

    /// <summary>
    /// Obtiene el rol del usuario guardado
    /// </summary>
    public string? GetStoredUserRole()
    {
        string filePath = Path.Combine(_dataDirectory, _roleFile);
        if (!File.Exists(filePath))
            return null;

        try
        {
            return File.ReadAllText(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Registra la aplicación para inicio automático en Windows
    /// </summary>
    public bool SetStartupEnabled(bool enable)
    {
        try
        {
            const string appName = "UnilockerClient";
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName 
                ?? System.Reflection.Assembly.GetExecutingAssembly().Location;

            // Si es un archivo .dll (dotnet run en desarrollo), obtener la ruta del exe
            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                exePath = exePath.Replace(".dll", ".exe");
            }

            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key == null)
                    return false;

                if (enable)
                {
                    // Agregar al inicio
                    key.SetValue(appName, $"\"{exePath}\"");
                }
                else
                {
                    // Remover del inicio
                    if (key.GetValue(appName) != null)
                    {
                        key.DeleteValue(appName);
                    }
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al configurar inicio automático: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica si el inicio automático está habilitado
    /// </summary>
    public bool IsStartupEnabled()
    {
        try
        {
            const string appName = "UnilockerClient";

            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
            {
                if (key == null)
                    return false;

                return key.GetValue(appName) != null;
            }
        }
        catch
        {
            return false;
        }
    }
}