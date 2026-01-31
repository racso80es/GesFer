using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class CheckDockerInput : CommandInputBase { }

public class RemoveContainersInput : CommandInputBase { }

public class CreateContainersInput : CommandInputBase { }

public class WaitMySqlInput : CommandInputBase { }

public class InitializeDatabaseInput : CommandInputBase { }

public class InitializationResultData
{
    public string Status { get; set; } = "ko";
    public List<string> Information { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
}

public class ValidateIntegrityInput : CommandInputBase { }

public class IntegrityValidationResultData
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    // Add other fields from IntegrityValidationResult if needed
}

public class EnforceGoldenRulesInput : CommandInputBase
{
    public bool ForceFullCheck { get; set; }
}

public class GoldenRulesResultData
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool HasWarnings { get; set; }
    // Add other fields if needed
}
