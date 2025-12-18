using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Unilocker.Client.Helpers;

/// <summary>
/// Helper para activar modo kiosco completo que bloquea todas las formas de escape:
/// - Alt+Tab (cambiar ventanas)
/// - Win+Tab (vista de tareas)
/// - Win+D (mostrar escritorio)
/// - Ctrl+Esc (menú inicio)
/// - Win (tecla Windows)
/// - Alt+F4 (cerrar ventana)
/// - Ctrl+Alt+Del (interceptado a nivel de sistema)
/// </summary>
public class KioskModeHelper
{
    private static IntPtr _hookID = IntPtr.Zero;
    private static LowLevelKeyboardProc? _proc;
    private static bool _isKioskModeActive = false;

    // Códigos de teclas virtuales de Windows
    private const int VK_LWIN = 0x5B;          // Tecla Windows izquierda
    private const int VK_RWIN = 0x5C;          // Tecla Windows derecha
    private const int VK_TAB = 0x09;           // Tab
    private const int VK_ESCAPE = 0x1B;        // Escape
    private const int VK_F4 = 0x73;            // F4
    private const int VK_DELETE = 0x2E;        // Delete
    private const int VK_CONTROL = 0x11;       // Control
    private const int VK_MENU = 0x12;          // Alt

    // Constantes para SetWindowsHookEx
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    // Delegado para el hook de teclado
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    #region Importaciones de WinAPI

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion

    /// <summary>
    /// Activa el modo kiosco bloqueando todas las combinaciones de teclas peligrosas
    /// </summary>
    public static void EnableKioskMode()
    {
        if (_isKioskModeActive)
        {
            Debug.WriteLine("⚠️ KioskMode ya está activo");
            return;
        }

        _proc = HookCallback;
        _hookID = SetHook(_proc);
        _isKioskModeActive = true;

        Debug.WriteLine("🔒 MODO KIOSCO ACTIVADO - Teclas del sistema bloqueadas");
    }

    /// <summary>
    /// Desactiva el modo kiosco permitiendo todas las teclas
    /// </summary>
    public static void DisableKioskMode()
    {
        if (!_isKioskModeActive)
        {
            Debug.WriteLine("⚠️ KioskMode no está activo");
            return;
        }

        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }

        _isKioskModeActive = false;
        Debug.WriteLine("🔓 MODO KIOSCO DESACTIVADO - Teclas del sistema desbloqueadas");
    }

    /// <summary>
    /// Verifica si el modo kiosco está activo
    /// </summary>
    public static bool IsKioskModeActive => _isKioskModeActive;

    /// <summary>
    /// Instala el hook de teclado de bajo nivel
    /// </summary>
    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule == null || curModule.ModuleName == null)
            {
                Debug.WriteLine("❌ No se pudo obtener el módulo actual para instalar el hook");
                return IntPtr.Zero;
            }

            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    /// <summary>
    /// Callback del hook que intercepta TODAS las teclas antes de que lleguen a la aplicación
    /// </summary>
    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // Detectar si están presionadas teclas modificadoras
            bool isCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            bool isAltPressed = (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
            bool isShiftPressed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            // BLOQUEAR: Tecla Windows (izquierda o derecha)
            if (vkCode == VK_LWIN || vkCode == VK_RWIN)
            {
                Debug.WriteLine($"🚫 Bloqueado: Tecla Windows");
                return (IntPtr)1; // Bloquear
            }

            // BLOQUEAR: Alt + Tab (cambiar ventanas)
            if (isAltPressed && vkCode == VK_TAB)
            {
                Debug.WriteLine($"🚫 Bloqueado: Alt+Tab");
                return (IntPtr)1; // Bloquear
            }

            // BLOQUEAR: Alt + F4 (cerrar ventana)
            if (isAltPressed && vkCode == VK_F4)
            {
                Debug.WriteLine($"🚫 Bloqueado: Alt+F4");
                return (IntPtr)1; // Bloquear
            }

            // BLOQUEAR: Ctrl + Esc (menú inicio)
            if (isCtrlPressed && vkCode == VK_ESCAPE)
            {
                Debug.WriteLine($"🚫 Bloqueado: Ctrl+Esc");
                return (IntPtr)1; // Bloquear
            }

            // BLOQUEAR: Ctrl + Alt + Delete (es más complejo, pero bloqueamos el inicio)
            if (isCtrlPressed && isAltPressed && vkCode == VK_DELETE)
            {
                Debug.WriteLine($"🚫 Bloqueado: Ctrl+Alt+Delete");
                return (IntPtr)1; // Bloquear
            }

            // BLOQUEAR: Ctrl + Shift + Esc (Administrador de tareas)
            if (isCtrlPressed && isShiftPressed && vkCode == VK_ESCAPE)
            {
                Debug.WriteLine($"🚫 Bloqueado: Ctrl+Shift+Esc (Administrador de tareas)");
                return (IntPtr)1; // Bloquear
            }

            // Aquí puedes agregar más combinaciones si lo deseas
        }

        // Permitir que la tecla continúe al siguiente hook
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}
