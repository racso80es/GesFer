# Kaizen: Admin API Test Coverage & Config

## Objetivo
Incrementar la cobertura de tests en `GesFer.Admin.Api` (actualmente crítica < 30%) y mejorar la visibilidad de los resultados de tests en CI/CD.

## Tareas Principales
1.  **Tests Unitarios para `CompanyController`:**
    *   Implementar una suite de tests completa (`CompanyControllerTests.cs`) cubriendo todos los endpoints CRUD (`GetAll`, `GetById`, `Create`, `Update`, `Delete`).
    *   Asegurar cobertura de caminos felices y manejo de excepciones (404, 400, 500).

2.  **Refactorización del Ejecutor de Tests:**
    *   Modificar el comando de consola `RunUnitTestsCommand` (`src/Console/Commands/TestCommands.cs`).
    *   Configurar `dotnet test` para generar archivos de resultados `.trx` con nombres únicos por proyecto (`{ProjectName}_results.trx`).
    *   Evitar la sobreescritura de resultados cuando se ejecutan múltiples proyectos en paralelo o secuencia.

## Impacto Esperado
*   **Cobertura:** Aumento significativo en la cobertura de `GesFer.Admin.Api`.
*   **Diagnóstico:** Facilidad para identificar qué proyecto específico falló en CI gracias a los logs separados.
*   **Estabilidad:** Reducción de regresiones en la gestión de empresas administrativas.
