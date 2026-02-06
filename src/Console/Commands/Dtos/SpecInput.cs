using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class SpecInput : CommandInputBase
{
    public string Token { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Title { get; set; } = "Untitled";
}
