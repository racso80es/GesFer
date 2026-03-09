using GesFer.ConsoleApp.Commands.Base;

namespace GesFer.ConsoleApp.Commands.Dtos;

public class ApplyMigrationsInput : CommandInputBase
{
    // No arguments needed currently
}

public class CreateInitialMigrationInput : CommandInputBase
{
    public string Domain { get; set; } = "Product";
}

public class SquashMigrationsInput : CommandInputBase
{
    // No arguments needed currently
}

public class EnsureEfToolInput : CommandInputBase
{
    // No arguments needed currently
}

public class MigrationSquashResultData
{
    public int DeletedFilesCount { get; set; }
    public int CreatedFilesCount { get; set; }
    public List<string> CreatedFiles { get; set; } = new List<string>();
    public List<string> TablesFound { get; set; } = new List<string>();
    public int TotalTablesInMigration { get; set; }
}
