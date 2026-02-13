# Deuda Técnica: Inconsistencia en Inyección de Dependencias y Arquitectura Shared Kernel

**Fecha:** 2026-02-13
**Autor:** Jules (AI Assistant)
**Estado:** Pendiente
**Impacto:** Crítico (Fallo masivo de tests de integración)

## Descripción del Problema
Durante la resolución de un fallo de CI en la rama `feat-spec-article-family`, se detectó que el proyecto `GesFer.Shared.Back.Application` (Kernel Compartido) faltaba en el repositorio, lo que impedía la compilación.

Para solucionar el build, se restauró este proyecto introduciendo una arquitectura basada en **MediatR** y el patrón **Result**, definiendo interfaces estándar (`ICommand`, `IQuery`, `ICommandHandler`, `IUserContext`) en `src/Shared/Back`.

Sin embargo, esto ha revelado una inconsistencia arquitectónica grave:
1.  **Nuevos/Refactorizados Controladores (ej. `TaxTypesController`):** Utilizan `ISender` (MediatR) para despachar comandos. Esto desacopla el controlador del manejador y funciona correctamente con la nueva infraestructura.
2.  **Controladores Legacy (ej. `UserController`, `CityController`, etc.):** Inyectan directamente interfaces como `ICommandHandler<CreateUserCommand, UserDto>`.
    - La configuración de DI (`DependencyInjection.cs`) fue actualizada para usar `services.AddMediatR(...)`, lo cual registra los handlers como `IRequestHandler<T,R>`.
    - El contenedor de inyección de dependencias **no puede resolver** las interfaces antiguas `ICommandHandler` que los controladores legacy esperan, provocando `System.InvalidOperationException` en tiempo de ejecución y el fallo de **82 tests de integración**.

## Solución Propuesta
Refactorizar todos los controladores existentes para adoptar el patrón Mediator estandarizado:
1.  Eliminar la inyección directa de `ICommandHandler` y `IQueryHandler` en los constructores de los controladores.
2.  Inyectar `ISender` (de MediatR).
3.  Cambiar las llamadas de `_handler.Handle(command)` a `_sender.Send(command)`.
4.  Asegurar que todos los Commands y Queries implementen las interfaces base de `GesFer.Shared.Back` (`ICommand<T>`, `IQuery<T>`).
5.  Normalizar los tipos de retorno a `Result<T>` para manejo consistente de errores HTTP (400, 404, etc.).

## Estado Actual
- El proyecto **compila** correctamente.
- La funcionalidad `TaxTypes` (refactorizada como prueba de concepto) debería funcionar.
- La mayoría de los **tests de integración fallan** debido a la imposibilidad de instanciar los controladores legacy.
- Se ha priorizado la documentación de este estado sobre la refactorización masiva en esta tarea.

---

## Información para corrección futura (revertida en esta rama)

Esta sección recopila los cambios que se deshicieron en la rama, para poder reaplicar la corrección del Shared Kernel y tests en otro momento.

### 1. Archivos y rutas involucrados

**Proyecto Shared Kernel (añadido/restaurado):**
- `src/Shared/Back/GesFer.Shared.Back.Application.csproj` — Proyecto con MediatR y FluentValidation.
- `src/Shared/Back/GesFer.Shared.Back.Domain.csproj` — Excluir `Abstractions\**` del compile (las abstracciones pasan a Application).
- `src/Shared/Back/Abstractions/Authentication/IUserContext.cs`
- `src/Shared/Back/Abstractions/Messaging/Error.cs`
- `src/Shared/Back/Abstractions/Messaging/Result.cs`
- `src/Shared/Back/Abstractions/Messaging/ICommand.cs`, `ICommandHandler.cs`
- `src/Shared/Back/Abstractions/Messaging/IQuery.cs`, `IQueryHandler.cs`

**Product Back – referencias y servicios:**
- `src/Product/Back/application/GesFer.Application.csproj` — Añadir referencia a `GesFer.Shared.Back.Application`, paquetes MediatR y FluentValidation.
- `src/Product/Back/Infrastructure/GesFer.Infrastructure.csproj` — Añadir referencia a `GesFer.Shared.Back.Application` y `Microsoft.AspNetCore.Http.Abstractions` (2.2.0).
- `src/Product/Back/Infrastructure/Services/UserContext.cs` — Implementación de `IUserContext` (Claims: `NameIdentifier`, `companyId`, `Identity.Name`, `Email`, `IsAuthenticated`).

**Product Back – TaxTypes (ejemplo de patrón correcto):**
- `src/Product/Back/Api/Controllers/TaxTypesController.cs` — Usar `ISender` (MediatR), rutas con `[controller]`, `UpdateTaxTypeCommand(id, request)` con `Id` explícito.
- Commands: `CreateTaxTypeCommand` → `ICommand<Guid>`, `UpdateTaxTypeCommand(Guid Id, UpdateTaxTypeDto TaxType)` → `ICommand`, `DeleteTaxTypeCommand` → `ICommand`.
- Queries: `GetTaxTypesQuery()` → `IQuery<IReadOnlyList<TaxTypeDto>>`, `GetTaxTypeByIdQuery(Guid Id)` → `IQuery<TaxTypeDto>`.
- Handlers: implementar `ICommandHandler<TCommand, TResponse>` o `ICommandHandler<TCommand>` (Shared), devolver `Result` / `Result<T>`; un mismo handler puede implementar varios `IQueryHandler<,>` (ej. GetTaxTypes y GetTaxTypeById).

### 2. Contrato del Shared Kernel (resumen)

- **ICommand** / **ICommand&lt;TResponse&gt;** — Heredan de MediatR `IRequest<Result>` e `IRequest<Result<TResponse>>`.
- **ICommandHandler&lt;TCommand&gt;** / **ICommandHandler&lt;TCommand, TResponse&gt;** — Heredan de `IRequestHandler<TCommand, Result>` y `IRequestHandler<TCommand, Result<TResponse>>`; método `Handle(...)`.
- **IQuery&lt;TResponse&gt;** — `IRequest<Result<TResponse>>`.
- **IQueryHandler&lt;TQuery, TResponse&gt;** — `IRequestHandler<TQuery, Result<TResponse>>`.
- **Result** / **Result&lt;T&gt;** — `IsSuccess`, `Error` (tipo `Error` con `Code`, `Name`), factory `Success`/`Failure`/`Create`.
- **IUserContext** — `UserId`, `CompanyId`, `UserName`, `Email`, `IsAuthenticated` (Guid/string desde Claims).

Los comandos/queries **no** envuelven el tipo de retorno en `Result<>` en la interfaz: `ICommand<Guid>` implica que el handler devuelve `Result<Guid>`; `ICommand` sin genérico implica `Result`.

### 3. Incompatibilidad con el código legacy

- **Legacy:** `Application.Common.Interfaces.ICommandHandler<in TCommand, TResult>` con método `HandleAsync` y tipos `Application.Common.Interfaces.ICommand` / `ICommand<TResult>`.
- **Shared/MediatR:** Handlers implementan `IRequestHandler<,>` (vía Shared), método `Handle`; el DI actual registra por reflexión `Application.Common.Interfaces.ICommandHandler<>`. Si se cambia el registro a `AddMediatR`, los controladores que inyectan `ICommandHandler<CreateUserCommand, UserDto>` (del Common) dejan de resolverse.
- **Controladores que inyectan ICommandHandler (legacy):** UserController, CityController, CountryController, StateController, CompanyController, CustomerController, SupplierController, GroupController, PostalCodeController, más Auth, SalesDeliveryNote, PurchaseDeliveryNote, etc. (ver búsqueda `ICommandHandler` en `Api/Controllers` y `application/Handlers`).

### 4. Pasos recomendados para la corrección

1. Restaurar/añadir el proyecto `GesFer.Shared.Back.Application` y las abstracciones tal como se describen arriba.
2. En Product Back: añadir referencias y `UserContext`; registrar `IUserContext` y MediatR (assembly de Application).
3. Refactorizar **todos** los controladores que usan `ICommandHandler`/`IQueryHandler` legacy a inyectar solo `ISender` y usar `_sender.Send(command)`/`_sender.Send(query)`.
4. Migrar Commands/Queries a implementar interfaces de Shared (`ICommand`, `ICommand<T>`, `IQuery<T>`) y Handlers a implementar `ICommandHandler`/`IQueryHandler` de Shared, con retorno `Result`/`Result<T>`.
5. Eliminar o deprecar el registro por reflexión de `Application.Common.Interfaces.ICommandHandler` en favor de MediatR.
6. Ajustar tests de integración para que los controladores reciban `ISender` y, si hace falta, mocks de MediatR o handlers reales registrados.
7. Opcional: unificar `Application.Common.Interfaces.ICommand`/`ICommandHandler` con Shared en una sola evolución (o mantener alias/compatibilidad temporal hasta fin de migración).
