# Objetivo de la Rama: kaizen-kalma2-desktop-tests

## 1. Propósito
Establecer la infraestructura de testing (Unitaria y E2E) para el proyecto `Kalma2/Interfaces/Desktop`, abordando la deuda técnica crítica de cobertura 0% identificada en la auditoría de seguridad.

## 2. Contexto
El frontend de Desktop (Electron + React) carecía de herramientas de prueba automatizadas, exponiendo el sistema a regresiones silenciosas, especialmente en la integración con `Kalma2/Core` y el puente IPC `window.calmaAPI`.

## 3. Cambios Realizados
1.  **Infraestructura de Tests Unitarios:**
    *   Instalación de Vitest, JSDOM y React Testing Library.
    *   Configuración de `vitest.config.ts` y `setupTests.ts` (con mocks globales).
    *   Creación de scripts `npm test`, `npm run test:coverage`.
2.  **Infraestructura de Tests E2E:**
    *   Instalación de Playwright.
    *   Configuración de `playwright.config.ts` para Electron.
    *   Creación de script `npm run e2e`.
3.  **Documentación y Gobernanza:**
    *   Registro de la skill `frontend-test`.
    *   Generación de auditorías de arquitectura y seguridad.
    *   Especificaciones formales en `openspecs/`.

## 4. Criterios de Éxito
*   [x] `npm run test:run` ejecuta tests unitarios exitosamente.
*   [x] `npm run test:coverage` genera reporte de cobertura.
*   [x] `npm run e2e` intenta lanzar la aplicación (validación de configuración).
*   [x] Documentación de arquitectura y seguridad actualizada.
