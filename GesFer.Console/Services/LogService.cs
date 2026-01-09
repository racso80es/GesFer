using System;
using System.IO;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para gestionar logs de la aplicación
/// </summary>
public class LogService
{
    private readonly string _logFilePath;
    private readonly string _rootPath;
    private readonly object _lockObject = new object();

    public LogService()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var logDir = Path.Combine(_rootPath, "logs");
        
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logFilePath = Path.Combine(logDir, $"gesfer-console_{timestamp}.log");
        
        // Crear el archivo de log inicial
        WriteLog("========================================");
        WriteLog("GesFer Console - Inicio de sesión");
        WriteLog($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLog("========================================");
        WriteLog("");
    }

    /// <summary>
    /// Escribe un mensaje en el log
    /// </summary>
    public void WriteLog(string message)
    {
        lock (_lockObject)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Si no se puede escribir el log, continuar sin fallar
            }
        }
    }

    /// <summary>
    /// Escribe un mensaje de error en el log
    /// </summary>
    public void WriteError(string message, Exception? exception = null)
    {
        WriteLog($"ERROR: {message}");
        if (exception != null)
        {
            WriteLog($"Exception: {exception.GetType().Name}");
            WriteLog($"Message: {exception.Message}");
            WriteLog($"Stack Trace: {exception.StackTrace}");
        }
    }

    /// <summary>
    /// Escribe la salida de un proceso en el log
    /// </summary>
    public void WriteProcessOutput(string processName, string output, bool isError = false)
    {
        var prefix = isError ? "STDERR" : "STDOUT";
        WriteLog($"[{processName}] {prefix}:");
        if (!string.IsNullOrWhiteSpace(output))
        {
            var lines = output.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                WriteLog($"  {line}");
            }
        }
        else
        {
            WriteLog("  (vacío)");
        }
    }

    /// <summary>
    /// Obtiene la ruta del archivo de log
    /// </summary>
    public string GetLogFilePath()
    {
        return _logFilePath;
    }

    /// <summary>
    /// Obtiene la ruta raíz del proyecto
    /// </summary>
    public string GetRootPath()
    {
        return _rootPath;
    }
}
