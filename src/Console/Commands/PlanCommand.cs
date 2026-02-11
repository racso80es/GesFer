using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

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
        Console.WriteLine($"[Plan] Starting planning process for spec: {command.SpecLocation}...");
        _logger.WriteLog($"[Plan] Start process. Spec: {command.SpecLocation}");

        // 1. Audit Token Validation
        if (!_auditor.ValidateToken(command.Token))
        {
            var msg = "Invalid or missing Auditor Token. Access denied.";
            _auditor.LogAccess("PLAN_GENERATION", "Unknown", "DENIED", "Invalid Token");
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        // 2. Load Content
        if (!File.Exists(command.SpecLocation))
        {
            var msg = $"Spec file not found: {command.SpecLocation}";
            _logger.WriteError(msg);
            return CommandResult<string>.Fail(msg);
        }

        var specContent = await File.ReadAllTextAsync(command.SpecLocation);
        var specDir = Path.GetDirectoryName(command.SpecLocation);
        var specName = Path.GetFileNameWithoutExtension(command.SpecLocation);

        // Infer Clarification File
        var clarificationFileName = $"{specName}_CLARIFICATIONS.md";
        var clarificationPath = Path.Combine(specDir ?? "", clarificationFileName);
        var clarifyContent = string.Empty;

        if (File.Exists(clarificationPath))
        {
             clarifyContent = await File.ReadAllTextAsync(clarificationPath);
             Console.WriteLine($"[Plan] Found associated clarification file: {clarificationFileName}");
        }
        else
        {
             Console.WriteLine($"[Plan] No associated clarification file found ({clarificationFileName}). Proceeding with Spec only.");
        }

        // 3. Extract Data (Simple heuristics for now)
        // We could implement more robust parsing, but for now we just dump content.

        // 4. Generate Output Data (Markdown)
        var planId = $"PLAN-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        var mdContent = GenerateMarkdown(planId, command.SpecLocation, clarificationPath, specContent, clarifyContent);

        // 5. Save Files
        var outputDir = specDir; // Save in same directory as Spec
        if (string.IsNullOrWhiteSpace(outputDir)) outputDir = "openspecs/plans"; // Fallback

        var planFileName = $"{specName}_PLAN.md";
        var mdPath = Path.Combine(outputDir, planFileName);

        await File.WriteAllTextAsync(mdPath, mdContent);

        _auditor.LogAccess("PLAN_GENERATION", "Authorized", "SUCCESS", $"Plan generated: {mdPath}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Plan] Plan generated successfully!");
        Console.WriteLine($"  File: {mdPath}");
        Console.ResetColor();

        return CommandResult<string>.Ok(mdPath, "Plan generated successfully.");
    }

    private string GenerateMarkdown(string id, string specPath, string clarifyPath, string specContent, string clarifyContent)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# PLAN: {id}");
        sb.AppendLine($"**Date:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Source Spec:** {Path.GetFileName(specPath)}");
        if (!string.IsNullOrEmpty(clarifyContent))
        {
            sb.AppendLine($"**Source Clarify:** {Path.GetFileName(clarifyPath)}");
        }
        sb.AppendLine();

        sb.AppendLine("## 1. Goal & Context");
        sb.AppendLine("(Extracted from Spec)");
        // TODO: Smarter extraction
        sb.AppendLine("> Refer to original Spec for full details.");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(clarifyContent))
        {
            sb.AppendLine("## 2. Clarifications Integrated");
            sb.AppendLine("Key points from clarification phase:");
            sb.AppendLine("> Refer to Clarification document for full details.");
            sb.AppendLine();
        }

        sb.AppendLine("## 3. Implementation Plan (Task Roadmap)");
        sb.AppendLine("<!-- Use structured action tags: [REF-VO], [FIX-LOG], [TEST], etc. -->");
        sb.AppendLine();
        sb.AppendLine("### Phase 1: Setup & Configuration");
        sb.AppendLine("- [ ] Create/Update JSON Configuration structures.");
        sb.AppendLine("- [ ] Verify directory permissions.");
        sb.AppendLine();
        sb.AppendLine("### Phase 2: Core Logic");
        sb.AppendLine("- [ ] Implement parsing logic for `initial.json` and `services.json`.");
        sb.AppendLine("- [ ] Add Unit Tests for parsers.");
        sb.AppendLine();
        sb.AppendLine("### Phase 3: UI Implementation");
        sb.AppendLine("- [ ] Create Project Selection View (Tabs).");
        sb.AppendLine("- [ ] Create Service Dashboard View.");
        sb.AppendLine();
        sb.AppendLine("### Phase 4: Integration & Verification");
        sb.AppendLine("- [ ] Implement `Verify_Status` HTTP check.");
        sb.AppendLine("- [ ] Verify SSL handling behavior.");
        sb.AppendLine("- [ ] Manual E2E Verification.");
        sb.AppendLine();

        sb.AppendLine("## 4. Risks & Mitigation");
        sb.AppendLine("- [ ] Risk: JSON Schema validation errors.");
        sb.AppendLine("  - *Mitigation:* strict schema validation tests.");

        return sb.ToString();
    }
}
