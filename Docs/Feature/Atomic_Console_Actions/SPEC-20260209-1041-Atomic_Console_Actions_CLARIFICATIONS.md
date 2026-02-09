# [CLARIFICATION]: Atomic Console Actions (Action 3)

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Clarificación** | SPEC-GF-2026-003-CLARIFICATIONS |
| **Especificación Base** | SPEC-GF-2026-003 (Atomic Console Actions) |
| **Fecha** | 2026-02-09 |
| **Estado** | Approved with Warnings |
| **Responsable** | Clarification Specialist |

## 2. Puntos Clarificados (Gaps Resolved)

### 2.1. Eliminación de Acción 8
**Pregunta:** ¿Debe eliminarse la Acción 8 (Seeds) del menú principal?
**Respuesta:** Sí. La funcionalidad de restaurar semillas se integrará exclusivamente en la nueva Acción 3.2.

### 2.2. Reubicación de Acción 3 (Legacy)
**Pregunta:** ¿Dónde se reubica la actual Acción 3 (Inicialización de BD)?
**Respuesta:** Se moverá a la nueva Acción 3.4 ("Inicialización Completa BD") dentro del submenú de acciones atómicas.

### 2.3. Persistencia de Acción 2
**Pregunta:** ¿Se mantiene la Acción 2 (Levantar entorno local) en el menú principal?
**Respuesta:** Sí, se mantiene como un atajo ("shortcut") para levantar todo, mientras que la nueva Acción 3.3 ofrecerá control granular.

### 2.4. Granularidad en Acción 3.3
**Pregunta:** ¿Qué nivel de granularidad se requiere para levantar servicios?
**Respuesta:** Debe permitir seleccionar servicios individuales (Product API, Admin API, Product Front, Admin Front) o una opción de "Iniciar Todos".

### 2.5. Ruta de Documentación
**Pregunta:** ¿Dónde deben almacenarse los artefactos de documentación de especificación?
**Respuesta:** Deben persistirse en `./Docs/Feature/{feature indicada}/`, desviándose de la ruta por defecto `openspecs/specs/`.

## 3. Advertencias y Riesgos (Warnings)

### 3.1. Generación Manual
**Warning:** Este documento de clarificación ha sido generado manualmente debido a limitaciones en la interactividad de la herramienta CLI `GesFer.Console --clarify` en el entorno actual. Se debe validar que el contenido refleje fielmente los requisitos acordados.

### 3.2. Dependencia de Rutas
**Warning:** El cambio de ruta de documentación (`Docs/Feature/...`) implica que futuros procesos automatizados (como `plan` o `impl`) deben ser informados explícitamente de la nueva ubicación de los archivos `.md`.

## 4. Trazabilidad de Auditoría

*   **Evento:** Clarificación manual tras confirmación de requisitos.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md` (Entrada manual requerida si no se ejecuta vía CLI).
