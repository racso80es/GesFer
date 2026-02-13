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
