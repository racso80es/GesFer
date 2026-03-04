# Auditoría Backend (Fase S+)
**Fecha:** 2026-03-04 UTC

## 1. Métricas de Salud (0-100%)
*   **Arquitectura:** 90% (Compilación exitosa, pero con SRP violations en Consola y lógica monolítica en Comandos).
*   **Nomenclatura:** 95% (Bien estructurado bajo `GesFer.*`).
*   **Estabilidad Async:** 98% (No se encontraron patrones perjudiciales explícitos "Fire and Forget" más allá de los Sinks de Log permitidos).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

*   🟡 **Medio:** SRP Violation - Duplicación de `DevelopmentHostEnvironment`
    *   **Hallazgo:** La clase anidada privada `DevelopmentHostEnvironment` se duplica tanto en `InitializeDatabaseCommand.cs` como en `SeedCommand.cs`. Esto rompe el Principio de Responsabilidad Única y dificulta el testing unitario y la reutilización de código de infraestructura en la Consola.
    *   **Ubicación:**
        *   `src/Console/Commands/InitializeDatabaseCommand.cs:368`
        *   `src/Console/Commands/SeedCommand.cs:223`

*   🟡 **Medio:** Inicialización Monolítica en Consola (DI Configuration)
    *   **Hallazgo:** Tanto `InitializeDatabaseCommand` como `SeedCommand` registran manualmente un `ServiceProvider` gigantesco configurando explícitamente `IConfiguration`, `DbContext`, `Seeder`, y múltiples servicios (`IMigrationService`, `IIntegrityCheckService`). Esto no escala y es propenso a errores al agregar nuevas dependencias en las APIs.
    *   **Ubicación:**
        *   `src/Console/Commands/InitializeDatabaseCommand.cs` (método HandleAsync)
        *   `src/Console/Commands/SeedCommand.cs` (método HandleAsync)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Extraer `DevelopmentHostEnvironment` a `ConsoleServiceFactory` / `Services`
**Instrucciones para el Executor:**
1. Crea un nuevo archivo `src/Console/Services/DevelopmentHostEnvironment.cs` (o colócalo bajo un espacio de nombres de Infraestructura/Compartido en Consola).
2. Extrae la clase anidada de los comandos a este archivo como una clase `public`.
3. Actualiza `InitializeDatabaseCommand.cs` y `SeedCommand.cs` para utilizar esta nueva clase pública compartida.

**Definition of Done (DoD):**
*   La clase anidada `DevelopmentHostEnvironment` no existe en ningún archivo de comandos.
*   El proyecto `GesFer.Console` compila correctamente.

### Acción 2: Refactorizar la Inyección de Dependencias de Consola (Extra)
**Instrucciones para el Executor:**
1. Evaluar mover la construcción del `ServiceCollection` que ocurre dentro de `InitializeDatabaseCommand` y `SeedCommand` a un `ConsoleServiceFactory` centralizado en `src/Console/Services/` para evitar duplicar el bloque de `.AddDbContext<ApplicationDbContext>`, `.AddSingleton<IConfiguration>`, etc.

**Definition of Done (DoD):**
*   Un servicio central o método de extensión configura los DbContext y servicios compartidos para los comandos.
