# MANIFIESTO DE VALORES — GesFer

Este manifiesto define los pilares no negociables que gobiernan el comportamiento técnico y la toma de decisiones en GesFer.

---

## 1) Soberanía de Racso

- La soberanía es el principio rector: **la decisión final y la dirección estratégica** pertenecen a Racso.
- El código, la arquitectura y la automatización deben **servir** esa soberanía: claridad, trazabilidad y control.
- No se aceptan "fuentes de verdad" implícitas o dispersas sin jerarquía. La soberanía exige **una Puerta de Entrada** y **leyes operativas** claras.

---

## 2) Proactividad (objetiva)

- La proactividad es obligatoria: detectar incoherencias, riesgos, deuda y contradicciones antes de que se conviertan en incidentes.
- La objetividad es un contrato: afirmar solo lo verificable y documentar supuestos cuando no se pueda verificar.
- Toda acción debe dejar evidencia: documentación de rama, cambios trazables y validaciones reproducibles.

---

## 3) Rigor Técnico

- **Compilación**: no se entrega trabajo si el proyecto no compila.
- **Logs**: los logs son evidencia, no adorno. Deben soportar diagnóstico y auditoría.
- **AC-001 [LOGS]**: antes de cerrar una tarea, debe existir un autocheck reproducible que confirme que el trabajo no rompe el contrato de logs/validación.

---

## 4) Pragmatismo de Sector

- GesFer se construye para la **realidad operativa del sector tradicional** (recuperación/chatarrerías).
- Se prioriza la **utilidad en planta** (flujo real, velocidad operativa, claridad de uso) por encima de la abstracción técnica.
- Toda decisión técnica debe poder justificarse con impacto directo en la operativa: **compras**, **stock por familias**, **ventas** y **flujo de caja**.

---

## 5) Arquitectura de Dominios — Estructura Física (S+)

GesFer se organiza en **4 pilares arquitectónicos** que definen la separación de responsabilidades y la jerarquía de dependencias.

### 5.1) Shared — ADN Común

**Propósito**: Componentes transversales compartidos entre todos los dominios.

**Estructura**:
```
src/Shared/
├── Back/              # Backend compartido (C#)
│   └── src/
│       └── domain/
│           ├── Common/        # BaseEntity, interfaces comunes
│           ├── Entities/     # Entidades geográficas (Country, State, City, Language, PostalCode)
│           └── ValueObjects/ # Value Objects base (Email, TaxId)
└── Front/             # Frontend compartido (TypeScript/React)
    ├── components/     # Componentes UI puros (Button, Input, DataTable, ModalBase)
    ├── lib/
    │   ├── types/     # Tipos TypeScript compartidos
    │   └── utils/     # Utilidades (cn, validators)
    └── ...
```

**Reglas de Oro**:
- **Shared es sagrado**: No puede depender de Product ni de Admin.
- Solo contiene código que es **genuinamente transversal** (geografía, UI base, tipos comunes).
- Cualquier componente que tenga lógica de negocio específica debe residir en Product o Admin.

### 5.2) Product — Aplicación Principal

**Propósito**: Sistema multi-empresa operativo (post-login).

**Estructura**:
```
src/Product/
├── Back/              # Backend Product (C# / .NET 8)
│   └── src/
│       ├── Api/       # Web API (Minimal API)
│       ├── application/
│       ├── domain/    # Entidades de dominio Product
│       └── Infrastructure/
│           ├── Data/  # DbContext, Repositories
│           └── Services/
└── Front/             # Frontend Product (Next.js / TypeScript)
    ├── app/           # App Router (Next.js 14+)
    │   ├── [locale]/  # Rutas multi-idioma
    │   └── (client)/  # Rutas del cliente
    ├── components/    # Componentes específicos de Product
    ├── lib/          # Lógica de negocio, API clients
    └── ...
```

**Reglas de Oro**:
- Product puede consumir Shared (Back y Front).
- Product **no puede** consumir Admin.
- Product es multi-tenant: toda operación está asociada a una empresa (`CompanyId`).

### 5.3) Admin — Gestión Global

**Propósito**: Sistema administrativo global (identidad única, no multi-empresa).

**Estructura**:
```
src/Admin/
├── Back/              # Backend Admin (C# / .NET 8)
│   └── src/
│       ├── Api/       # Web API (Minimal API)
│       ├── application/
│       ├── domain/    # Entidades de dominio Admin (AdminUser, AuditLog, Log)
│       └── Infrastructure/
│           ├── Data/  # DbContext Admin
│           └── Services/
└── Front/             # Frontend Admin (Next.js / TypeScript)
    ├── app/
    │   ├── dashboard/ # Dashboard administrativo
    │   ├── login/     # Login Admin (no multi-empresa)
    │   └── api/       # API routes (NextAuth)
    ├── components/    # Componentes específicos de Admin
    └── ...
```

**Reglas de Oro**:
- Admin puede consumir Shared (Back y Front).
- Admin **no puede** consumir Product.
- Admin es **global**: no tiene concepto de empresa (`CompanyId`).
- Admin usa autenticación separada (JWT con claim `role: Admin`).

### 5.4) Utils — Herramientas Transversales

**Propósito**: Herramientas de orquestación y datos maestros.

**Estructura**:
```
src/Utils/
└── Data/
    └── Seeds/         # Taxonomía de seeds (Master, Demo, Test)
        ├── master/    # Datos maestros
        ├── demo/      # Datos de demostración
        └── test/      # Datos de prueba

src/Console/           # Consola de orquestación (C#)
├── Services/          # Servicios de consola
│   ├── SeedService.cs
│   ├── MigrationService.cs
│   ├── DatabaseInitializationService.cs
│   └── ...
└── ...
```

**Reglas de Oro**:
- Utils puede consumir Shared, Product y Admin (para orquestación).
- Utils contiene la lógica de seeding y migración de base de datos.
- La Consola es el punto de entrada para operaciones administrativas del sistema.

---

## 6) Mapa de Rutas Físicas — Migración Legacy → Nueva Estructura

### 6.1) Backend

| Ruta Legacy | Ruta Nueva | Dominio |
|-------------|------------|---------|
| `./Api/src/Api` | `./src/Product/Back/src/Api` | Product |
| `./Api/src/domain` | `./src/Product/Back/src/domain` | Product |
| `./Api/src/Infrastructure` | `./src/Product/Back/src/Infrastructure` | Product |
| `./Api/src/IntegrationTests` | `./src/Product/Back/src/IntegrationTests` | Product |
| — | `./src/Admin/Back/src/Api` | Admin (nuevo) |
| — | `./src/Admin/Back/src/domain` | Admin (nuevo) |
| — | `./src/Admin/Back/src/Infrastructure` | Admin (nuevo) |
| — | `./src/Shared/Back/src/domain` | Shared (nuevo) |

### 6.2) Frontend

| Ruta Legacy | Ruta Nueva | Dominio |
|-------------|------------|---------|
| `./Cliente` | `./src/Product/Front` | Product |
| `./Cliente/app/(admin)` | `./src/Admin/Front/app/dashboard` | Admin (migrado) |
| `./Cliente/components/shared` | `./src/Shared/Front/components/shared` | Shared (migrado) |
| `./Cliente/components/ui` | `./src/Shared/Front/components/ui` | Shared (migrado) |

### 6.3) Herramientas

| Ruta Legacy | Ruta Nueva | Dominio |
|-------------|------------|---------|
| `./GesFer.Console` | `./src/Console` | Utils |
| `./Api/src/Infrastructure/Data/Seeds` | `./src/Utils/Data/Seeds` | Utils (nueva taxonomía) |

### 6.4) Configuración

| Ruta Legacy | Ruta Nueva | Dominio |
|-------------|------------|---------|
| `./Api/docker-compose.yml` | `./src/Product/Back/docker-compose.yml` | Product |
| `./Api/appsettings.json` | `./src/Product/Back/src/Api/appsettings.json` | Product |
| — | `./src/Admin/Back/src/Api/appsettings.json` | Admin (nuevo) |

**Nota**: Las rutas legacy se mantienen como fallback temporal en `JsonDataSeeder.cs` con warnings. La prioridad es siempre la nueva estructura.

---

## 7) Invariantes de Dependencia — Jerarquía de Consumo (S+)

### 7.1) Regla Fundamental

**Shared es sagrado**: No puede depender de Product ni de Admin.

```
┌─────────┐
│ Shared  │  ← No depende de nadie
└────┬────┘
     │
     ├──────────────┐
     │              │
┌────▼────┐    ┌────▼────┐
│ Product │    │  Admin  │  ← Solo dependen de Shared
└─────────┘    └─────────┘
     │              │
     └──────┬───────┘
            │
     ┌──────▼──────┐
     │    Utils    │  ← Puede consumir Shared, Product y Admin
     │  (Console)  │     (para orquestación)
     └─────────────┘
```

### 7.2) Prohibiciones Explícitas

#### Backend (C#)

- ❌ **Prohibido**: `using GesFer.Product.Back.*` en `src/Shared/Back/`
- ❌ **Prohibido**: `using GesFer.Admin.Back.*` en `src/Shared/Back/`
- ❌ **Prohibido**: `using GesFer.Admin.Back.*` en `src/Product/Back/`
- ❌ **Prohibido**: `using GesFer.Product.Back.*` en `src/Admin/Back/`
- ✅ **Permitido**: `using GesFer.Shared.Back.*` en Product y Admin
- ✅ **Permitido**: `using GesFer.*` en `src/Console/` (orquestación)

#### Frontend (TypeScript)

- ❌ **Prohibido**: `import` desde `@product` o `@admin` en `src/Shared/Front/`
- ❌ **Prohibido**: `import` desde `@admin` en `src/Product/Front/`
- ❌ **Prohibido**: `import` desde `@product` en `src/Admin/Front/`
- ✅ **Permitido**: `import` desde `@shared` en Product y Admin
- ✅ **Permitido**: Rutas relativas dentro del mismo dominio

### 7.3) Separación de Identidad (Admin ↔ Product)

- **Admin es global**: No tiene concepto de empresa (`CompanyId`).
- **Product es multi-tenant**: Toda operación requiere `CompanyId`.
- **Prohibido**: Que Admin consuma DTOs/contratos de Product (ej. `LoginRequestDto` con `CompanyId`).
- **Prohibido**: Que Product consuma DTOs/contratos de Admin (ej. `AdminLoginRequest`).
- **Obligatorio**: Namespaces separados para autenticación:
  - Admin: `admin_*` (cookies, tokens, sesiones)
  - Product: `auth_*` (cookies, tokens, sesiones)

### 7.4) Validación en Compilación

- La Consola **no debe compilar** si hay referencias directas a rutas legacy (`./Api`, `./Cliente`).
- Los proyectos deben usar `ProjectReference` correctos:
  - Product: referencia `Shared/Back`
  - Admin: referencia `Shared/Back`
  - Console: referencia `Shared/Back`, `Product/Back`, `Admin/Back`

---

## 8) Taxonomía de Seeds — Sistema de 3 Niveles

### 8.1) Estructura Física

```
src/Utils/Data/Seeds/
├── master/                    # Datos maestros esenciales
│   ├── master-data.json       # Shared: Languages, Countries, States, Cities, PostalCodes
│   ├── admin-master-data.json # Admin: AdminUser base
│   └── product-master-data.json # Product: Permissions, Groups
├── demo/                      # Datos de demostración
│   ├── demo-data.json         # Shared: Datos demo compartidos
│   ├── admin-demo-data.json   # Admin: Datos demo Admin
│   └── product-demo-data.json # Product: Companies, Users, Customers, Articles
└── test/                      # Datos de prueba
    ├── test-data.json         # Shared: Datos test compartidos
    ├── admin-test-data.json   # Admin: Datos test Admin
    └── product-test-data.json # Product: Datos test con IDs fijos
```

### 8.2) Ámbitos (Scopes)

#### Shared
- Languages (idiomas)
- Countries, States, Cities, PostalCodes (geografía)

#### Admin
- AdminUsers (usuarios administrativos)
- AuditLogs (logs de auditoría)
- Logs (logs del sistema)

#### Product
- Companies (empresas)
- Users (usuarios)
- Customers, Suppliers (terceros)
- Articles, Families (catálogo)
- Permissions, Groups (RBAC)

### 8.3) Niveles

#### Master
- **Propósito**: Datos maestros esenciales que deben existir siempre.
- **Uso**: Inicialización de base de datos, entornos de producción.
- **Características**: Datos mínimos, válidos, sin IDs fijos (UUIDs generados).

#### Demo
- **Propósito**: Datos de demostración para entornos de desarrollo/demo.
- **Uso**: Desarrollo local, presentaciones, pruebas de flujo completo.
- **Características**: Datos realistas, relaciones completas, sin IDs fijos.

#### Test
- **Propósito**: Datos de prueba para entornos de testing.
- **Uso**: Tests de integración, tests E2E, validación de reglas de negocio.
- **Características**: IDs fijos, datos controlados, casos edge incluidos.

### 8.4) Uso desde Consola

La consola (`src/Console`) permite seleccionar:

1. **Ámbito**: 
   - `[1] Shared` — Solo datos compartidos
   - `[2] Admin` — Solo datos Admin
   - `[3] Product` — Solo datos Product
   - `[4] All` — Todos los ámbitos

2. **Nivel**:
   - `[1] Master` — Datos maestros
   - `[2] Demo` — Datos de demostración
   - `[3] Test` — Datos de prueba

### 8.5) Migración desde Estructura Legacy

- **Legacy**: `src/Product/Back/src/Infrastructure/Data/Seeds/` (mantenido como fallback temporal).
- **Nueva**: `src/Utils/Data/Seeds/{level}/{scope}-data.json` (prioritaria).
- **Comportamiento**: `JsonDataSeeder.cs` prioriza la nueva estructura y emite warnings si usa rutas legacy.

---

## 9) Referencias y Fuentes de Verdad

### 9.1) Documentos Soberanos

- **`/Tekton/Configuration/MANIFESTO.md`** (este documento): Valores y arquitectura.
- **`/Tekton/Rules/GOLDEN_RULES.md`**: Leyes operativas y reglas de enforcement.

### 9.2) Documentos Derivados

- `docs/branches/<rama>.md`: Documentación de rama (obligatoria).
- `docs/Output/LEGACY-REFERENCES-FOUND.md`: Reporte de auditoría Zero-Legacy Policy.
- `src/Utils/Data/Seeds/README.md`: Documentación de taxonomía de seeds.

### 9.3) Precedencia

Si existe contradicción entre documentos, **prevalece**:
1. `GOLDEN_RULES.md` (leyes operativas)
2. `MANIFESTO.md` (este documento, valores y arquitectura)
3. Documentos derivados (documentación de rama, reportes)

---

**Última Actualización**: 2026-01-26  
**Versión**: 2.0 (Post-Reestructuración a ./src)
