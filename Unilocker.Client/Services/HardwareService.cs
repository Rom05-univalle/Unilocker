using System;
using System.Management;
using Unilocker.Client.Models;

namespace Unilocker.Client.Services;

public class HardwareService
{
    /// <summary>
    /// Obtiene el nombre de la computadora
    /// </summary>
    public string GetComputerName()
    {
        return Environment.MachineName;
    }

    /// <summary>
    /// Obtiene la marca del fabricante (ej: Dell, HP, Lenovo)
    /// </summary>
    public string? GetManufacturer()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["Manufacturer"]?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo fabricante: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Obtiene el modelo de la computadora
    /// </summary>
    public string? GetModel()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["Model"]?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo modelo: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Obtiene el número de serie del equipo
    /// </summary>
    public string? GetSerialNumber()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo número de serie: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Obtiene toda la información del hardware en un solo método
    /// </summary>
    public HardwareInfo GetHardwareInfo()
    {
        return new HardwareInfo
        {
            ComputerName = GetComputerName(),
            Manufacturer = GetManufacturer(),
            Model = GetModel(),
            SerialNumber = GetSerialNumber()
        };
    }
}

