using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class ClarifyInput : CommandInputBase
{
    public string Token { get; set; } = string.Empty;
    public string SpecLocation { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
