# AUDITORÍA BACKEND 2026-02-25

## 1. Métricas de Salud (0-100%)

*   **Arquitectura:** 90%
    *   *Análisis:* La estructura de la solución es sólida ("The Wall" intacto, compila correctamente). Se respeta el invariante de Shared (BaseEntity y ValueObjects centralizados). Se detecta deuda técnica en la organización de clases auxiliares en Console.
*   **Nomenclatura:** 80%
    *   *Análisis:* Inconsistencias detectadas en el contexto de base de datos de Product (`ApplicationDbContext` vs `AdminDbContext`) y en el namespace del proyecto Console (`GesFer.ConsoleApp` vs carpeta `src/Console`).
*   **Estabilidad Async:** 100%
    *   *Análisis:* No se encontraron patrones "Fire and Forget" (`async void`) en el código fuente. Las tareas en comandos de consola son esperadas correctamente (`await`).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Crítico: Ausencia de Tests Unitarios para Customer Handlers
*   **Hallazgo:** El módulo de `Customer` (dominio core) carece de pruebas unitarias para sus Handlers (Create, Update, Delete), a diferencia del módulo `User` que sí las tiene. Esto viola el principio de Testability.
*   **Ubicación:** `src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/` (Falta carpeta `Customer`)

### 🟡 Medio: Inconsistencia de Naming en DbContext (Product)
*   **Hallazgo:** El contexto de base de datos del dominio Product se llama `ApplicationDbContext`, mientras que el de Admin es `AdminDbContext`. Debería ser `ProductDbContext` para mantener la simetría y claridad.
*   **Ubicación:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`

### 🟡 Medio: Namespace Incorrecto en Proyecto Console
*   **Hallazgo:** El proyecto ubicado en `src/Console` utiliza el namespace `GesFer.ConsoleApp`, lo cual genera discrepancia con la estructura de directorios y el nombre del ensamblado (`GesFer.Console`).
*   **Ubicación:** Todo el directorio `src/Console` (ej. `Program.cs`, `Commands/*`)

### 🟡 Medio: Clase Anidada `DevelopmentHostEnvironment`
*   **Hallazgo:** La clase `InitializeDatabaseCommand` contiene una clase anidada `DevelopmentHostEnvironment` de ~15 líneas. Esto dificulta la testabilidad y viola el principio de responsabilidad única.
*   **Ubicación:** `src/Console/Commands/InitializeDatabaseCommand.cs` (Línea ~368)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Implementar Tests Unitarios para Customer Handlers
**Instrucciones:**
Crear la estructura de carpetas `Handlers/Customer` en el proyecto de test y replicar el patrón de pruebas de `User`.

```csharp
// src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/Customer/CreateCustomerCommandHandlerTests.cs
public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    // Setup y Tests para HandleAsync
}
```

**Definition of Done (DoD):**
*   Tests creados para Create, Update y Delete Customer.
*   Cobertura de código > 80% en estos handlers.
*   Ejecución exitosa de `dotnet test`.

---

### Acción 2: Renombrar ApplicationDbContext a ProductDbContext
**Instrucciones:**
1.  Renombrar el archivo `ApplicationDbContext.cs` a `ProductDbContext.cs`.
2.  Renombrar la clase `ApplicationDbContext` a `ProductDbContext`.
3.  Actualizar referencias en `InitializeDatabaseCommand.cs`, `Program.cs` (Api) y `DependencyInjection.cs`.
4.  **Importante:** Actualizar manualmente los archivos `.Designer.cs` de las migraciones existentes si tienen el atributo `[DbContext(typeof(ApplicationDbContext))]`.

```csharp
// src/Product/Back/Infrastructure/Data/ProductDbContext.cs
public class ProductDbContext : DbContext
{
    // ...
}
```

**Definition of Done (DoD):**
*   La solución compila sin errores.
*   `InitializeDatabaseCommand` utiliza `ProductDbContext`.

---

### Acción 3: Normalizar Namespace GesFer.Console
**Instrucciones:**
Realizar un Find & Replace masivo en `src/Console`:
*   Buscar: `namespace GesFer.ConsoleApp`
*   Reemplazar: `namespace GesFer.Console`

**Definition of Done (DoD):**
*   Todos los archivos en `src/Console` usan el namespace `GesFer.Console`.
*   El proyecto compila correctamente.

---

### Acción 4: Extraer DevelopmentHostEnvironment
**Instrucciones:**
Mover la clase anidada a un nuevo archivo en `src/Console/Services/DevelopmentHostEnvironment.cs`.

```csharp
// src/Console/Services/DevelopmentHostEnvironment.cs
namespace GesFer.Console.Services; // (Ojo con el cambio de namespace de la Acción 3)

public class DevelopmentHostEnvironment : IHostEnvironment
{
    public DevelopmentHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
    }
    // ... implementación
}
```

**Definition of Done (DoD):**
*   `InitializeDatabaseCommand.cs` ya no contiene la clase anidada.
*   `DevelopmentHostEnvironment` es una clase pública (o internal) en su propio archivo.
