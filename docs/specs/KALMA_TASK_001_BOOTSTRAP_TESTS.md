# KALMA_TASK_001: Bootstrap Test Infrastructure

**Estado:** Planificado
**Prioridad:** Crítica
**Responsable:** Jules (Engineer)

## 1. Objetivo
Habilitar la infraestructura de pruebas unitarias y reportes de cobertura para el proyecto `Kalma2/Interface/Desktop` (React + Electron), permitiendo validaciones de seguridad y arquitectura.

## 2. Alcance
- Directorio: `src/Kalma2/Interface/Desktop`
- Tecnologías: Vitest, React Testing Library, JSDOM.

## 3. Requerimientos Técnicos

### 3.1 Dependencias
Se deben instalar las siguientes librerías de desarrollo:
- `vitest`: Runner de tests rápido.
- `jsdom`: Entorno de navegador simulado para React.
- `@testing-library/react`: Utilidades para probar componentes React.
- `@testing-library/dom`: Utilidades base.
- `@vitest/coverage-v8`: Motor de cobertura.

### 3.2 Configuración (vitest.config.ts)
El archivo de configuración debe establecer:
- `test.environment`: 'jsdom'
- `test.globals`: true (para usar `describe`, `it`, `expect` sin importar).
- `test.setupFiles`: ['./src/setupTests.ts']

### 3.3 Setup Global (src/setupTests.ts)
Debe incluir mocks globales necesarios para el entorno de Electron/React:
- `window.matchMedia` (fix común de JSDOM).
- `window.calmaAPI` (mock parcial para evitar errores en componentes que usen IPC).

### 3.4 Scripts NPM
Se deben agregar al `package.json`:
- `"test": "vitest run"`
- `"coverage": "vitest run --coverage"`

## 4. Plan de Implementación

1.  **Instalación:** Ejecutar `npm install -D ...` en el directorio del proyecto.
2.  **Configuración:** Crear `vitest.config.ts`.
3.  **Setup:** Crear `src/setupTests.ts`.
4.  **Scripts:** Modificar `package.json`.
5.  **Verificación:** Crear `src/App.test.tsx` (Smoke Test) y ejecutar `npm run coverage`.
