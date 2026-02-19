# Reporte de Auditoría Backend

**Fecha:** 2026-02-19
**Auditor:** Guardián de la Infraestructura Backend

## 1. Métricas de Salud (0-100%)
*   **Arquitectura:** 90%
*   **Nomenclatura:** 85%
*   **Estabilidad Async:** 100%

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Hallazgo: Ambigüedad y Namespace Incorrecto en DbContext Principal
El contexto de base de datos del producto se llama `ApplicationDbContext` (nombre genérico) y reside en el namespace `GesFer.Infrastructure.Data`, lo cual viola la estructura de directorios (`src/Product/Back/Infrastructure/Data`) y crea asimetría con `AdminDbContext`.

**Ubicación:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`

### 🟡 Hallazgo: Inconsistencia en Casing de Carpetas de Dominio
El módulo `Shared` utiliza PascalCase (`Domain`), mientras que `Product` y `Admin` utilizan lowercase (`domain`). Esto genera inconsistencia visual y estructural en la solución.

**Ubicación:**
- `src/Product/Back/domain`
- `src/Admin/Back/domain`

### 🟡 Hallazgo: Acoplamiento Fuerte en Comandos de Consola
Los comandos `SeedCommand` y `CreateInitialMigrationCommand` tienen rutas hardcodeadas hacia `src/Product/Back/Api` para cargar configuraciones. Esto acopla la consola específicamente a la estructura de directorios del Producto, dificultando su reutilización o movimiento.

**Ubicación:**
- `src/Console/Commands/SeedCommand.cs`
- `src/Console/Commands/CreateInitialMigrationCommand.cs`

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Refactorización de ProductDbContext
**Objetivo:** Alinear el contexto de base de datos con el estándar de `AdminDbContext` y su ubicación física.

**Instrucciones:**
1.  Renombrar el archivo `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` a `ProductDbContext.cs`.
2.  Renombrar la clase `ApplicationDbContext` a `ProductDbContext`.
3.  Cambiar el namespace de `GesFer.Infrastructure.Data` a `GesFer.Product.Back.Infrastructure.Data`.
4.  Actualizar todas las referencias en la solución (especialmente en `Program.cs` de la API de Producto y en `SeedCommand.cs`).

**Definition of Done (DoD):**
- La solución compila correctamente (`dotnet build`).
- No existen referencias a `ApplicationDbContext`.
- El namespace coincide con la ruta de carpetas.

### Acción 2: Normalización de Estructura de Directorios
**Objetivo:** Estandarizar el naming de carpetas críticas.

**Instrucciones:**
1.  Renombrar la carpeta `src/Product/Back/domain` a `src/Product/Back/Domain`.
2.  Renombrar la carpeta `src/Admin/Back/domain` a `src/Admin/Back/Domain`.
3.  Verificar que los namespaces en los archivos contenidos coincidan (aunque C# no fuerza carpeta=namespace, es buena práctica).

**Definition of Done (DoD):**
- Todas las carpetas de Dominio usan PascalCase.
- Git registra el cambio de nombre (usar `git mv` si es necesario en entorno local, o asegurar que el sistema de archivos lo refleje).

### Acción 3: Abstracción de Rutas en Consola
**Objetivo:** Eliminar hardcoded strings que apuntan a rutas de otros proyectos.

**Instrucciones:**
1.  Modificar `SeedCommand` y `CreateInitialMigrationCommand` para que la ruta base de configuración sea inyectada o determinada dinámicamente, o mover la lógica de resolución de rutas a un servicio de infraestructura compartido (`IPathService`).
2.  Alternativamente, aceptar la ruta del archivo de configuración como argumento del comando.

**Definition of Done (DoD):**
- No existen strings literales como `"src/Product/Back/Api"` dentro de la lógica de los comandos.
