namespace GesFer.ConsoleApp.Commands.Dtos;

public class CheckDockerInput { }

public class RemoveContainersInput { }

public class CreateContainersInput { }

public class WaitMySqlInput { }

public class InitializeDatabaseInput { }

public class InitializationResultData
{
    public string Status { get; set; } = "ko";
    public List<string> Information { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
}

public class ValidateIntegrityInput { }

public class IntegrityValidationResultData
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    // Add other fields from IntegrityValidationResult if needed
}

public class EnforceGoldenRulesInput
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
