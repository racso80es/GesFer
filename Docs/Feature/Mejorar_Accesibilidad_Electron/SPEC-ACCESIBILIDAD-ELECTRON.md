# [SPEC-ACCESIBILIDAD-ELECTRON-001]: Mejorar accesibilidad a proyecto Electron como interface

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-ACCESIBILIDAD-ELECTRON-001 |
| **Rama Relacionada** | feature/mejorar-accesibilidad-electron |
| **Estado** | Draft |
| **Responsable** | Spec Architect |
| **Token de Auditoría** | AUDITOR-PROCESS-OK |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Mejorar la accesibilidad para ejecutar el proyecto frontend en Electron, proporcionando un script batch estándar (`ejecutar-interface.bat`) que facilite su lanzamiento, similar a `ejecutar-consola.bat`.

### 2.2. Alcance (Scope)
*   **Incluido:** Creación de `ejecutar-interface.bat`, eliminación de scripts obsoletos o rotos (`ejecutar-desktop.bat`, `ejecutar-interfaz.bat`).
*   **Fuera de Alcance:** Modificaciones en el código fuente de Electron o Consola.

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   Root Directory: Scripts batch de ejecución.
*   `Kalma2/Interfaces/Desktop`: Directorio objetivo del script.

### 3.2. Modelo de Datos / Lógica
Se utilizará la lógica existente en `ejecutar-electron.bat` como base, asegurando validaciones de seguridad (existencia de directorios, herramientas npm/node).

## 4. Requisitos de Seguridad

*   **Validación de Input:** N/A (Script batch sin input de usuario complejo).
*   **Privacidad:** No maneja datos sensibles.
*   **Autorización:** Ejecución local.
*   **Integridad:** El script debe verificar la existencia de `package.json` antes de ejecutar.

## 5. Criterios de Aceptación

Para dar por cerrada esta especificación, se deben cumplir los siguientes puntos:

- [ ] El script `ejecutar-interface.bat` existe en la raíz.
- [ ] El script lanza correctamente la aplicación Electron desde `Kalma2/Interfaces/Desktop`.
- [ ] Los scripts obsoletos (`ejecutar-desktop.bat`, `ejecutar-interfaz.bat`) han sido eliminados.
- [ ] El log de auditoría en `docs/audits/ACCESS_LOG.md` ha sido actualizado.

## 6. Structured Action Tags (Previstos)

```csharp
// N/A - Cambios en scripts batch solamente.
```

## 7. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-02-10
*   **Evento:** Generación manual siguiendo template.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
