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

**E2E (navegador + API):**
1. La aplicación web debe estar ejecutándose en `http://localhost:3000` (o la levanta Playwright con webServer)
2. La API debe estar en `http://localhost:5000` **o** usar mock: `USE_MOCK_API=1` y `API_URL=http://localhost:5002` (tras levantar `infrastructure/mock-apis`)

**Solo tests de API (sin frontend):** `npm run test:e2e:api`. La API debe estar en 5000, o mock en 5002 con `USE_MOCK_API=1` y `API_URL=http://localhost:5002`.

### E2E API (Product Back) con mock

Tests E2E que validan **solo el backend (API) de Product**, con dependencias mockeadas (sin API real ni BD):

1. **Levantar el mock** (Product en 5002):
   ```powershell
   cd infrastructure\mock-apis
   npm install
   npm start
   ```
2. **Ejecutar la suite de API** desde Product Front:
   ```powershell
   cd src\Product\Front
   $env:USE_MOCK_API="1"; $env:API_URL="http://127.0.0.1:5002"
   npm run test:e2e:api
   ```
   Ver también: `docs/infrastructure/MOCK_APIS_AND_TEST_MODES.md`.

Credenciales de prueba (API real o mock):
- Company: "Emp" + "resa Demo"
- Usuario: "admin"
- Contraseña: "admin123"

## Reporte HTML (localhost:9323)

Tras ejecutar `npm run test:e2e`, puedes abrir el reporte con:

```bash
npm run test:e2e:report
```

Playwright abre el reporte en un puerto disponible (por ejemplo **http://localhost:9323**). Ahí ves qué tests pasaron o fallaron, traces y capturas. Si todos los tests salen "malos", suele ser porque **la API no estaba en ejecución** al correr los tests: el `globalSetup` comprueba que la API (puerto 5000) responda antes de empezar; si no, falla con un mensaje claro.

## Notas

- Los tests se ejecutan en paralelo por defecto
- **Antes de ejecutar E2E:** la API Product debe estar levantada en el puerto 5000 (p. ej. `docker-compose up -d gesfer-product-api` o ejecutar el backend desde IDE)
- Los screenshots y videos se guardan solo cuando fallan
- Los traces se guardan solo en reintentos
- El reporte HTML se genera automáticamente después de cada ejecución

