# Auditoría Backend: Reporte de Estado

**Fecha:** 2026-02-24
**Auditor:** Jules (Guardián de la Infraestructura Backend)
**Protocolo:** The Wall + Deep Scan + Persistence & Contracts

---

## 1. Métricas de Salud

| Métrica | Puntuación | Estado |
| :--- | :---: | :--- |
| **Arquitectura** | **100%** | 🟢 Óptimo. Los tests de arquitectura (`GesFer.Architecture.Tests`) pasan correctamente. No se detectó duplicidad de `BaseEntity` ni `ValueObject` en los dominios específicos. |
| **Nomenclatura** | **85%** | 🟡 Mejorable. Se detectaron inconsistencias en el casing de carpetas (`domain` vs `Domain`) y namespaces (`GesFer.ConsoleApp` vs `GesFer.Console`). |
| **Estabilidad Async** | **100%** | 🟢 Impecable. No se encontraron patrones `async void` (Fire and Forget) ni llamadas bloqueantes `.Result` / `.Wait()` en el código fuente. |

---

## 2. Pain Points (Hallazgos)

### 🟡 Medio: Inconsistencia en Casing de Directorios de Dominio
**Hallazgo:** Las carpetas de dominio en Product y Admin están en minúsculas (`domain`), rompiendo la convención PascalCase utilizada en Shared (`Domain`) y en el resto de la solución.
**Ubicación:**
- `src/Product/Back/domain`
- `src/Admin/Back/domain`

### 🟡 Medio: Inconsistencia de Namespace en Proyecto Console
**Hallazgo:** El proyecto ubicado en `src/Console` utiliza el namespace raíz `GesFer.ConsoleApp`, lo que genera una discrepancia entre la estructura física y lógica.
**Ubicación:**
- `src/Console/Program.cs`
- `src/Console/GesFer.Console.csproj` (<RootNamespace>)

### 🟡 Medio: Namespace Genérico en ApplicationDbContext
**Hallazgo:** El `ApplicationDbContext` del dominio Product utiliza el namespace `GesFer.Infrastructure.Data`, el cual es demasiado genérico y no especifica que pertenece al contexto de Product (`GesFer.Product.Infrastructure.Data`).
**Ubicación:**
- `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` (Línea 5)

---

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Estandarización de Directorios de Dominio
**Objetivo:** Unificar el casing de las carpetas de dominio a PascalCase (`Domain`).

**Instrucciones para el Executor:**
1. Utilizar `git mv` para renombrar las carpetas (crítico en sistemas case-insensitive como Windows/macOS).
```bash
git mv src/Product/Back/domain src/Product/Back/Domain_Temp
git mv src/Product/Back/Domain_Temp src/Product/Back/Domain
git mv src/Admin/Back/domain src/Admin/Back/Domain_Temp
git mv src/Admin/Back/Domain_Temp src/Admin/Back/Domain
```
2. Verificar que los `csproj` no tengan referencias explícitas a las rutas antiguas (limpiar si es necesario).
3. Compilar la solución para asegurar que no hay referencias rotas.

**Definition of Done (DoD):**
- Las carpetas existen como `src/Product/Back/Domain` y `src/Admin/Back/Domain`.
- `dotnet build` finaliza con éxito (0 errores).

---

### Acción 2: Corrección de Namespace en Console
**Objetivo:** Alinear el namespace del proyecto Console con su ubicación física (`GesFer.Console`).

**Instrucciones para el Executor:**
1. Modificar `src/Console/GesFer.Console.csproj`:
   - Cambiar `<RootNamespace>GesFer.ConsoleApp</RootNamespace>` a `<RootNamespace>GesFer.Console</RootNamespace>`.
2. Realizar un Find & Replace global en `src/Console`:
   - Buscar: `namespace GesFer.ConsoleApp`
   - Reemplazar: `namespace GesFer.Console`
3. Corregir `Program.cs` y los comandos.

**Definition of Done (DoD):**
- `grep -r "namespace GesFer.ConsoleApp" src/Console` no devuelve resultados.
- La aplicación de consola compila y ejecuta correctamente.

---

### Acción 3: Refinamiento de Namespace en Product Context
**Objetivo:** Especificar el namespace de `ApplicationDbContext` para evitar ambigüedades.

**Instrucciones para el Executor:**
1. En `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`:
   - Cambiar `namespace GesFer.Infrastructure.Data;` por `namespace GesFer.Product.Infrastructure.Data;`.
2. Actualizar `DbInitializer.cs` y `Program.cs` (Api) para importar el nuevo namespace.
3. Actualizar `InitializeDatabaseCommand.cs` si lo referencia.

**Definition of Done (DoD):**
- El namespace es explícito: `GesFer.Product.Infrastructure.Data`.
- Todos los proyectos que consumen el contexto compilan correctamente.
