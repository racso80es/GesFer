using System.Collections.Generic;

namespace GesFer.ConsoleApp.Services.Interfaces;

public interface ISecurityScanner
{
    SecurityScanResult Scan(string content);
}

public class SecurityScanResult
{
    public bool IsCritical { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<string> Findings { get; set; } = new List<string>();
}
