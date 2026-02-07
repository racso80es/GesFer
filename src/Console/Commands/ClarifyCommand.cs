using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesFer.ConsoleApp.Commands;

public class ClarifyCommand : ICommandHandler<ClarifyInput, string>
{
    private readonly IAuditorService _auditor;
    private readonly ISecurityScanner _security;
    private readonly LogService _logger;

    // Required sections in a Spec
    private readonly string[] _requiredSections = { "Context", "Goal", "Analysis", "Security", "Implementation Plan", "Verification" };

    public ClarifyCommand(IAuditorService auditor, ISecurityScanner security, LogService logger)
    {
        _auditor = auditor;
        _security = security;
        _logger = logger;
    }

    public async Task<CommandResult<string>> HandleAsync(ClarifyInput command)
    {
        Console.WriteLine($"[Clarify] Iniciando proceso de clarificación...");
        _logger.WriteLog($"[Clarify] Start process. Spec: {command.SpecPath}");

        // 1. Audit Token Validation
        if (!_auditor.ValidateToken(command.Token))
        {
            var msg = "Token de Auditor inválido o faltante. Acceso denegado.";
            _auditor.LogAccess("CLARIFY_ACTION", "Unknown", "DENIED", "Invalid Token");
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        // 2. Load Content
        string contentToAnalyze = command.Context;
        string specName = "AdHocContext";

        if (!string.IsNullOrWhiteSpace(command.SpecPath))
        {
            if (!File.Exists(command.SpecPath))
            {
                var msg = $"Archivo Spec no encontrado: {command.SpecPath}";
                _logger.WriteError(msg);
                return CommandResult<string>.Fail(msg);
            }
            contentToAnalyze = await File.ReadAllTextAsync(command.SpecPath);
            specName = Path.GetFileNameWithoutExtension(command.SpecPath);
        }

        if (string.IsNullOrWhiteSpace(contentToAnalyze))
        {
            return CommandResult<string>.Fail("No hay contenido para analizar (SpecPath vacío o Context vacío).");
        }

        // 3. Gap Analysis
        var gaps = IdentifyGaps(contentToAnalyze);

        if (!gaps.Any())
        {
            Console.WriteLine("[Clarify] No se detectaron gaps obvios. El spec parece completo estructuralmente.");
            _auditor.LogAccess("CLARIFY_ACTION", "Authorized", "SUCCESS", "No Gaps Found");
            return CommandResult<string>.Ok("", "No se requieren clarificaciones.");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[Clarify] Se detectaron {gaps.Count} áreas que requieren clarificación:");
        Console.ResetColor();

        // 4. Interactive Dialogue & Security Scan
        var clarifications = new Dictionary<string, string>();

        foreach (var gap in gaps)
        {
            Console.WriteLine($"\n--- GAP DETECTADO: {gap.ToUpper()} ---");
            Console.WriteLine($"Por favor, provee detalles para '{gap}' (o presiona Enter para omitir):");

            Console.Write("> ");
            var input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  (Omitido)");
                continue;
            }

            // Security Scan on Input
            var securityResult = _security.Scan(input);
            if (securityResult.IsCritical)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SEGURIDAD] Entrada rechazada. Riesgo Crítico: {string.Join(", ", securityResult.Findings)}");
                Console.ResetColor();
                _auditor.LogAccess("CLARIFY_INPUT", "Authorized", "BLOCKED", $"Critical Risk in input for {gap}");
                continue;
            }
            else if (securityResult.RiskLevel == "High")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[SEGURIDAD] Advertencia: {string.Join(", ", securityResult.Findings)}");
                Console.ResetColor();
            }

            clarifications.Add(gap, input);
        }

        if (!clarifications.Any())
        {
            Console.WriteLine("\n[Clarify] No se registraron clarificaciones.");
            return CommandResult<string>.Ok("", "Proceso finalizado sin cambios.");
        }

        // 5. Persistence
        var outputPath = await SaveClarificationsAsync(specName, clarifications);

        _auditor.LogAccess("CLARIFY_ACTION", "Authorized", "SUCCESS", $"Clarifications saved to {outputPath}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[Clarify] Reporte generado exitosamente: {outputPath}");
        Console.ResetColor();

        return CommandResult<string>.Ok(outputPath, "Clarificaciones guardadas.");
    }

    private List<string> IdentifyGaps(string content)
    {
        var gaps = new List<string>();

        // Check for missing sections
        foreach (var section in _requiredSections)
        {
            // Simple heuristic: check if section header exists (case insensitive)
            if (!content.Contains($"# {section}", StringComparison.OrdinalIgnoreCase) &&
                !content.Contains($"## {section}", StringComparison.OrdinalIgnoreCase))
            {
                gaps.Add($"Missing Section: {section}");
            }
        }

        // Check for TODOs
        if (content.Contains("TODO", StringComparison.OrdinalIgnoreCase))
        {
            gaps.Add("Unresolved TODOs");
        }

        // Check for specific vague terms (simple heuristic)
        if (content.Contains("TBD") || content.Contains("To Be Defined"))
        {
            gaps.Add("TBD / To Be Defined markers");
        }

        return gaps;
    }

    private async Task<string> SaveClarificationsAsync(string specName, Dictionary<string, string> clarifications)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# CLARIFICATION REPORT: {specName}");
        sb.AppendLine($"**Date:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Agent:** Clarification-Specialist (via GesFer.Console)");
        sb.AppendLine();
        sb.AppendLine("## Resolved Gaps");
        sb.AppendLine();

        foreach (var kvp in clarifications)
        {
            sb.AppendLine($"### {kvp.Key}");
            sb.AppendLine(kvp.Value);
            sb.AppendLine();
        }

        var fileName = $"{specName}_CLARIFICATIONS_{DateTime.UtcNow:yyyyMMdd-HHmm}.md";
        // If specName came from a file path, we might want to save next to it.
        // Assuming openspecs/specs/ structure for now based on context.
        var outputDir = "openspecs/specs";

        // If specName was a path, let's try to respect that directory if possible,
        // otherwise default to openspecs/specs
        if (specName.Contains(Path.DirectorySeparatorChar) || specName.Contains(Path.AltDirectorySeparatorChar))
        {
             // This is likely not happening due to Path.GetFileNameWithoutExtension above,
             // but good for robustness if logic changes.
             outputDir = Path.GetDirectoryName(specName) ?? "openspecs/specs";
        }
        else if (Directory.Exists("openspecs/specs"))
        {
            outputDir = "openspecs/specs";
        }
        else
        {
            outputDir = "docs/clarifications"; // Fallback
        }

        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, fileName);

        await File.WriteAllTextAsync(outputPath, sb.ToString());
        return outputPath;
    }
}
