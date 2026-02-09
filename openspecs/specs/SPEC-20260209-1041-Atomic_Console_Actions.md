# [SPEC-ID]: Atomic Console Actions (Action 3)

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-003 |
| **Rama Relacionada** | feature/atomic-console-actions |
| **Estado** | Draft |
| **Responsable** | Spec Architect |
| **Token de Auditoría** | b9148395-53c1-4f78-8444-236127000003 |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Refactorizar el menú de `GesFer.Console` para incluir una nueva "Acción 3: Acciones Atómicas" que permita la ejecución granular de tareas de inicialización y puesta en marcha del entorno. Esto incluye la gestión de Docker, restauración de semillas (Seeds) y arranque de servicios (Backend/Frontend) de forma independiente o conjunta. Además, se eliminarán las acciones redundantes (Action 8) y se reubicará la inicialización de BD (antigua Action 3) dentro de este nuevo menú.

### 2.2. Alcance (Scope)
*   **Incluido:**
    *   Creación de submenú para Acción 3.
    *   **3.1 Inicializar Docker:** Detener contenedores, recrearlos y esperar a MySQL.
    *   **3.2 Restaurar Datos Seed:** Selección de Scope (Admin/Product/All) y Level. (Reemplaza Action 8).
    *   **3.3 Levantar Servicios:** Selección granular (Product API, Admin API, Product Front, Admin Front) o "Iniciar Todos".
    *   **3.4 Inicialización Completa BD:** Migraciones + Seeds (Reemplaza antigua Action 3).
    *   Eliminación de Action 8 del menú principal.
    *   Actualización de referencias en `MenuService.cs` y comandos relacionados.
*   **Fuera de Alcance:**
    *   Modificación de la lógica interna de los seeders o migraciones (solo su invocación).
    *   Cambios en la infraestructura de Docker (docker-compose files).

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   `src/Console/Services/MenuService.cs`: Reestructuración del menú principal y lógica de manejo de opciones.
*   `src/Console/Commands/StartLocalEnvironmentCommand.cs`: Refactorización para aceptar parámetros de entrada que definan qué servicios iniciar (granularidad).
*   `src/Console/Commands/InitializeDatabaseCommand.cs`: Reubicación lógica.
*   `src/Console/Program.cs`: Ajuste de argumentos CLI si es necesario.

### 3.2. Modelo de Datos / Lógica
*   Nuevo DTO `StartLocalEnvironmentInput` con propiedades booleanas: `StartProductApi`, `StartAdminApi`, `StartProductFront`, `StartAdminFront`.

## 4. Requisitos de Seguridad

*   **Validación de Input:** Uso de `SecurityScanner` para validar entradas en los menús interactivos.
*   **Privacidad:** No aplica (herramienta de desarrollo).
*   **Autorización:** Requiere ejecución en entorno de desarrollo (verificado por `Program.cs`).

## 5. Criterios de Aceptación

- [ ] La Acción 3 muestra el submenú correcto con las 4 opciones.
- [ ] La opción 3.1 reinicia Docker correctamente y espera a la BD.
- [ ] La opción 3.2 permite seleccionar Scope y Level y ejecuta los seeds correspondientes.
- [ ] La opción 3.3 permite elegir qué servicios levantar (uno, varios o todos) y libera los puertos correspondientes antes de iniciar.
- [ ] La opción 3.4 ejecuta migraciones y seeds completos correctamente.
- [ ] La Acción 8 ya no aparece en el menú principal.
- [ ] La antigua Acción 3 ya no está en el menú principal (movida a 3.4).
- [ ] El código compila sin errores.

## 6. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-02-09
*   **Evento:** Generación manual (corrección de plantilla) tras `GesFer.Console --spec`.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
