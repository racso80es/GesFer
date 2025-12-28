# Tests de Playwright - GesFer

Este directorio contiene los tests de automatización E2E y API usando Playwright.

## Estructura de Carpetas

```
tests/
├── api/                    # Tests de API (localhost:5000)
│   ├── api-client.ts        # Cliente API reutilizable
│   ├── auth-api.spec.ts
│   └── usuarios-api.spec.ts
├── e2e/                    # Tests End-to-End (localhost:3000)
│   ├── login.spec.ts
│   └── usuarios.spec.ts
├── fixtures/               # Fixtures reutilizables
│   └── auth.fixture.ts
└── page-objects/          # Page Object Model
    ├── BasePage.ts
    ├── LoginPage.ts
    ├── DashboardPage.ts
    └── UsuariosPage.ts
```

## Configuración

- **Web**: http://localhost:3000
- **API**: http://localhost:5000

La configuración se encuentra en `playwright.config.ts` en la raíz del proyecto.

## Comandos Disponibles

```bash
# Ejecutar todos los tests
npm run test:e2e

# Ejecutar tests con UI interactiva
npm run test:e2e:ui

# Ejecutar tests en modo debug
npm run test:e2e:debug

# Ejecutar tests con navegador visible
npm run test:e2e:headed

# Ver reporte HTML
npm run test:e2e:report
```

## Page Objects

Los Page Objects encapsulan la lógica de interacción con las páginas:

- `BasePage`: Clase base con funcionalidad común
- `LoginPage`: Manejo de la página de login
- `DashboardPage`: Manejo del dashboard
- `UsuariosPage`: Manejo de la página de usuarios

## Fixtures

Los fixtures proporcionan datos y configuraciones reutilizables:

- `authenticatedPage`: Página ya autenticada con token
- `apiClient`: Cliente API configurado

## Ejecutar Tests Específicos

```bash
# Solo tests de login
npx playwright test tests/e2e/login.spec.ts

# Solo tests de API
npx playwright test tests/api/

# Tests en un navegador específico
npx playwright test --project=chromium
```

## Requisitos Previos

1. La aplicación web debe estar ejecutándose en `http://localhost:3000`
2. La API debe estar ejecutándose en `http://localhost:5000`
3. Credenciales de prueba:
   - Empresa: "Empresa Demo"
   - Usuario: "admin"
   - Contraseña: "admin123"

## Notas

- Los tests se ejecutan en paralelo por defecto
- Los screenshots y videos se guardan solo cuando fallan
- Los traces se guardan solo en reintentos
- El reporte HTML se genera automáticamente después de cada ejecución

