using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace GesFer.ConsoleApp.Commands;

public class PlanCommand : ICommandHandler<PlanInput, string>
{
    private readonly IAuditorService _auditor;
    private readonly ISecurityScanner _security;
    private readonly LogService _logger;

    public PlanCommand(IAuditorService auditor, ISecurityScanner security, LogService logger)
    {
        _auditor = auditor;
        _security = security;
        _logger = logger;
    }

    public async Task<CommandResult<string>> HandleAsync(PlanInput command)
    {
        Console.WriteLine($"[Plan] Starting planning process for spec: {command.SpecPath}...");
        _logger.WriteLog($"[Plan] Start process. Spec: {command.SpecPath}");

        // 1. Audit Token Validation
        if (!_auditor.ValidateToken(command.Token))
        {
            var msg = "Invalid or missing Auditor Token. Access denied.";
            _auditor.LogAccess("PLAN_GENERATION", "Unknown", "DENIED", "Invalid Token");
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        // 2. Load Content
        if (!File.Exists(command.SpecPath))
        {
            var msg = $"Spec file not found: {command.SpecPath}";
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        var specContent = await File.ReadAllTextAsync(command.SpecPath);
        var clarifyContent = string.Empty;

        if (!string.IsNullOrWhiteSpace(command.ClarifyPath))
        {
             if (File.Exists(command.ClarifyPath))
             {
                 clarifyContent = await File.ReadAllTextAsync(command.ClarifyPath);
             }
             else
             {
                 Console.WriteLine($"[Plan] Warning: Clarify file not found: {command.ClarifyPath}");
             }
        }

        // 3. Extract Data
        var goal = ExtractSection(specContent, "Goal");
        var context = ExtractSection(specContent, "Context");
        var clarifications = ExtractClarifications(clarifyContent);

        // 4. Generate Output Data (JSON)
        var planId = $"PLAN-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        var planData = new
        {
            id = planId,
            timestamp = DateTime.UtcNow,
            auditor_token = command.Token,
            source_spec = command.SpecPath,
            source_clarify = command.ClarifyPath,
            goal = goal,
            context = context,
            clarifications = clarifications,
            steps = new string[] { },
            risks = new string[] { }
        };

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var jsonContent = JsonSerializer.Serialize(planData, jsonOptions);

        // 5. Generate Output Data (Markdown)
        var mdContent = GenerateMarkdown(planId, command.SpecPath, command.ClarifyPath, goal, context, clarifications);

        // 6. Save Files
        var outputDir = "openspecs/plans";
        Directory.CreateDirectory(outputDir);

        var specName = Path.GetFileNameWithoutExtension(command.SpecPath);
        specName = string.Join("_", specName.Split(Path.GetInvalidFileNameChars()));

        var jsonPath = Path.Combine(outputDir, $"{planId}-{specName}.json");
        var mdPath = Path.Combine(outputDir, $"{planId}-{specName}.md");

        await File.WriteAllTextAsync(jsonPath, jsonContent);
        await File.WriteAllTextAsync(mdPath, mdContent);

        _auditor.LogAccess("PLAN_GENERATION", "Authorized", "SUCCESS", $"Plan generated: {mdPath}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Plan] Plan generated successfully!");
        Console.WriteLine($"  JSON: {jsonPath}");
        Console.WriteLine($"  MD:   {mdPath}");
        Console.ResetColor();

        return CommandResult<string>.Ok(mdPath, "Plan generated successfully.");
    }

    private string ExtractSection(string content, string sectionName)
    {
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var result = new System.Text.StringBuilder();
        bool capturing = false;

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("#"))
            {
                if (capturing) break;

                if (Regex.IsMatch(line, $@"^#+\s*{Regex.Escape(sectionName)}", RegexOptions.IgnoreCase))
                {
                    capturing = true;
                    continue;
                }
            }

            if (capturing)
            {
                result.AppendLine(line);
            }
        }

        var extracted = result.ToString().Trim();
        return string.IsNullOrEmpty(extracted) ? "Not found in Spec." : extracted;
    }

    private string[] ExtractClarifications(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Array.Empty<string>();

        var clarifications = new System.Collections.Generic.List<string>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var currentClarification = new System.Text.StringBuilder();
        bool inClarification = false;
        string currentTitle = "";

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("### "))
            {
                if (inClarification && currentClarification.Length > 0)
                {
                     clarifications.Add($"{currentTitle}: {currentClarification.ToString().Trim()}");
                     currentClarification.Clear();
                }

                currentTitle = line.TrimStart('#', ' ').Trim();
                inClarification = true;
            }
            else if (inClarification)
            {
                if (line.TrimStart().StartsWith("#") && !line.TrimStart().StartsWith("###"))
                {
                    inClarification = false;
                    if (currentClarification.Length > 0)
                    {
                         clarifications.Add($"{currentTitle}: {currentClarification.ToString().Trim()}");
                         currentClarification.Clear();
                    }
                }
                else
                {
                    currentClarification.AppendLine(line);
                }
            }
        }

        if (inClarification && currentClarification.Length > 0)
        {
             clarifications.Add($"{currentTitle}: {currentClarification.ToString().Trim()}");
        }

        return clarifications.ToArray();
    }

    private string GenerateMarkdown(string id, string specPath, string clarifyPath, string goal, string context, string[] clarifications)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# PLAN: {id}");
        sb.AppendLine($"**Date:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Source Spec:** {specPath}");
        if (!string.IsNullOrEmpty(clarifyPath))
        {
            sb.AppendLine($"**Source Clarify:** {clarifyPath}");
        }
        sb.AppendLine();

        sb.AppendLine("## Goal");
        sb.AppendLine(goal);
        sb.AppendLine();

        sb.AppendLine("## Context");
        sb.AppendLine(context);
        sb.AppendLine();

        if (clarifications != null && clarifications.Length > 0)
        {
            sb.AppendLine("## Clarifications Integrated");
            foreach (var c in clarifications)
            {
                sb.AppendLine($"- {c.Replace("\n", " ").Replace("\r", "")}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Implementation Plan (Task Roadmap)");
        sb.AppendLine("<!-- Use structured action tags: [REF-VO], [FIX-LOG], [TEST], etc. -->");
        sb.AppendLine();
        sb.AppendLine("- [ ] Step 1: Initialize...");
        sb.AppendLine("- [ ] Step 2: Implement...");
        sb.AppendLine("- [ ] Step 3: Verify...");
        sb.AppendLine();

        sb.AppendLine("## Risks & Mitigation");
        sb.AppendLine("- [ ] Risk 1: ...");

        return sb.ToString();
    }
}
