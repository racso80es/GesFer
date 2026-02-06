using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.IO;
using System.Text.Json;

namespace GesFer.ConsoleApp.Services;

public class AuditorService : IAuditorService
{
    // Using relative paths assuming execution from repo root
    private const string AGENT_PATH = "openspecs/agents/auditor/process-interaction.json";
    private const string LOG_PATH = "docs/audits/ACCESS_LOG.md";
    private string? _cachedToken;

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            if (_cachedToken == null)
            {
                if (!File.Exists(AGENT_PATH))
                {
                    Console.WriteLine($"[Auditor] Warning: Config file not found at {AGENT_PATH}");
                    return false;
                }

                var json = File.ReadAllText(AGENT_PATH);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("auditor_token", out var tokenElement))
                {
                    _cachedToken = tokenElement.GetString();
                }
            }

            return string.Equals(token, _cachedToken, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auditor] Error reading config: {ex.Message}");
            return false;
        }
    }

    public void LogAccess(string action, string user, string result, string details = "")
    {
        try
        {
            var logEntry = $"| {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {action} | {user} | {result} | {details} |{Environment.NewLine}";

            var dir = Path.GetDirectoryName(LOG_PATH);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(LOG_PATH))
            {
                File.WriteAllText(LOG_PATH, "| Timestamp | Action | User | Result | Details |\n|---|---|---|---|---|\n");
            }

            File.AppendAllText(LOG_PATH, logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auditor] Error logging access: {ex.Message}");
        }
    }
}
