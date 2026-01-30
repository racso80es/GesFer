namespace GesFer.ConsoleApp.Commands.Dtos;

public enum SeedScope
{
    Shared = 1,
    Admin = 2,
    Product = 3,
    All = 4
}

public enum SeedLevel
{
    Master = 1,
    Demo = 2,
    Test = 3
}

public class SeedCommandInput
{
    public SeedScope Scope { get; set; }
    public SeedLevel Level { get; set; }
}
