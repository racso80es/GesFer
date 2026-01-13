using Xunit;

namespace GesFer.IntegrationTests;

/// <summary>
/// Fixture compartido para todos los tests en la colección "DatabaseStep".
/// Contiene la instancia única de IntegrationTestWebAppFactory que gestiona
/// el contenedor MySQL compartido entre todas las clases de test.
/// 
/// Esta clase se instancia una sola vez al inicio de la colección de tests
/// y se limpia al finalizar todos los tests.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public IntegrationTestWebAppFactory<GesFer.Api.Program> Factory { get; private set; } = null!;

    /// <summary>
    /// Acceso a los servicios del contenedor de inyección de dependencias.
    /// Útil para tests que necesitan acceder directamente al DbContext u otros servicios.
    /// </summary>
    public IServiceProvider Services => Factory.Services;

    /// <summary>
    /// Inicializa el fixture compartido. Se ejecuta una sola vez antes de todos los tests.
    /// Crea y configura el IntegrationTestWebAppFactory que levanta el contenedor MySQL.
    /// </summary>
    public async Task InitializeAsync()
    {
        Factory = new IntegrationTestWebAppFactory<GesFer.Api.Program>();
        await Factory.InitializeAsync();
    }

    /// <summary>
    /// Limpia el fixture compartido. Se ejecuta una sola vez después de todos los tests.
    /// Destruye el contenedor MySQL y libera los recursos.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }
    }
}
