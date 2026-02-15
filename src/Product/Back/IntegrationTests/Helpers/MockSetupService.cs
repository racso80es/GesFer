using GesFer.Api.Services;

namespace GesFer.IntegrationTests.Helpers;

/// <summary>
/// Mock de ISetupService para tests. Evita ejecutar Docker en Initialize_EndpointShouldExist.
/// </summary>
public class MockSetupService : ISetupService
{
    public Task<SetupResult> InitializeEnvironmentAsync()
    {
        return Task.FromResult(new SetupResult
        {
            Success = true,
            Steps = new List<string> { "Mock: Inicialización simulada para tests" },
            Message = "Tests: Docker no ejecutado"
        });
    }
}
