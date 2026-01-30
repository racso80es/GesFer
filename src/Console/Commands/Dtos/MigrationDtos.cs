namespace GesFer.ConsoleApp.Commands.Dtos;

public class ApplyMigrationsInput
{
    // No arguments needed currently
}

public class CreateInitialMigrationInput
{
    // No arguments needed currently
}

public class SquashMigrationsInput
{
    // No arguments needed currently
}

public class EnsureEfToolInput
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
