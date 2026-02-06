using GesFer.ConsoleApp.Services.Interfaces;
using System;
using System.Linq;

namespace GesFer.ConsoleApp.Services;

public class SecurityScanner : ISecurityScanner
{
    private readonly string[] _criticalKeywords = new[]
    {
        "delete", "drop", "truncate", "revoke", "shutdown", "reboot", "format"
    };

    private readonly string[] _highRiskKeywords = new[]
    {
        "grant", "password", "secret", "key", "token", "auth", "admin", "update", "alter"
    };

    public SecurityScanResult Scan(string content)
    {
        var result = new SecurityScanResult();
        if (string.IsNullOrWhiteSpace(content)) return result;

        var lowerContent = content.ToLowerInvariant();

        foreach (var keyword in _criticalKeywords)
        {
            if (lowerContent.Contains(keyword))
            {
                result.IsCritical = true;
                result.RiskLevel = "CRITICAL";
                result.Findings.Add($"Found critical keyword: '{keyword}'");
            }
        }

        // Even if critical found, we scan for high risk too, or stop?
        // Let's scan everything to be thorough
        foreach (var keyword in _highRiskKeywords)
        {
            if (lowerContent.Contains(keyword))
            {
                if (!result.IsCritical && result.RiskLevel == "Low")
                {
                    result.RiskLevel = "High";
                }
                else if (result.RiskLevel == "Low")
                {
                    // If it was low, upgrade to High. If already Critical, stay Critical.
                     result.RiskLevel = "High";
                }

                result.Findings.Add($"Found high-risk keyword: '{keyword}'");
            }
        }

        // Ensure if critical is true, risk level is critical
        if (result.IsCritical) result.RiskLevel = "CRITICAL";

        return result;
    }
}
