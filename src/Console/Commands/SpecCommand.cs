using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GesFer.ConsoleApp.Commands;

public class SpecCommand : ICommandHandler<SpecInput, string>
{
    private readonly IAuditorService _auditor;
    private readonly ISecurityScanner _security;
    private readonly LogService _logger;

    public SpecCommand(IAuditorService auditor, ISecurityScanner security, LogService logger)
    {
        _auditor = auditor;
        _security = security;
        _logger = logger;
    }

    public async Task<CommandResult<string>> HandleAsync(SpecInput command)
    {
        Console.WriteLine($"[Spec] Iniciando generación: {command.Title}...");
        _logger.WriteLog($"[Spec] Start generation: {command.Title}");

        // 1. Audit Token
        if (!_auditor.ValidateToken(command.Token))
        {
            var msg = "Token de Auditor inválido o faltante. Acceso denegado.";
            _auditor.LogAccess("SPEC_GENERATION", "Unknown", "DENIED", "Invalid Token");
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        // 2. Security Scan
        var securityResult = _security.Scan(command.Content);
        if (securityResult.IsCritical)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[SEGURIDAD] ALERTA CRÍTICA DETECTADA");
            Console.ResetColor();
            _logger.WriteLog("[SECURITY] CRITICAL RISK DETECTED");

            foreach (var finding in securityResult.Findings)
            {
                Console.WriteLine($"  - {finding}");
                _logger.WriteLog($"  - {finding}");
            }

            _auditor.LogAccess("SPEC_GENERATION", "Authorized", "WARNING", "Critical Keywords: " + string.Join(", ", securityResult.Findings));
        }
        else if (securityResult.RiskLevel == "High")
        {
             Console.ForegroundColor = ConsoleColor.Yellow;
             Console.WriteLine("[SEGURIDAD] Advertencia de riesgo alto");
             Console.ResetColor();
             foreach (var finding in securityResult.Findings)
             {
                 Console.WriteLine($"  - {finding}");
             }
             _auditor.LogAccess("SPEC_GENERATION", "Authorized", "SUCCESS", "High Risk Keywords: " + string.Join(", ", securityResult.Findings));
        }
        else
        {
             _auditor.LogAccess("SPEC_GENERATION", "Authorized", "SUCCESS");
        }

        // 3. Load Template
        var templatePath = "openspecs/templates/spec-template.md";
        string templateContent;
        if (File.Exists(templatePath))
        {
            templateContent = await File.ReadAllTextAsync(templatePath);
        }
        else
        {
            templateContent = "# SPEC: {TITLE}\n\n{CONTEXT}\n\n{GOAL}\n\n{SECURITY_ANALYSIS}";
            _logger.WriteLog($"Template not found at {templatePath}, using fallback.");
        }

        // 4. Fill Template
        var content = templateContent
            .Replace("{TITLE}", command.Title)
            .Replace("{ID}", $"SPEC-{DateTime.UtcNow:yyyyMMdd-HHmm}")
            .Replace("{DATE}", DateTime.UtcNow.ToString("yyyy-MM-dd"))
            .Replace("{AUTHOR}", "Agent-Spec")
            .Replace("{CONTEXT}", "Generado desde CLI")
            .Replace("{GOAL}", command.Content)
            .Replace("{SECURITY_ANALYSIS}", $"Nivel de Riesgo: {securityResult.RiskLevel}\nHallazgos:\n- " + string.Join("\n- ", securityResult.Findings));

        // 5. Determine Output Path
        var baseDir = string.IsNullOrWhiteSpace(command.Context) ? "openspecs/specs" : command.Context;
        var fileName = $"SPEC-{DateTime.UtcNow:yyyyMMdd-HHmm}-{SanitizeFilename(command.Title)}.md";
        var outputPath = Path.Combine(baseDir, fileName);

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        // 6. Write File
        await File.WriteAllTextAsync(outputPath, content);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Spec] Archivo generado: {outputPath}");
        Console.ResetColor();
        _logger.WriteLog($"[Spec] File generated: {outputPath}");

        return CommandResult<string>.Ok(outputPath, "Spec generada exitosamente.");
    }

    private string SanitizeFilename(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var result = name;
        foreach (var c in invalidChars)
        {
            result = result.Replace(c, '_');
        }
        return result.Replace(" ", "_");
    }
}
