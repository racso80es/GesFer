# Objetivo de la Rama

Refactorizar la arquitectura del módulo `GesFer.Admin` para cumplir con los principios de Clean Architecture y optimizar la seguridad en la configuración de la base de datos.

## Descripción

La auditoría de backend detectó una violación crítica de arquitectura ("The Wall Violation") donde la capa de Aplicación (`GesFer.Admin.Application`) dependía directamente de la Infraestructura (`GesFer.Admin.Infra`), creando un acoplamiento circular e incorrecto. Además, se identificó un riesgo de seguridad en `SeedCommand.cs` debido al uso de credenciales de base de datos hardcodeadas como fallback.

Esta rama introduce `IAdminDbContext` para desacoplar las capas y asegura que la configuración de conexión sea obligatoria.

## Acciones Realizadas

1.  **Refactorización de Arquitectura Admin**:
    *   Se creó la interfaz `IAdminDbContext` en `GesFer.Admin.Application.Common.Interfaces` exponiendo los `DbSet` necesarios.
    *   Se eliminó la referencia de proyecto de `Application` hacia `Infra`.
    *   Se invirtió la dependencia: ahora `Infra` referencia a `Application` e implementa `IAdminDbContext`.
    *   Se actualizó la inyección de dependencias en `GesFer.Admin.Api` para registrar la interfaz.

2.  **Actualización de Handlers**:
    *   Se refactorizaron todos los Handlers de Company (`Create`, `Update`, `Delete`, `Get*`) para inyectar `IAdminDbContext` en lugar del contexto concreto.

3.  **Optimización de Seguridad en Console**:
    *   Se eliminó el string de conexión hardcodeado en `SeedCommand.cs`. Ahora lanza una excepción si la configuración no está presente en `appsettings.json` o variables de entorno.

4.  **Documentación**:
    *   Se actualizaron `KAIZEN_BACKLOG.md` y `EVOLUTION_LOG.md` con el progreso.
