# [SPEC-GF-2026-001]: Refactorización y Aislamiento de Kalma2

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-001 |
| **Rama Relacionada** | feature/rf_isolando_kalma2 |
| **Estado** | Draft |
| **Responsable** | Tekton Developer |
| **Token de Auditoría** | AUDITOR-PROCESS-OK |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Aislar el módulo `Kalma2` (Desktop/Core) moviéndolo desde `src/Kalma2` a la raíz del repositorio (`./Kalma2`). Esto tiene como fin desacoplar el desarrollo de la aplicación de escritorio del resto de la arquitectura monolítica de `src/`, facilitando su evolución independiente.

### 2.2. Alcance (Scope)
*   **Incluido:**
    *   Corrección de rutas en scripts de lanzamiento (`ejecutar-electron.bat`).
    *   Corrección de referencias internas (e.g., `kaizen-check.js`).
    *   Estandarización de nombres de directorios (`core` -> `Core`).
    *   Verificación de compilación y ejecución básica.
*   **Fuera de Alcance:**
    *   Cambios en la lógica de negocio de Kalma2.
    *   Refactorización profunda de `GesFer` (Backend/Frontend Web).

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   `./Kalma2`: Nueva ubicación raíz.
*   `src/Kalma2`: Eliminado (movido).
*   `ejecutar-electron.bat`: Actualizado para apuntar a la nueva ruta.
*   `Kalma2/Interfaces/Desktop/scripts/kaizen-check.js`: Actualizado para validar la nueva ubicación de la Constitución.

### 3.2. Modelo de Datos / Lógica
No aplica cambios en el modelo de datos.
> **Nota de Arquitectura:** Se ha renombrado `Kalma2/core` a `Kalma2/Core` para mantener la consistencia (PascalCase) con el resto del proyecto y evitar problemas de case-sensitivity en CI/Linux.

## 4. Requisitos de Seguridad

*   **Validación de Input:** No aplica cambios.
*   **Privacidad:** No se tocan datos sensibles.
*   **Autorización:** No cambia.

## 5. Criterios de Aceptación

Para dar por cerrada esta especificación, se deben cumplir los siguientes puntos:

- [ ] El script `ejecutar-electron.bat` apunta a la ruta correcta (`Kalma2\Interfaces\Desktop`).
- [ ] El script de validación `kaizen-check.js` se ejecuta exitosamente encontrando `CONSTITUTION.md`.
- [ ] La estructura de carpetas en `Kalma2` sigue la convención PascalCase (`Core`, `Docs`, `Interfaces`).
- [ ] El código compila y no hay referencias rotas a `src/Kalma2`.

## 6. Structured Action Tags (Previstos)

```csharp
// TODO: [REF-DIR] - Moved Kalma2 to root.
```

## 7. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-02-09
*   **Evento:** Generación manual tras refactorización.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
