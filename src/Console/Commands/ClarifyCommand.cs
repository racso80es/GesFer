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

    public ClarifyCommand(IAuditorService auditor, ISecurityScanner security, LogService logger)
    {
        _auditor = auditor;
        _security = security;
        _logger = logger;
    }

    public async Task<CommandResult<string>> HandleAsync(ClarifyInput command)
    {
        Console.WriteLine($"[Clarify] Iniciando proceso de clarificación...");
        _logger.WriteLog($"[Clarify] Start process. Spec: {command.SpecLocation}");

        // 1. Audit Token Validation
        if (!_auditor.ValidateToken(command.Token))
        {
            var msg = "Token de Auditor inválido o faltante. Acceso denegado.";
            _auditor.LogAccess("CLARIFY_ACTION", "Unknown", "DENIED", "Invalid Token");
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        // 2. Validate Spec Location
        if (string.IsNullOrWhiteSpace(command.SpecLocation) || !File.Exists(command.SpecLocation))
        {
            var msg = $"Archivo Spec no encontrado o ruta vacía: {command.SpecLocation}";
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        string specName = Path.GetFileNameWithoutExtension(command.SpecLocation);
        string specDir = Path.GetDirectoryName(command.SpecLocation) ?? string.Empty;

        // 3. Determine and Enforce Output Directory Structure
        string targetDir = specDir;

        // Check if we are in a Feature directory context (Kalma2/Docs/Feature/...)
        // and if the file is not already in its own dedicated folder.
        // We look for "Feature" in the path and check if the parent folder name matches the spec name.
        bool isFeatureContext = specDir.Contains("Feature", StringComparison.OrdinalIgnoreCase);
        string parentDirName = new DirectoryInfo(specDir).Name;

        if (isFeatureContext && !string.Equals(parentDirName, specName, StringComparison.OrdinalIgnoreCase))
        {
            // Migration Logic: Create dedicated folder and move spec
            targetDir = Path.Combine(specDir, specName);
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    _logger.WriteLog($"[Clarify] Created dedicated feature directory: {targetDir}");
                }

                string newSpecPath = Path.Combine(targetDir, Path.GetFileName(command.SpecLocation));
                File.Move(command.SpecLocation, newSpecPath);
                _logger.WriteLog($"[Clarify] Migrated spec file to: {newSpecPath}");

                // Update spec location for further processing
                command.SpecLocation = newSpecPath;
            }
            catch (Exception ex)
            {
                _logger.WriteError($"[Clarify] Failed to migrate spec file structure: {ex.Message}", ex);
                return CommandResult<string>.Fail($"Error migrando estructura de archivos: {ex.Message}");
            }
        }

        // 4. Generate Clarification File
        string clarificationFileName = $"{specName}_CLARIFICATIONS.md";
        string clarificationPath = Path.Combine(targetDir, clarificationFileName);

        var sb = new StringBuilder();

        // If file exists, append; otherwise create new
        if (File.Exists(clarificationPath))
        {
            sb.AppendLine();
            sb.AppendLine($"---");
            sb.AppendLine();
            sb.AppendLine($"### Update: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        }
        else
        {
            sb.AppendLine($"# CLARIFICATION REPORT: {specName}");
            sb.AppendLine($"**Created:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"**Spec File:** {Path.GetFileName(command.SpecLocation)}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(command.Content))
        {
            // Security Scan on Input
            var securityResult = _security.Scan(command.Content);
            if (securityResult.IsCritical)
            {
                 var msg = $"[SEGURIDAD] Entrada rechazada. Riesgo Crítico: {string.Join(", ", securityResult.Findings)}";
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine(msg);
                 Console.ResetColor();
                 _auditor.LogAccess("CLARIFY_INPUT", "Authorized", "BLOCKED", msg);
                 return CommandResult<string>.Fail(msg);
            }

            sb.AppendLine("## Input / Context");
            sb.AppendLine(command.Content);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("## Analysis");
            sb.AppendLine("No additional context provided. Pending manual review.");
            sb.AppendLine();
        }

        // 5. Write to File
        await File.AppendAllTextAsync(clarificationPath, sb.ToString());

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Clarify] Archivo de clarificación actualizado/creado: {clarificationPath}");
        Console.ResetColor();
        _auditor.LogAccess("CLARIFY_ACTION", "Authorized", "SUCCESS", $"Clarification saved to {clarificationPath}");

        return CommandResult<string>.Ok(clarificationPath, "Clarificación registrada exitosamente.");
    }
}
