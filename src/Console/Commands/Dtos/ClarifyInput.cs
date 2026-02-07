using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class ClarifyInput : CommandInputBase
{
    public string Token { get; set; } = string.Empty;
    public string SpecPath { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}
