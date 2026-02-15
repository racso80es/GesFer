# [SPEC-ID]: {Nombre de la Tarea o Característica}

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-{Correlativo} |
| **Rama Relacionada** | {tipo}/{descripcion-breve} |
| **Estado** | Draft / Validado / Implementado |
| **Responsable** | Spec Architect |
| **Token de Auditoría** | AUDITOR-PROCESS-OK |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Describir de forma sucinta qué se quiere lograr y qué problema resuelve.

### 2.2. Alcance (Scope)
*   **Incluido:** Punto A, Punto B.
*   **Fuera de Alcance:** Elementos que no se tocarán en esta iteración para evitar el scope creep.

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
Identificar qué partes del sistema GesFer serán modificadas:
*   `src/Core`: Entidades de dominio o VOs.
*   `src/Infrastructure`: Repositorios o persistencia.
*   `src/Console`: Comandos y lógica de interfaz.

### 3.2. Modelo de Datos / Lógica
Si aplica, incluir definiciones de tipos o cambios en el esquema.
> **Nota de Arquitectura:** Se debe respetar el uso de `company` en lugar de `empresa` en todo el código nuevo.

## 4. Requisitos de Seguridad

*   **Validación de Input:** Uso de `SecurityScanner` para todas las entradas de usuario.
*   **Privacidad:** Identificar si se manejan datos sensibles (PII).
*   **Autorización:** Roles necesarios para ejecutar la funcionalidad.

## 5. Criterios de Aceptación

Para dar por cerrada esta especificación, se deben cumplir los siguientes puntos:

- [ ] El código compila sin errores (`dotnet build`).
- [ ] Se han pasado los tests unitarios correspondientes.
- [ ] El log de auditoría en `docs/audits/ACCESS_LOG.md` ha sido actualizado.
- [ ] El Audit Log de la rama refleja la evolución.

## 6. Structured Action Tags (Previstos)

Lista de etiquetas que se usarán en la fase de implementación:
```csharp
// TODO: [REF-VO] - ...
// TODO: [FIX-LOG] - ... (Activando AC-001 [LOGS])
```

## 7. Trazabilidad de Auditoría

*   **Fecha de Creación:** YYYY-MM-DD hh:mm
*   **Evento:** Generación inicial mediante `GesFer.Console --spec`.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
