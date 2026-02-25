namespace GesFer.ConsoleApp.Commands.Dtos;

public class CustomerCommandInput
{
    // Empty for now as main menu is interactive
}

public class CustomerCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
