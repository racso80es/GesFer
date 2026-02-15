# Auditoría de Cobertura de Tests - Kalma2/Desktop

**Fecha:** 15 de Febrero de 2026
**Proyecto:** Kalma2/Interfaces/Desktop
**Autor:** Jules (Agente Principal)

## Resumen Ejecutivo
El análisis de cobertura realizado sobre el proyecto `Kalma2/Interfaces/Desktop` revela una cobertura superficial (~30% en `src/`) que enmascara una ausencia total de tests en las capas críticas de seguridad y lógica de negocio. Los directorios `electron/` (Proceso Principal) y `Kalma2/Core` (Lógica Compartida) no están siendo auditados por la suite de tests actual.

## Análisis de Seguridad (Agente Seguridad)
Se han identificado los siguientes riesgos críticos debido a la falta de cobertura:

1.  **Proceso Principal (Electron Main) Sin Auditar:**
    -   El directorio `electron/` contiene la lógica de privilegios elevados, manejo de IPC y acceso al sistema de archivos.
    -   La configuración actual de Vitest excluye explícitamente este directorio.
    -   **Riesgo:** Vulnerabilidades en IPC handlers o fugas de datos en el manejo de secretos (IOTA Wallet) podrían pasar desapercibidas.

2.  **Lógica Core (Kalma2/Core) Expuesta:**
    -   El `tsconfig.json` incluye `../../Core` como parte del código fuente, pero no existen tests unitarios que validen esta lógica (Auditoría, Criptografía).
    -   **Riesgo:** Fallos en la lógica de auditoría o inmutabilidad de datos comprometen la integridad del sistema "The Wall".

3.  **Configuración de Entorno Inadecuada:**
    -   Los tests actuales corren en `jsdom` (navegador simulado), lo cual es incompatible con las dependencias de Node.js (`crypto`, `fs`, `@iota/sdk`) que utiliza el Core y Electron Main.

## Análisis de Arquitectura (Agente Arquitectura)
La estructura actual de tests es insuficiente para una arquitectura hexagonal/modular:

1.  **Acoplamiento de Código Fuente:**
    -   `Kalma2/Interfaces/Desktop` consume directamente el código fuente de `Kalma2/Core`. Esto requiere que los tests de Desktop asuman la responsabilidad de probar Core.

2.  **Falta de Estrategia de Testing Dual:**
    -   Se intenta probar todo con una sola configuración (`jsdom`).
    -   **Recomendación:** Separar la ejecución de tests en dos suites:
        -   **Unit Frontend:** `vitest.config.ts` (jsdom) para Componentes React (`src/`).
        -   **Unit Backend/Core:** `vitest.config.node.ts` (node) para Lógica de Negocio y Electron Main (`electron/`, `../../Core`).

3.  **Ruido en Métricas:**
    -   Archivos de configuración (`vite.config.ts`, `postcss.config.js`) se incluyen en el reporte de cobertura, diluyendo las métricas reales.

## Matriz de Acciones Priorizadas

| Prioridad | Acción | Impacto | Esfuerzo | Justificación |
| :--- | :--- | :--- | :--- | :--- |
| **1 (Alta)** | **Configurar Suite de Tests Node** | Alto | Bajo | Habilita el testing de `Core` y `Electron` que actualmente es imposible en `jsdom`. |
| **2 (Alta)** | **Tests Unitarios para Kalma2/Core** | Alto | Medio | Asegura la lógica de negocio crítica (Auditoría, Entidades). Requiere acción 1. |
| **3 (Media)** | **Tests Unitarios para Electron IPC** | Medio | Alto | Asegura la capa de comunicación. Requiere mocks complejos de Electron. |
| **4 (Baja)** | **Limpieza de Configuración** | Bajo | Bajo | Excluir archivos de config del reporte para métricas reales. |
| **5 (Baja)** | **Tests de Componentes React** | Bajo | Bajo | `App.tsx` ya tiene cobertura básica. No es crítico para seguridad. |

## Próximos Pasos (Plan de Acción)
Se procederá a ejecutar la **Acción 1 y 2** combinadas:
1.  Crear `vitest.config.node.ts` para ejecutar tests en entorno Node.
2.  Crear un test de prueba para un componente de `Kalma2/Core` (e.g., `AuditorService` o similar) para validar el flujo.
