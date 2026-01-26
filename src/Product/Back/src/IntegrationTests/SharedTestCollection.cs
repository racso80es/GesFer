using Xunit;

namespace GesFer.IntegrationTests;

/// <summary>
/// Colección de tests que se ejecutan de forma secuencial para evitar interferencias
/// entre tests que comparten la misma base de datos.
/// 
/// Esta colección desactiva la paralelización para garantizar que los tests
/// se ejecuten uno tras otro, eliminando problemas de concurrencia en la BD compartida.
/// </summary>
[CollectionDefinition("SequentialTests", DisableParallelization = true)]
public class SharedTestCollection
{
    // Esta clase solo sirve como marcador para la colección de tests.
    // No necesita implementar ningún método.
}
