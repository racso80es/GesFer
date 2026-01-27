using Xunit;

namespace GesFer.IntegrationTests;

/// <summary>
/// Colección de tests que comparten el mismo contenedor MySQL mediante Collection Fixture.
/// Esta colección garantiza que todos los tests compartan una única instancia de DatabaseFixture
/// que contiene el contenedor MySQL y la base de datos inicializada.
/// 
/// Patrón oficial de xUnit para compartir contexto entre múltiples clases de test.
/// </summary>
[CollectionDefinition("DatabaseStep", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Esta clase solo sirve como marcador para la colección de tests.
    // No necesita implementar ningún método.
    // xUnit creará una única instancia de DatabaseFixture que se compartirá
    // entre todas las clases de test marcadas con [Collection("DatabaseStep")]
}
