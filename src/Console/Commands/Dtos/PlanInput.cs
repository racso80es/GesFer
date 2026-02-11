using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class PlanInput : CommandInputBase
{
    public string Token { get; set; } = string.Empty;
    public string SpecLocation { get; set; } = string.Empty;
}
