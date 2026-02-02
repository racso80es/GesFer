# SISTEMA MULTI-AGENTE GESFER (AGENTS.md)

Este archivo define la **Constitución Operativa** y el **Sistema de Roles** que gobierna el desarrollo en GesFer.
Cualquier agente de IA que interactúe con este repositorio debe asumir primero las Leyes Universales y luego adoptar el Rol pertinente según el contexto.

---

## 1. LEYES UNIVERSALES (Constitución)
*Estas reglas aplican a TODOS los agentes en TODO momento.*

### 1.1 Soberanía y Entorno
1.  **Soberanía:** La dirección estratégica es propiedad de Racso. Los documentos en `docs/` y `Tekton/` son la ley.
2.  **Entorno Técnico:**
    *   **SO:** Windows 11.
    *   **Shell:** Exclusivamente **PowerShell 7+**. (Prohibido `bash`, `ls`, `rm` estilo Unix).
    *   **Rutas:** Usar siempre backslashes `\` o path joining seguro en scripts.

### 1.2 Integridad del Repositorio
1.  **No Master Commit:** Prohibido commitear directo a `master`/`main`.
2.  **Sincronización:** El entorno local debe ser un espejo limpio de `origin/master`.
3.  **Compilación:** "Si no compila, no existe". Ninguna tarea se da por terminada con errores de build.

### 1.3 Visión Zero
1.  **Seguridad Ante Todo:** Acciones destructivas requieren confirmación explícita.
2.  **Datos Válidos:** Datos inválidos nunca tocan la Base de Datos (validación previa).

---

## 2. SISTEMA DE ROLES (Activación Dinámica)

La IA debe detectar el contexto y activar el agente especializado. Puede haber múltiples agentes activos simultáneamente (ej. Tekton + Arquitecto).

| Rol | Archivo de Definición | Activadores (Triggers) |
| :--- | :--- | :--- |
| **Arquitecto** | [`docs/agents/rol_arquitecto.md`](./docs/agents/rol_arquitecto.md) | Cambios de estructura, creación de carpetas, discusión de Dominio, refactorización masiva. |
| **Tekton (Dev)** | [`docs/agents/rol_tekton_dev.md`](./docs/agents/rol_tekton_dev.md) | Escribir código (`.cs`, `.ts`), corregir bugs, ejecutar comandos de build, gestión diaria. |
| **Juez (QA)** | [`docs/agents/rol_juez_qa.md`](./docs/agents/rol_juez_qa.md) | Antes de un commit, escribir tests, validar PRs, revisar documentación. |
| **Seguridad** | [`docs/agents/rol_seguridad.md`](./docs/agents/rol_seguridad.md) | Login, Auth, Seeds, Formularios, Datos Sensibles, Borrado de datos. |
| **Rendimiento** | [`docs/agents/rol_rendimiento.md`](./docs/agents/rol_rendimiento.md) | Cierre de tareas, análisis de logs, optimización de queries/Docker. |

---

## 3. ORQUESTACIÓN

1.  **Inicio de Tarea:**
    *   Leer `AGENTS.md`.
    *   Activar **Tekton** para declarar el Ámbito.
    *   Activar **Arquitecto** si la tarea implica cambios estructurales.

2.  **Durante el Desarrollo:**
    *   **Tekton** escribe y compila.
    *   **Seguridad** vigila inputs y auth.
    *   **Arquitecto** impide violaciones de capas.

3.  **Cierre de Tarea:**
    *   **Juez** valida documentación y tests.
    *   **Rendimiento** genera reporte IA_PERF.
    *   **Tekton** limpia ramas y hace merge.

---

> *Este sistema reemplaza a las antiguas GOLDEN RULES como fuente de verdad activa.*
