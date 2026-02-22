# AUDITORÍA DE BACKEND (2026-02-22)

## 1. Métricas de Salud (0-100%)

*   **Arquitectura: 90%** (Buena separación de dominios y uso correcto de Shared, pero inconsistencias en estructura de directorios).
*   **Nomenclatura: 70%** (Inconsistencias graves en namespaces y capitalización de carpetas de dominio).
*   **Estabilidad Async: 100%** (No se encontraron patrones "fire and forget" ni métodos `async void` en código de producción).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

*   🔴 **Hallazgo:** Inconsistencia de Capitalización en Directorios de Dominio.
    *   **Descripción:** Los directorios de dominio en Product y Admin están en minúsculas (`domain`), rompiendo la convención PascalCase de .NET y del resto del proyecto (`Shared/Back/Domain`).
    *   **Ubicación:**
        - `src/Product/Back/domain`
        - `src/Admin/Back/domain`

*   🟡 **Hallazgo:** Namespaces Incompletos/Incorrectos.
    *   **Descripción:** Los namespaces no reflejan la estructura jerárquica de carpetas, lo que dificulta la navegación y el autodiscovery.
    *   **Ubicación:**
        - `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs` (Namespace actual: `GesFer.Infrastructure.Services`, esperado: `GesFer.Product.Back.Infrastructure.Services`).
        - `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs` (Namespace actual: `GesFer.Admin.Infrastructure.Data`, esperado: `GesFer.Admin.Back.Infrastructure.Data`).

*   🟡 **Hallazgo:** Rutas Físicas Hardcodeadas (Fragilidad).
    *   **Descripción:** Dependencia de la estructura de carpetas física en tiempo de ejecución para localizar archivos de configuración.
    *   **Ubicación:** `src/Console/Commands/InitializeDatabaseCommand.cs` (Línea ~53: `Path.Combine(rootPath, "src", "Product", "Back", "Api")`).

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Normalización de Directorios (Prioridad Alta)
**Objetivo:** Estandarizar la estructura de carpetas para cumplir convenciones .NET.

**Instrucciones:**
1. Renombrar la carpeta `src/Product/Back/domain` a `src/Product/Back/Domain`.
2. Renombrar la carpeta `src/Admin/Back/domain` a `src/Admin/Back/Domain`.
3. Verificar que los namespaces en los archivos contenidos (`Entities`, `Services`) sean consistentes (ya parecen ser `GesFer.Product.Back.Domain...`, por lo que solo el rename de carpeta es necesario).

**Definition of Done (DoD):**
- Las carpetas `Domain` en Product y Admin están en PascalCase.
- El proyecto compila correctamente tras el cambio (Git suele manejar esto bien en sistemas case-insensitive, pero requiere cuidado en Linux/CI).

### Acción 2: Corrección de Namespaces (Prioridad Media)
**Objetivo:** Alinear namespaces con la estructura física y lógica.

**Instrucciones:**
1. En `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`:
   ```csharp
   // Cambiar namespace
   namespace GesFer.Product.Back.Infrastructure.Services;
   ```
2. En `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs`:
   ```csharp
   // Cambiar namespace
   namespace GesFer.Admin.Back.Infrastructure.Data;
   ```
3. Actualizar `usings` en todos los archivos que consuman estas clases (ej. `InitializeDatabaseCommand.cs`, `Program.cs` de la API, `IntegrationTestWebAppFactory`, etc.).

**Definition of Done (DoD):**
- `JsonDataSeeder` y `AdminDbContext` tienen namespaces completos (`Product.Back` / `Admin.Back`).
- Todos los consumidores han actualizado sus `using`.
- La solución compila sin errores.

### Acción 3: Abstracción de Rutas (Prioridad Media)
**Objetivo:** Eliminar dependencia de rutas físicas absolutas/relativas hardcodeadas.

**Instrucciones:**
1. Modificar `InitializeDatabaseCommand` para aceptar la ruta de la API como parámetro opcional o configuración, en lugar de inferirla estrictamente.
2. O bien, utilizar `IHostEnvironment` para detectar la raíz de contenido de forma más robusta si es posible, o centralizar la lógica de "búsqueda de raíz" en un servicio de infraestructura compartido (`IPathService`).

**Definition of Done (DoD):**
- El comando `InitializeDatabaseCommand` no falla si se ejecuta desde una ubicación diferente o si la estructura de carpetas cambia ligeramente (siempre que la configuración sea inyectable).
