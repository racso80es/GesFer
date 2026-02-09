# [SPEC-ID]: Refactor Console Menu to CommandHandlers

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-REF-004 |
| **Rama Relacionada** | refactor/console-commands-menu |
| **Estado** | Draft |
| **Responsable** | Spec Architect |
| **Token de Auditoría** | b9148395-53c1-4f78-8444-236127000003 |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Desacoplar la lógica de negocio del `MenuService` y delegar cada opción del menú (especialmente las nuevas Acciones Atómicas) en su propio `ICommandHandler`. Esto mejorará la mantenibilidad, testabilidad y el principio de responsabilidad única (SRP).

### 2.2. Alcance (Scope)
*   **Incluido:**
    *   Creación de nuevos Comandos:
        *   `InitializeDockerCommand` (extraer lógica de 3.1).
        *   `RestoreSeedsMenuCommand` (extraer lógica de 3.2).
        *   `StartServicesMenuCommand` (extraer lógica de 3.3).
    *   Refactorización de `MenuService.cs` para inyectar y ejecutar estos comandos en lugar de contener métodos privados con lógica de negocio.
    *   Actualización del contenedor de inyección de dependencias (`Program.cs`) para registrar los nuevos comandos.
*   **Fuera de Alcance:**
    *   Cambios en la funcionalidad subyacente de Docker o Seeds (solo reubicación de código).

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   `src/Console/Services/MenuService.cs`: Se convertirá en un orquestador ligero que solo muestra opciones y delega ejecución.
*   `src/Console/Commands/`: Nuevas clases implementando `ICommandHandler`.

### 3.2. Estrategia de Refactorización
1.  **InitializeDockerCommand:** Mover la secuencia `Remove -> Create -> WaitMySql` a un comando unificado.
2.  **StartServicesMenuCommand:** Mover la lógica de presentación del sub-menú de servicios y la llamada a `StartLocalEnvironmentCommand` a un comando dedicado.

## 4. Criterios de Aceptación

- [ ] `MenuService.cs` no contiene métodos privados con lógica de negocio compleja (como `ExecuteDockerInitializationAsync`).
- [ ] Cada opción del menú principal y sub-menús se corresponde con una llamada a `ICommandHandler.HandleAsync`.
- [ ] El comportamiento de la aplicación es idéntico al actual (funcionalidad preservada).
- [ ] El código compila y pasa los tests unitarios.

## 5. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-02-09
*   **Evento:** Generación de especificación futura tras implementación de Atomic Actions.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
