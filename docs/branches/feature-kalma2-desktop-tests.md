# Objetivo de la Rama

Implementar la infraestructura de pruebas para `Kalma2/Interface/Desktop`.

## Descripción

Esta rama se enfoca en configurar Vitest, JSDOM y React Testing Library en el proyecto de escritorio para permitir la ejecución de pruebas unitarias y de cobertura, cumpliendo con los estándares de calidad definidos en `openspecs/skills/frontend-test.json`.

## Acciones Realizadas

- [x] Instalación de dependencias: `vitest`, `jsdom`, `@testing-library/react`, `@vitest/coverage-v8`.
- [x] Configuración de `vitest.config.ts`.
- [x] Creación de `src/setupTests.ts` con mocks globales para `window.calmaAPI`.
- [x] Implementación de `src/App.test.tsx` como smoke test.
- [x] Generación de informe de auditoría en `docs/audits/AUDITORIA_TEST_KALMA2_DESKTOP.md`.
