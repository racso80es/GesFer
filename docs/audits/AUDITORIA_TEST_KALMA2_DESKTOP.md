# Auditoría de Cobertura y Calidad de Tests - Kalma2/Interface/Desktop

**Fecha:** 2024-05-22
**Objetivo:** Analizar el estado actual de la cobertura de pruebas, identificar riesgos de seguridad y arquitectura, y proponer un plan de acción priorizado.

---

## 1. Informe del Agente de Seguridad

### Contexto
La aplicación `Kalma2/Interface/Desktop` es un cliente Electron que interactúa con el sistema operativo y procesos críticos. La superficie de ataque incluye la comunicación IPC (`window.calmaAPI`), la manipulación del DOM y la gestión de datos sensibles.

### Hallazgos Críticos
1.  **Ausencia Total de Pruebas Automatizadas:** No existe infraestructura de testing (Vitest/Jest) configurada en el proyecto. Esto significa que la cobertura de código es **0%**.
2.  **Riesgo en IPC (Inter-Process Communication):** La interfaz `calmaAPI` expuesta en `preload.ts` (métodos como `startSequence`, `updateSettings`, `runAudit`) no tiene validación automatizada de entradas ni pruebas de integración. Un cambio accidental podría romper la comunicación o exponer vulnerabilidades.
3.  **Falta de Validación de Regresión:** No hay mecanismo para detectar regresiones de seguridad en actualizaciones de dependencias (`electron`, `react`).

### Evaluación de Riesgo
**Nivel: CRÍTICO**
La falta de pruebas en una aplicación de escritorio con privilegios de sistema (a través de Electron) implica que cualquier refactorización o nueva característica podría introducir vulnerabilidades de seguridad graves sin ser detectadas.

---

## 2. Informe del Agente de Arquitectura

### Contexto
El proyecto sigue una arquitectura basada en React + Vite + Electron, con un núcleo de lógica de negocio (`src/core`) y servicios (`src/services`). Se espera que cumpla con los estándares definidos en `openspecs/skills/frontend-test.json`.

### Análisis de Brecha (Gap Analysis)
1.  **Desviación del Estándar:** El archivo `openspecs/skills/frontend-test.json` define comandos estándar (`test-desktop-unit`, `test-desktop-coverage`) que **no están implementados** en el `package.json` actual.
2.  **Deuda Técnica:** La lógica de negocio en `src/core/di` (inyección de dependencias) y `src/services` carece de arneses de prueba, lo que dificulta la refactorización y el mantenimiento a largo plazo.
3.  **Ecosistema Incompleto:** Faltan las dependencias de desarrollo esenciales: `vitest`, `@testing-library/react`, `jsdom`, `@vitest/coverage-v8`.

### Recomendaciones
1.  **Prioridad Inmediata:** Establecer la infraestructura de pruebas unitarias (`vitest`) para alinearse con los skills definidos.
2.  **Estrategia de Testing:**
    *   **Core Logic:** Unit tests puros para `src/core` y `src/services`.
    *   **Componentes:** Tests de integración (shallow) para componentes UI críticos usando `testing-library`.
    *   **E2E:** Implementar Playwright para flujos críticos (Login, Secuencias).

---

## 3. Acciones Ponderadas y Priorización

A continuación se presentan las acciones recomendadas, ponderadas por **Impacto (1-10)** (valor para el negocio/seguridad) y **Esfuerzo (1-10)** (costo de implementación, donde 1 es muy alto y 10 es muy bajo/fácil).

| Acción | Descripción | Impacto | Esfuerzo (Inv.) | Prioridad (I * E) |
| :--- | :--- | :---: | :---: | :---: |
| **1. Configurar Infraestructura de Testing** | Instalar Vitest, JSDOM, configurar scripts y mocks globales (`window.calmaAPI`). | 10 | 8 | **80** |
| **2. Crear Smoke Test (App.tsx)** | Test básico que verifique que la aplicación renderiza sin errores. | 9 | 9 | **81** |
| **3. Unit Tests para Core/DI** | Asegurar que el contenedor de inyección de dependencias funciona correctamente. | 8 | 7 | **56** |
| **4. Tests de Servicios IPC** | Mockear y probar los wrappers de IPC en el frontend. | 8 | 6 | **48** |
| **5. Cobertura de UI Components** | Añadir tests para componentes visuales (Botones, Inputs). | 6 | 5 | **30** |

### Plan de Ejecución Seleccionado
Dado que la acción #2 depende de la #1, y la #1 es el bloqueante principal para cualquier otra actividad de calidad:

**Acción Seleccionada:** **Configurar Infraestructura de Testing (Action #1)**
*   **Skill Requerido:** `frontend-test`
*   **Entregables:** `vitest.config.ts`, `setupTests.ts`, scripts en `package.json`.
