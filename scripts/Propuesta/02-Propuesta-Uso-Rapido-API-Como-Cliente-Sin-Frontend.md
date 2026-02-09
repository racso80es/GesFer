# Propuesta de uso rápido: API testeada como cliente (sin frontend)

**Objetivo:** Ejecutar solo los **tests de API** (Playwright como cliente HTTP) **sin levantar el frontend**, contra la API real o contra el mock.

---

## Requisitos

- Node.js 18+
- Terminal PowerShell (Windows)
- Para **API real:** backend Product en ejecución (puerto 5000)  
- Para **mock:** servidor mock en ejecución (puerto 5002)

---

## Opción A – Con API real (puerto 5000)

1. Asegurar que la **API Product** esté en ejecución en http://localhost:5000 (p. ej. Docker o `dotnet run` en el backend).

2. En la raíz del frontend Product:

```powershell
cd src\Product\Front
npm run test:e2e:api
```

No se levanta el cliente Next.js; solo se ejecutan los tests en `tests\api\` contra la API.

---

## Opción B – Con mock (sin API real)

1. Levantar el **mock** en una terminal:

```powershell
cd infrastructure\mock-apis
npm install
npm start
```

2. En **otra** terminal, ejecutar los tests de API apuntando al mock:

```powershell
cd src\Product\Front
$env:USE_MOCK_API="1"
$env:API_URL="http://127.0.0.1:5002"
npm run test:e2e:api
```

Se valida el contrato (request/response) del cliente contra el mock, sin backend real ni frontend.

---

## Resumen

| Modo     | Condición              | Comando / entorno                                      |
|----------|------------------------|--------------------------------------------------------|
| API real | API en 5000            | `npm run test:e2e:api`                                 |
| Mock     | Mock en 5002           | `USE_MOCK_API=1`, `API_URL=http://127.0.0.1:5002`, luego `npm run test:e2e:api` |

---

## Referencias

- Configuración Playwright API-only: `src\Product\Front\playwright.api-only.config.ts`
- Mocks: `infrastructure\mock-apis\README.md`
- Modos de test: `docs\infrastructure\MOCK_APIS_AND_TEST_MODES.md`
