using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using GesFer.ConsoleApp.Services;
using GesFer.ConsoleApp.Commands;
using GesFer.ConsoleApp.Commands.Dtos;

namespace GesFer.Console.E2ETests;

[Trait("Category", "Heavy")]
public class Option1IntegrationTest
{
    private readonly ITestOutputHelper _output;

    public Option1IntegrationTest(ITestOutputHelper output)
    {
        _output = output;
        // Redirect Console.WriteLine to xUnit output for debugging
        System.Console.SetOut(new Converter(output));
    }

    [Fact]
    public async Task ExecuteOption1_FullInitialization_ShouldSucceed()
    {
        // 1. Setup Root Path
        // Test Bin: src/Console/tests/GesFer.Console.E2ETests/bin/Debug/net8.0/
        // Root is 7 levels up.
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", ".."));

        _output.WriteLine($"Root Path determined as: {rootPath}");
        Assert.True(File.Exists(Path.Combine(rootPath, "docker-compose.yml")), $"docker-compose.yml not found at {rootPath}");

        // 2. Instantiate Services
        var logService = new LogService(rootPath);
        _output.WriteLine($"Log file: {logService.GetLogFilePath()}");

        var checkDockerCommand = new CheckDockerCommand(logService);
        var checkDockerComposeCommand = new CheckDockerComposeCommand(logService);
        var removeContainersCommand = new RemoveContainersCommand(logService);
        var createContainersCommand = new CreateContainersCommand(logService);
        var waitMySqlReadyCommand = new WaitMySqlReadyCommand(logService);
        var applyMigrationsCommand = new ApplyMigrationsCommand(logService);
        var createInitialMigrationCommand = new CreateInitialMigrationCommand(logService);
        var squashMigrationsCommand = new SquashMigrationsCommand(logService);
        var ensureEfToolCommand = new EnsureEfToolCommand(logService);
        var seedCommand = new SeedCommand(logService);
        var initializeDatabaseCommand = new InitializeDatabaseCommand(logService);
        var integrityValidationService = new IntegrityValidationService(logService);
        var goldenRulesService = new GoldenRulesComplianceService(logService);

        var menuService = new MenuService(
            checkDockerCommand,
            checkDockerComposeCommand,
            removeContainersCommand,
            createContainersCommand,
            waitMySqlReadyCommand,
            applyMigrationsCommand,
            createInitialMigrationCommand,
            squashMigrationsCommand,
            ensureEfToolCommand,
            seedCommand,
            initializeDatabaseCommand,
            integrityValidationService,
            goldenRulesService,
            logService);

        // 3. Execute Option 1 (Full Initialization)
        _output.WriteLine("Starting Option 1 execution...");
        var success = await menuService.ExecuteOptionAsync(1, waitForInput: false);

        // 4. Assert Execution Success
        Assert.True(success, "Option 1 (Full Initialization) failed. Check logs.");
        _output.WriteLine("Option 1 completed successfully.");

        // 5. Verify Ecosystem Integrity
        _output.WriteLine("Starting Integrity Validation...");
        var validationResult = await integrityValidationService.ValidateEcosystemAsync(useCache: false);

        // 6. Assert Validation Success
        foreach (var error in validationResult.Errors)
        {
            _output.WriteLine($"Validation Error: {error}");
        }

        // Assert specific checks
        Assert.True(validationResult.Checks.ContainsKey("Docker") && validationResult.Checks["Docker"], "Docker check failed");
        Assert.True(validationResult.Checks.ContainsKey("Backend") && validationResult.Checks["Backend"], "Backend API check failed");
        Assert.True(validationResult.Checks.ContainsKey("AdminUsers") && validationResult.Checks["AdminUsers"], "AdminUsers check failed");

        // Note: We intentionally ignore ProductFront check because docker-compose.yml in option 1 usually doesn't start frontend,
        // so validationResult.IsValid might be false due to Frontend check failure.
    }

    private class Converter : TextWriter
    {
        ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
        public override void WriteLine(string? message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, object? arg0)
        {
            _output.WriteLine(string.Format(format, arg0));
        }
        public override void Write(char value)
        {
            // Ignore single chars to avoid spam
        }
    }
}
