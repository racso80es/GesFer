# Propuesta de uso rápido: Clientes validados con APIs mock

**Objetivo:** Validar los frontends (Product y Admin) cuando **no hay acceso a las APIs reales**, usando servidores mock.

---

## Requisitos

- Node.js 18+
- Terminal PowerShell (Windows)

---

## Paso 1: Levantar los mocks

En una terminal:

```powershell
cd infrastructure\mock-apis
npm install
npm start
```

Dejar esta terminal abierta. Por defecto quedan:

- **Product mock** → http://localhost:5002  
- **Admin mock** → http://localhost:5012  

---

## Paso 2: Levantar el cliente que quieras validar

### Opción A – Product (cliente multi-tenant)

En **otra** terminal:

```powershell
cd src\Product\Front
$env:NEXT_PUBLIC_API_URL="http://localhost:5002"
npm run dev
```

- Abrir en el navegador: **http://localhost:3000/login**
- Credenciales mock:
  - **Empresa:** Empresa Demo  
  - **Usuario:** admin  
  - **Contraseña:** admin123  

### Opción B – Admin (panel administrativo)

En **otra** terminal:

```powershell
cd src\Admin\Front
$env:ADMIN_API_URL="http://localhost:5012"
npm run dev
```

- Abrir en el navegador: **http://localhost:3001/login**
- Credenciales mock:
  - **Usuario:** admin  
  - **Contraseña:** admin  

---

## Resumen

| Cliente   | URL login              | Variable de entorno              | Credenciales mock                          |
|----------|------------------------|----------------------------------|-------------------------------------------|
| Product  | http://localhost:3000/login  | `NEXT_PUBLIC_API_URL=http://localhost:5002` | Empresa Demo / admin / admin123 |
| Admin    | http://localhost:3001/login | `ADMIN_API_URL=http://localhost:5012`       | admin / admin                    |

---

## Referencias

- Detalle de mocks: `infrastructure\mock-apis\README.md`
- Infraestructura y modos de test: `docs\infrastructure\MOCK_APIS_AND_TEST_MODES.md`
