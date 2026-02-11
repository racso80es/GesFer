# AUDITORIA_TESTS_2026_02_11

**Fecha:** 11 de Febrero de 2026
**Auditor:** Agente QA / Kaizen Specialist
**Versión:** 1.0

## 1. Resumen Ejecutivo

La salud general del sistema de pruebas se califica como **B- (Aceptable con Riesgos Bloqueantes)**.

El subsistema de **Pruebas Unitarias** muestra una robustez excelente, con un 100% de tasa de éxito y una calidad de código alta (patrones AAA, nomenclatura clara, uso de FluentAssertions). Sin embargo, el subsistema de **Pruebas de Integración** presenta fallos críticos de infraestructura en el módulo Admin (bloqueo por Serilog) y fragilidad en los datos de prueba del módulo Product (validación de hashes y semillas).

Estos fallos en integración reducen la confianza en el despliegue continuo, a pesar de la solidez de la lógica de negocio aislada.

## 2. Dashboard de Métricas

| Proyecto | Tipo | Total Tests | Pasados | Fallados | Estado | Observaciones |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **GesFer.Shared.Back** | Unit | 5 | 5 | 0 | 🟢 OK | ~90% (Estimado) - Core logic intacta. |
| **GesFer.Admin** | Unit | 30 | 30 | 0 | 🟢 OK | ~85% (Estimado) - Alta fiabilidad en Handlers. |
| **GesFer.Admin** | Integ | 9 | 2 | **7** | 🔴 CRITICAL | <20% (Crítico) - Fallo sistémico de infraestructura (Logger). |
| **GesFer.Product** | Unit | 18 | 18 | 0 | 🟢 OK | ~80% (Estimado) - Servicios base correctos. |
| **GesFer.Product** | Integ | 102 | 99 | **3** | 🟡 WARN | ~75% (Estimado) - Fallos de datos/semilla específicos. |
| **GesFer.Console** | E2E | 2 | 2 | 0 | 🟢 OK | N/A (E2E) - Flujos principales operativos. |
| **GesFer.Architecture** | Arch | 1 | 1 | 0 | 🟢 OK | N/A (Arch) - Reglas de arquitectura cumplidas. |

**Total General:** 167 Tests | **Pasados:** 157 (94%) | **Fallados:** 10 (6%)

### Análisis de Cobertura Crítica

*   **GesFer.Admin (Integration):** Cobertura funcional severamente afectada por los fallos de inicialización. Requiere atención inmediata.
*   **GesFer.Product (Services):** Aunque los tests unitarios pasan, la validación de ValueObjects en integración revela brechas en la cobertura de escenarios negativos en unitarios.

## 3. Puntos de Dolor (Pain Points)

### A. Conflicto de Serilog en Tests de Integración (Admin)
*   **Error:** `System.InvalidOperationException: The logger is already frozen.`
*   **Impacto:** 7 de 9 tests fallan.
*   **Causa Raíz:** La instancia estática de Serilog (`Log.Logger`) se inicializa y "congela" en el primer test ejecutado por `WebApplicationFactory`. Los tests subsiguientes intentan reconfigurarla o usarla en un estado inválido.
*   **Ubicación:** `GesFer.Admin.IntegrationTests`.

### B. Fragilidad en Validación de Hash (Product)
*   **Error:** `Expected userFromDb.PasswordHash to be "$2a$11$IRko..." but found "$2a$11$FSAZ..."`.
*   **Impacto:** Falso negativo en test de Login.
*   **Causa Raíz:** El test `AuthControllerTests` compara el string exacto del hash BCrypt. Si el `SeedRunner` regenera el hash (aunque sea para la misma contraseña) o cambia el factor de coste/salt, el test falla. Esto es una aserción frágil.
*   **Ubicación:** `GesFer.IntegrationTests.Controllers.AuthControllerTests`.

### C. Inconsistencia en Datos de Semilla (Product)
*   **Error:** `Expected validCompany not to be <null>`.
*   **Impacto:** Tests de validación fallan.
*   **Causa Raíz:** El test espera que una compañía "válida" (ID terminada en `...12`) exista en la base de datos tras el seeding. Su ausencia sugiere que el proceso de seeding falló silenciosamente o que la entidad fue rechazada por validaciones de dominio más estrictas (posiblemente `TaxId`).
*   **Ubicación:** `GesFer.IntegrationTests.Services.ValueObjectValidationTests`.

## 4. Evaluación de Calidad de Código (Muestreo)

Se analizaron archivos representativos de Unit Tests:
*   `SetupServiceTests.cs` (Product): **Excelente**. Uso claro de AAA, Mocks (Moq) y aserciones fluidas. Manejo correcto de dependencias complejas (`IServiceScopeFactory`).
*   `CreateCompanyHandlerTests.cs` (Admin): **Muy Bueno**. Tests limpios, independientes, uso de `InMemoryDatabase` efectivo. Nomenclatura descriptiva (`Handle_WithValidDto_...`).

## 5. Acciones Kaizen (Plan de Mejora)

Para la próxima jornada, se recomiendan las siguientes acciones prioritarias:

1.  **🔧 FIX Infraestructura Admin:** Refactorizar `AdminWebAppFactory` o `Program.cs` en `GesFer.Admin` para permitir la ejecución paralela/secuencial de tests sin conflicto de Serilog. Considerar `Log.CloseAndFlush()` en el `Dispose` del fixture.
2.  **🛡️ Robustecer Test Auth:** Modificar `AuthControllerTests` para validar la contraseña verificando el hash (lógica funcional) en lugar de comparar el string del hash (implementación frágil).
3.  **🐛 Depurar Seeding:** Investigar por qué la "Company ...12" no se persiste. Ejecutar el seeder con logs detallados para ver si `TaxId` está rechazando el formato.
4.  **📈 Visualizar Cobertura:** Integrar herramienta de reporte (ej. ReportGenerator) para transformar los XML de Cobertura en HTML visible para el equipo.

---
*Fin del Informe*
