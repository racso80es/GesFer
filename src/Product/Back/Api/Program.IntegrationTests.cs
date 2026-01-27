// Archivo para hacer Program accesible para tests de integración
// Los top-level statements generan una clase Program interna automáticamente
// Esta declaración parcial la hace pública para los tests
namespace GesFer.Api;

/// <summary>
/// Clase Program para tests de integración
/// </summary>
public partial class Program
{
    // Esta clase permite que WebApplicationFactory pueda acceder a Program
}

