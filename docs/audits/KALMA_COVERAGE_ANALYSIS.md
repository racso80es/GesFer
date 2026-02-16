# Auditoría de Cobertura: Kalma2/Desktop

**Fecha:** 2025-01-26
**Objetivo:** Analizar la cobertura de tests actual y proponer acciones de mejora.
**Agentes:** Security Engineer, System Architect.

## 1. Estado Actual

| Métrica | Valor | Notas |
| :--- | :--- | :--- |
| **Cobertura Global** | **~80% (App.tsx)** | Infraestructura Vitest + JSDOM habilitada. |
| **Infraestructura** | **Operativa** | `vitest` instalado y configurado. Scripts `test` y `coverage` activos. |
| **Tests Encontrados** | 1 | `src/App.test.tsx` (Smoke Test). |

## 2. Análisis por Agente

### 🛡️ Security Engineer Report

> "La infraestructura base permite ahora asegurar la UI, pero el riesgo principal (IPC/Wallet) sigue latente."

**Hallazgos Críticos (Actualizado):**
1.  **Untested IPC Handlers:** La lógica en `electron/main.ts` sigue sin tests.
2.  **UI Verification:** `App.tsx` ahora tiene tests básicos, lo que valida que la aplicación monta y muestra información, reduciendo el riesgo de "pantalla blanca" en producción.

**Recomendaciones de Seguridad:**
*   **URGENTE:** Crear tests unitarios para `electron/main.ts` o la capa de servicio que maneja las claves.

### 🏛️ System Architect Report

> "Se ha establecido el patrón de testing para la UI. El siguiente paso es aislar el entorno Node."

**Hallazgos Estructurales (Actualizado):**
1.  **DI Mocking Verified:** Se confirmó que es posible mockear el contenedor DI (`core/di/container`) usando `vi.spyOn`, lo que permite testear componentes desacoplados de la implementación real.
2.  **Global Mocks:** `src/setupTests.ts` está configurado correctamente con `vi.fn()` para `window.calmaAPI`.

## 3. Propuesta de Acciones (Backlog)

| ID | Acción | Peso (Impacto/Esfuerzo) | Prioridad | Estado |
| :--- | :--- | :--- | :--- | :--- |
| **ACT-001** | **Bootstrap Test Infrastructure** | **10/10** | **CRÍTICA** | **✅ COMPLETADO** |
| **ACT-002** | **Unit Test: `App.tsx` (Smoke Test UI)** | 8/10 | ALTA | **✅ COMPLETADO** |
| ACT-003 | Unit Test: `electron/main.ts` (IPC & Security) | 9/10 (Seguridad Crítica) | ALTA | *Siguiente* |
| ACT-004 | E2E Tests: Playwright Setup | 7/10 (Esfuerzo Alto) | MEDIA | *Pendiente* |

## 4. Próximos Pasos

La acción **ACT-001** y **ACT-002** han sido completadas exitosamente.
La infraestructura permite correr `npm run coverage` y obtener reportes v8.

Se recomienda proceder con **ACT-003** en el siguiente ciclo para cubrir la lógica de backend (Electron Main).
