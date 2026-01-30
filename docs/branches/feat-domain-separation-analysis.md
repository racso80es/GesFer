# Análisis Inicial: Separación de Dominios Product/Admin

**Fecha**: 2026-01-26  
**Rama**: `feat/domain-separation` (pendiente de creación)  
**Autor**: Senior Software Architect / Tekton Governance  
**Estado**: FASE 1 - Análisis y Estructuración

---

## CONTEXTO Y OBJETIVO

Reinicio de la separación total de dominios **Product** y **Admin** en API y Cliente, orquestados bajo una nueva estructura en `./src` que aglutinará:

- `src/Shared/`: ADN común (ValueObjects en C#, Componentes Base en Node)
- `src/Product/`: API y Frontend operativo post-login (multi-tenant)
- `src/Admin/`: Nueva API y nuevo Frontend (gestión global)
- `src/Utils/`: Consola global para orquestación de Seeds

---

## 1. ANÁLISIS DE IMPACTO - COMPONENTES ACTUALES

### 1.1 Cliente Frontend - Componentes Admin

#### Rutas Admin (Exclusivas)
- `Cliente/app/(admin)/admin/login/page.tsx` → `src/Admin/Frontend/app/admin/login/page.tsx`
- `Cliente/app/(admin)/admin/dashboard/page.tsx` → `src/Admin/Frontend/app/admin/dashboard/page.tsx`
- `Cliente/app/(admin)/layout.tsx` → `src/Admin/Frontend/app/(admin)/layout.tsx`

#### Componentes Admin (Exclusivos)
- `Cliente/components/layout/admin-layout.tsx` → `src/Admin/Frontend/components/layout/admin-layout.tsx`

#### Servicios/API Admin (Exclusivos)
- **Ninguno identificado actualmente** - Los endpoints Admin se consumen directamente desde páginas

#### Auth Admin (Exclusivo)
- Provider `admin` en `Cliente/auth.ts` (líneas 67-117) → `src/Admin/Frontend/auth.ts`

---

### 1.2 Cliente Frontend - Componentes Product

#### Rutas Product (Operativas post-login)
- `Cliente/app/(client)/*` → `src/Product/Frontend/app/(client)/*`
- `Cliente/app/[locale]/*` → `src/Product/Frontend/app/[locale]/*`
  - `/login` (multi-tenant con selector de empresa)
  - `/dashboard`
  - `/empresas`
  - `/usuarios`
  - `/clientes`
  - `/perfil`

#### Componentes Product (Exclusivos)
- `Cliente/components/empresas/company-form.tsx` → `src/Product/Frontend/components/companies/company-form.tsx`
- `Cliente/components/usuarios/user-form.tsx` → `src/Product/Frontend/components/users/user-form.tsx`
- `Cliente/components/layout/main-layout.tsx` → `src/Product/Frontend/components/layout/main-layout.tsx`
- `Cliente/components/layout/Sidebar.tsx` → `src/Product/Frontend/components/layout/Sidebar.tsx`

#### Servicios/API Product
- `Cliente/lib/api/auth.ts` → `src/Product/Frontend/lib/api/auth.ts`
- `Cliente/lib/api/users.ts` → `src/Product/Frontend/lib/api/users.ts`
- `Cliente/lib/api/companies.ts` → `src/Product/Frontend/lib/api/companies.ts`
- `Cliente/lib/api/customers.ts` → `src/Product/Frontend/lib/api/customers.ts`
- `Cliente/lib/api/logs.ts` → **ANÁLISIS REQUERIDO**: ¿Admin o Product?

#### Auth Product (Multi-tenant)
- Provider `credentials` en `Cliente/auth.ts` (líneas 13-65) → `src/Product/Frontend/auth.ts`

---

### 1.3 Cliente Frontend - Componentes Shared

#### Componentes UI Base (Inmutables)
- `Cliente/components/shared/*` → `src/Shared/Frontend/components/shared/*`
  - `Button.tsx`
  - `Input.tsx`
  - `DataTable.tsx`
  - `ModalBase.tsx`
  - `DestructiveActionConfirm.tsx`

#### Componentes UI (shadcn/ui)
- `Cliente/components/ui/*` → `src/Shared/Frontend/components/ui/*`
  - `button.tsx`, `input.tsx`, `card.tsx`, `dialog.tsx`, `label.tsx`, `loading.tsx`, `error-message.tsx`, `overlay-fix.tsx`

#### Utilidades Neutrales
- `Cliente/lib/utils/cn.ts` → `src/Shared/Frontend/lib/utils/cn.ts`
- `Cliente/lib/utils/locale.ts` → `src/Shared/Frontend/lib/utils/locale.ts`
- `Cliente/lib/utils/id-validation.ts` → **ANÁLISIS REQUERIDO**: ¿Shared o Product?

#### Contextos
- `Cliente/contexts/sidebar-context.tsx` → **ANÁLISIS REQUERIDO**: ¿Product exclusivo o Shared?
- `Cliente/contexts/auth-context.tsx` → **ANÁLISIS REQUERIDO**: ¿Product exclusivo o Shared?

#### Configuración
- `Cliente/lib/config.ts` → `src/Shared/Frontend/lib/config.ts`
- `Cliente/messages/*.json` → `src/Shared/Frontend/messages/*.json` (i18n)

---

### 1.4 API Backend - Controladores Admin

#### Controladores Admin (Exclusivos)
- `Api/src/Api/Controllers/AdminAuthController.cs` → `src/Admin/Api/Controllers/AdminAuthController.cs`
- `Api/src/Api/Controllers/DashboardController.cs` → `src/Admin/Api/Controllers/DashboardController.cs`
- `Api/src/Api/Controllers/LogController.cs` → **ANÁLISIS REQUERIDO**: ¿Admin exclusivo? (Actualmente con `[Authorize(Policy = "AdminOnly")]`)

#### DTOs Admin (Exclusivos)
- `Api/src/application/DTOs/Admin/Auth/AdminLoginRequest.cs` → `src/Admin/Api/Application/DTOs/Auth/AdminLoginRequest.cs`
- `Api/src/application/DTOs/Admin/Auth/AdminLoginResponse.cs` → `src/Admin/Api/Application/DTOs/Auth/AdminLoginResponse.cs`
- `Api/src/application/DTOs/Admin/DashboardSummaryDto.cs` → `src/Admin/Api/Application/DTOs/DashboardSummaryDto.cs`

#### Servicios Admin (Exclusivos)
- `Api/src/Infrastructure/Services/AdminAuthService.cs` → `src/Admin/Api/Infrastructure/Services/AdminAuthService.cs`
- `Api/src/Infrastructure/Services/AdminJwtService.cs` → `src/Admin/Api/Infrastructure/Services/AdminJwtService.cs`
- `Api/src/Infrastructure/Services/IAuditLogService.cs` → **ANÁLISIS REQUERIDO**: ¿Admin exclusivo?
- `Api/src/Infrastructure/Services/AuditLogService.cs` → **ANÁLISIS REQUERIDO**: ¿Admin exclusivo?

#### Entidades Admin (Exclusivas)
- `Api/src/domain/Entities/AdminUser.cs` → `src/Admin/Api/Domain/Entities/AdminUser.cs`
- `Api/src/domain/Entities/AuditLog.cs` → **ANÁLISIS REQUERIDO**: ¿Admin exclusivo o Shared?

---

### 1.5 API Backend - Controladores Product

#### Controladores Product (Multi-tenant)
- `Api/src/Api/Controllers/AuthController.cs` → `src/Product/Api/Controllers/AuthController.cs`
- `Api/src/Api/Controllers/UserController.cs` → `src/Product/Api/Controllers/UserController.cs`
- `Api/src/Api/Controllers/CompanyController.cs` → `src/Product/Api/Controllers/CompanyController.cs`
- `Api/src/Api/Controllers/CustomerController.cs` → `src/Product/Api/Controllers/CustomerController.cs`
- `Api/src/Api/Controllers/SupplierController.cs` → `src/Product/Api/Controllers/SupplierController.cs`
- `Api/src/Api/Controllers/GroupController.cs` → `src/Product/Api/Controllers/GroupController.cs`
- `Api/src/Api/Controllers/ProfileController.cs` → `src/Product/Api/Controllers/ProfileController.cs`
- `Api/src/Api/Controllers/SetupController.cs` → **ANÁLISIS REQUERIDO**: ¿Admin o Product?
- `Api/src/Api/Controllers/HealthController.cs` → `src/Shared/Api/Controllers/HealthController.cs`
- `Api/src/Api/Controllers/TelemetryController.cs` → `src/Shared/Api/Controllers/TelemetryController.cs`

#### DTOs Product
- `Api/src/application/DTOs/Auth/LoginRequestDto.cs` → `src/Product/Api/Application/DTOs/Auth/LoginRequestDto.cs`
- `Api/src/application/DTOs/Auth/LoginResponseDto.cs` → `src/Product/Api/Application/DTOs/Auth/LoginResponseDto.cs`
- `Api/src/application/DTOs/User/UserDto.cs` → `src/Product/Api/Application/DTOs/User/UserDto.cs`
- `Api/src/application/DTOs/Company/CompanyDto.cs` → `src/Product/Api/Application/DTOs/Company/CompanyDto.cs`
- `Api/src/application/DTOs/Customer/CustomerDto.cs` → `src/Product/Api/Application/DTOs/Customer/CustomerDto.cs`
- `Api/src/application/DTOs/Supplier/SupplierDto.cs` → `src/Product/Api/Application/DTOs/Supplier/SupplierDto.cs`
- `Api/src/application/DTOs/Group/GroupDto.cs` → `src/Product/Api/Application/DTOs/Group/GroupDto.cs`

#### Servicios Product
- `Api/src/Infrastructure/Services/AuthService.cs` → `src/Product/Api/Infrastructure/Services/AuthService.cs`
- `Api/src/Infrastructure/Services/JwtService.cs` → `src/Product/Api/Infrastructure/Services/JwtService.cs`

#### Entidades Product (Multi-tenant)
- `Api/src/domain/Entities/User.cs` → `src/Product/Api/Domain/Entities/User.cs`
- `Api/src/domain/Entities/Company.cs` → `src/Product/Api/Domain/Entities/Company.cs`
- `Api/src/domain/Entities/Customer.cs` → `src/Product/Api/Domain/Entities/Customer.cs`
- `Api/src/domain/Entities/Supplier.cs` → `src/Product/Api/Domain/Entities/Supplier.cs`
- `Api/src/domain/Entities/Group.cs` → `src/Product/Api/Domain/Entities/Group.cs`
- `Api/src/domain/Entities/Permission.cs` → **ANÁLISIS REQUERIDO**: ¿Shared o Product?
- `Api/src/domain/Entities/UserPermission.cs` → **ANÁLISIS REQUERIDO**: ¿Shared o Product?
- `Api/src/domain/Entities/UserGroup.cs` → **ANÁLISIS REQUERIDO**: ¿Shared o Product?
- `Api/src/domain/Entities/GroupPermission.cs` → **ANÁLISIS REQUERIDO**: ¿Shared o Product?

#### Entidades de Dominio de Negocio (Product)
- `Api/src/domain/Entities/Article.cs` → `src/Product/Api/Domain/Entities/Article.cs`
- `Api/src/domain/Entities/Family.cs` → `src/Product/Api/Domain/Entities/Family.cs`
- `Api/src/domain/Entities/Tariff.cs` → `src/Product/Api/Domain/Entities/Tariff.cs`
- `Api/src/domain/Entities/TariffItem.cs` → `src/Product/Api/Domain/Entities/TariffItem.cs`
- `Api/src/domain/Entities/PurchaseDeliveryNote.cs` → `src/Product/Api/Domain/Entities/PurchaseDeliveryNote.cs`
- `Api/src/domain/Entities/PurchaseInvoice.cs` → `src/Product/Api/Domain/Entities/PurchaseInvoice.cs`
- `Api/src/domain/Entities/SalesDeliveryNote.cs` → `src/Product/Api/Domain/Entities/SalesDeliveryNote.cs`
- `Api/src/domain/Entities/SalesInvoice.cs` → `src/Product/Api/Domain/Entities/SalesInvoice.cs`

#### Entidades Geográficas (Shared)
- `Api/src/domain/Entities/Country.cs` → `src/Shared/Api/Domain/Entities/Country.cs`
- `Api/src/domain/Entities/State.cs` → `src/Shared/Api/Domain/Entities/State.cs`
- `Api/src/domain/Entities/City.cs` → `src/Shared/Api/Domain/Entities/City.cs`
- `Api/src/domain/Entities/PostalCode.cs` → `src/Shared/Api/Domain/Entities/PostalCode.cs`
- `Api/src/domain/Entities/Language.cs` → `src/Shared/Api/Domain/Entities/Language.cs`

---

### 1.6 API Backend - Componentes Shared

#### Value Objects (Shared - Sagrado)
- `Api/src/domain/ValueObjects/Email.cs` → `src/Shared/Api/Domain/ValueObjects/Email.cs`
- `Api/src/domain/ValueObjects/TaxId.cs` → `src/Shared/Api/Domain/ValueObjects/TaxId.cs`

#### Base Entities (Shared)
- `Api/src/domain/BaseEntity.cs` → `src/Shared/Api/Domain/BaseEntity.cs`

#### Infraestructura Compartida
- `Api/src/Infrastructure/Data/ApplicationDbContext.cs` → **ANÁLISIS REQUERIDO**: ¿Cómo dividir? ¿Shared con DbSets por dominio?
- `Api/src/Infrastructure/Repositories/IRepository.cs` → `src/Shared/Api/Infrastructure/Repositories/IRepository.cs`
- `Api/src/Infrastructure/Repositories/Repository.cs` → `src/Shared/Api/Infrastructure/Repositories/Repository.cs`
- `Api/src/Infrastructure/Data/ISequentialGuidGenerator.cs` → `src/Shared/Api/Infrastructure/Data/ISequentialGuidGenerator.cs`
- `Api/src/Infrastructure/Data/SequentialGuidGenerator.cs` → `src/Shared/Api/Infrastructure/Data/SequentialGuidGenerator.cs`

#### Servicios de Dominio (Shared)
- `Api/src/domain/Services/IStockService.cs` → **ANÁLISIS REQUERIDO**: ¿Product exclusivo?
- `Api/src/Infrastructure/Services/StockService.cs` → **ANÁLISIS REQUERIDO**: ¿Product exclusivo?

---

### 1.7 Seeds y Consola

#### Seeds Actuales
- `Api/src/Infrastructure/Data/Seeds/master-data.json` → `src/Utils/Seeds/master-data.json`
- `Api/src/Infrastructure/Data/Seeds/demo-data.json` → `src/Utils/Seeds/demo-data.json`
- `Api/src/Infrastructure/Data/Seeds/test-data.json` → `src/Utils/Seeds/test-data.json`

#### Servicios de Seeding
- `Api/src/Infrastructure/Services/JsonDataSeeder.cs` → `src/Utils/Services/JsonDataSeeder.cs`
- `Api/src/Infrastructure/Services/MasterDataSeeder.cs` → `src/Utils/Services/MasterDataSeeder.cs`
- `Api/src/Infrastructure/Data/DbInitializer.cs` → `src/Utils/Data/DbInitializer.cs`

#### Consola
- `GesFer.Console/Program.cs` → `src/Utils/Console/Program.cs`
- `GesFer.Console/Services/*` → `src/Utils/Console/Services/*`

---

## 2. ÁRBOL DE DIRECTORIOS PROYECTADO - ./src

```
src/
├── Shared/                          # ADN común - SAGRADO (no depende de Product ni Admin)
│   ├── Api/                         # API compartida (Health, Telemetry, Geografía)
│   │   ├── Controllers/
│   │   │   ├── HealthController.cs
│   │   │   └── TelemetryController.cs
│   │   ├── Domain/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Entities/
│   │   │   │   ├── Country.cs
│   │   │   │   ├── State.cs
│   │   │   │   ├── City.cs
│   │   │   │   ├── PostalCode.cs
│   │   │   │   └── Language.cs
│   │   │   └── ValueObjects/
│   │   │       ├── Email.cs
│   │   │       └── TaxId.cs
│   │   └── Infrastructure/
│   │       ├── Data/
│   │       │   ├── ISequentialGuidGenerator.cs
│   │       │   └── SequentialGuidGenerator.cs
│   │       └── Repositories/
│   │           ├── IRepository.cs
│   │           └── Repository.cs
│   └── Frontend/                    # Frontend compartido
│       ├── components/
│       │   ├── shared/              # Componentes inmutables
│       │   │   ├── Button.tsx
│       │   │   ├── Input.tsx
│       │   │   ├── DataTable.tsx
│       │   │   ├── ModalBase.tsx
│       │   │   └── DestructiveActionConfirm.tsx
│       │   └── ui/                  # shadcn/ui
│       │       ├── button.tsx
│       │       ├── input.tsx
│       │       ├── card.tsx
│       │       ├── dialog.tsx
│       │       ├── label.tsx
│       │       ├── loading.tsx
│       │       ├── error-message.tsx
│       │       └── overlay-fix.tsx
│       ├── lib/
│       │   ├── config.ts
│       │   └── utils/
│       │       ├── cn.ts
│       │       └── locale.ts
│       └── messages/                # i18n
│           ├── es.json
│           ├── en.json
│           └── ca.json
│
├── Product/                         # Dominio Product (multi-tenant)
│   ├── Api/                         # API Product
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── UserController.cs
│   │   │   ├── CompanyController.cs
│   │   │   ├── CustomerController.cs
│   │   │   ├── SupplierController.cs
│   │   │   ├── GroupController.cs
│   │   │   └── ProfileController.cs
│   │   ├── Application/
│   │   │   └── DTOs/
│   │   │       ├── Auth/
│   │   │       │   ├── LoginRequestDto.cs
│   │   │       │   └── LoginResponseDto.cs
│   │   │       ├── User/
│   │   │       │   └── UserDto.cs
│   │   │       ├── Company/
│   │   │       │   └── CompanyDto.cs
│   │   │       ├── Customer/
│   │   │       │   └── CustomerDto.cs
│   │   │       ├── Supplier/
│   │   │       │   └── SupplierDto.cs
│   │   │       └── Group/
│   │   │           └── GroupDto.cs
│   │   ├── Domain/
│   │   │   └── Entities/
│   │   │       ├── User.cs
│   │   │       ├── Company.cs
│   │   │       ├── Customer.cs
│   │   │       ├── Supplier.cs
│   │   │       ├── Group.cs
│   │   │       ├── Permission.cs
│   │   │       ├── UserPermission.cs
│   │   │       ├── UserGroup.cs
│   │   │       ├── GroupPermission.cs
│   │   │       ├── Article.cs
│   │   │       ├── Family.cs
│   │   │       ├── Tariff.cs
│   │   │       ├── TariffItem.cs
│   │   │       ├── PurchaseDeliveryNote.cs
│   │   │       ├── PurchaseInvoice.cs
│   │   │       ├── SalesDeliveryNote.cs
│   │   │       └── SalesInvoice.cs
│   │   └── Infrastructure/
│   │       ├── Services/
│   │       │   ├── AuthService.cs
│   │       │   ├── JwtService.cs
│   │       │   └── StockService.cs
│   │       └── Data/
│   │           └── Configurations/  # EF Core configurations para entidades Product
│   │
│   └── Frontend/                    # Frontend Product
│       ├── app/
│       │   ├── (client)/
│       │   │   ├── login/
│       │   │   │   └── page.tsx
│       │   │   ├── dashboard/
│       │   │   │   └── page.tsx
│       │   │   ├── empresas/
│       │   │   │   ├── page.tsx
│       │   │   │   └── [id]/
│       │   │   │       └── page.tsx
│       │   │   ├── usuarios/
│       │   │   │   ├── page.tsx
│       │   │   │   └── [id]/
│       │   │   │       └── page.tsx
│       │   │   ├── clientes/
│       │   │   │   └── page.tsx
│       │   │   └── perfil/
│       │   │       └── page.tsx
│       │   └── [locale]/
│       │       └── ... (rutas con i18n)
│       ├── components/
│       │   ├── layout/
│       │   │   ├── main-layout.tsx
│       │   │   └── Sidebar.tsx
│       │   ├── companies/
│       │   │   └── company-form.tsx
│       │   └── users/
│       │       └── user-form.tsx
│       ├── lib/
│       │   ├── api/
│       │   │   ├── auth.ts
│       │   │   ├── users.ts
│       │   │   ├── companies.ts
│       │   │   └── customers.ts
│       │   ├── validations/
│       │   │   ├── company.ts
│       │   │   └── user.ts
│       │   └── hooks/
│       │       ├── use-locale.ts
│       │       └── use-session.ts
│       ├── contexts/
│       │   ├── auth-context.tsx
│       │   └── sidebar-context.tsx
│       └── auth.ts                  # Provider "credentials" (multi-tenant)
│
├── Admin/                           # Dominio Admin (gestión global)
│   ├── Api/                         # API Admin
│   │   ├── Controllers/
│   │   │   ├── AdminAuthController.cs
│   │   │   ├── DashboardController.cs
│   │   │   └── LogController.cs
│   │   ├── Application/
│   │   │   └── DTOs/
│   │   │       ├── Auth/
│   │   │       │   ├── AdminLoginRequest.cs
│   │   │       │   └── AdminLoginResponse.cs
│   │   │       └── DashboardSummaryDto.cs
│   │   ├── Domain/
│   │   │   └── Entities/
│   │   │       ├── AdminUser.cs
│   │   │       └── AuditLog.cs
│   │   └── Infrastructure/
│   │       └── Services/
│   │           ├── AdminAuthService.cs
│   │           ├── AdminJwtService.cs
│   │           └── AuditLogService.cs
│   │
│   └── Frontend/                    # Frontend Admin
│       ├── app/
│       │   └── (admin)/
│       │       ├── layout.tsx
│       │       ├── admin/
│       │       │   ├── login/
│       │       │   │   └── page.tsx
│       │       │   └── dashboard/
│       │       │       └── page.tsx
│       │       └── logs/             # Si LogController es Admin
│       │           └── page.tsx
│       ├── components/
│       │   └── layout/
│       │       └── admin-layout.tsx
│       └── auth.ts                  # Provider "admin" (identidad global)
│
└── Utils/                           # Consola global y Seeds
    ├── Console/                     # GesFer.Console migrado
    │   ├── Program.cs
    │   └── Services/
    │       ├── DatabaseInitializationService.cs
    │       ├── DockerService.cs
    │       ├── GoldenRulesComplianceService.cs
    │       ├── IntegrityValidationService.cs
    │       ├── LogService.cs
    │       ├── MenuService.cs
    │       ├── MigrationService.cs
    │       └── SeedService.cs
    ├── Services/                    # Servicios de seeding
    │   ├── JsonDataSeeder.cs
    │   └── MasterDataSeeder.cs
    ├── Data/
    │   ├── DbInitializer.cs
    │   └── Seeds/                    # Taxonomía de Seeds (ver sección 3)
    │       ├── master/
    │       │   ├── master-data.json
    │       │   ├── admin-master-data.json
    │       │   └── product-master-data.json
    │       ├── demo/
    │       │   ├── demo-data.json
    │       │   ├── admin-demo-data.json
    │       │   └── product-demo-data.json
    │       └── test/
    │           ├── test-data.json
    │           ├── admin-test-data.json
    │           └── product-test-data.json
    └── SeedRunner/                  # Proyecto separado para ejecutar seeds
        └── Program.cs
```

---

## 3. TAXONOMÍA DE SEEDS - 3 NIVELES POR ÁMBITO

### 3.1 Estructura de Seeds

Cada nivel (Master, Demo, Test) se divide por ámbito:

#### Master (Datos Maestros del Sistema)
- `master/master-data.json`: Datos maestros compartidos (Languages, Countries, States, Cities, PostalCodes)
- `master/admin-master-data.json`: Datos maestros Admin (AdminUser base, permisos Admin)
- `master/product-master-data.json`: Datos maestros Product (Permissions, Groups base, configuración multi-tenant)

#### Demo (Datos de Demostración)
- `demo/demo-data.json`: Datos demo compartidos (geografía de ejemplo)
- `demo/admin-demo-data.json`: Datos demo Admin (usuarios Admin de ejemplo, logs de auditoría)
- `demo/product-demo-data.json`: Datos demo Product (Companies, Users, Customers, Suppliers, Articles, etc.)

#### Test (Datos de Prueba para Tests)
- `test/test-data.json`: Datos test compartidos (geografía para tests)
- `test/admin-test-data.json`: Datos test Admin (AdminUsers con IDs fijos, logs de prueba)
- `test/product-test-data.json`: Datos test Product (Companies/Users/Customers con IDs fijos para tests determinísticos)

### 3.2 Orquestación de Seeds

El `JsonDataSeeder` en `src/Utils/Services/JsonDataSeeder.cs` debe:

1. **Cargar Seeds Compartidos primero** (Shared)
   - `master/master-data.json` → Languages, Countries, States, Cities, PostalCodes
   
2. **Cargar Seeds Admin** (si el ámbito incluye Admin)
   - `master/admin-master-data.json` → AdminUser base
   - `demo/admin-demo-data.json` → AdminUsers demo
   - `test/admin-test-data.json` → AdminUsers test
   
3. **Cargar Seeds Product** (si el ámbito incluye Product)
   - `master/product-master-data.json` → Permissions, Groups base
   - `demo/product-demo-data.json` → Companies, Users, Customers, Suppliers, Articles
   - `test/product-test-data.json` → Datos con IDs fijos para tests

### 3.3 Flags de Orquestación

El `DbInitializer` debe aceptar flags para controlar qué seeds cargar:

```csharp
public class SeedOptions
{
    public bool IncludeAdmin { get; set; } = false;
    public bool IncludeProduct { get; set; } = false;
    public SeedLevel Level { get; set; } = SeedLevel.Master; // Master, Demo, Test
}

public enum SeedLevel
{
    Master,
    Demo,
    Test
}
```

---

## 4. DEPENDENCIAS CRÍTICAS Y REGLAS DE ORO

### 4.1 Invariantes de Dependencia

#### Shared es Sagrado
- ✅ **Shared NO puede depender de Product ni Admin**
- ✅ **Product y Admin pueden depender de Shared**
- ✅ **Product y Admin NO pueden depender entre sí**

#### Nomenclatura
- ✅ Todo lo relativo a "empresa" se codifica como **company** (no "empresa")
- ✅ Rutas Admin: `/admin/*`
- ✅ Rutas Product: `/[locale]/*` o `/(client)/*`

#### Dualidad Backend ↔ Frontend
- ✅ Cada movimiento en Backend debe tener correspondencia en Frontend
- ✅ DTOs Admin en Backend → Tipos TypeScript Admin en Frontend
- ✅ DTOs Product en Backend → Tipos TypeScript Product en Frontend

### 4.2 ApplicationDbContext - Estrategia de División

**OPCIÓN A: DbContext Único Compartido (Recomendada para Fase 1)**
- `src/Shared/Api/Infrastructure/Data/ApplicationDbContext.cs`
- Contiene todos los `DbSet<T>` de Shared, Product y Admin
- Product y Admin referencian este DbContext compartido
- **Ventaja**: Migración más simple, sin cambios en EF Core Migrations
- **Desventaja**: Acoplamiento físico (pero no lógico)

**OPCIÓN B: DbContexts Separados (Futuro)**
- `src/Shared/Api/Infrastructure/Data/SharedDbContext.cs`
- `src/Product/Api/Infrastructure/Data/ProductDbContext.cs`
- `src/Admin/Api/Infrastructure/Data/AdminDbContext.cs`
- **Ventaja**: Separación total
- **Desventaja**: Requiere migración compleja de EF Core y posiblemente múltiples bases de datos

**RECOMENDACIÓN FASE 1**: Opción A (DbContext único compartido en Shared)

### 4.3 Componentes Pendientes de Análisis

Los siguientes componentes requieren decisión arquitectónica antes de la migración:

1. **LogController y AuditLog**: ¿Admin exclusivo o Shared?
   - **Recomendación**: Admin exclusivo (logs de auditoría son gestión global)

2. **SetupController**: ¿Admin o Product?
   - **Recomendación**: Admin (inicialización del sistema es gestión global)

3. **Permission/UserPermission/UserGroup/GroupPermission**: ¿Shared o Product?
   - **Recomendación**: Product (pertenecen al dominio multi-tenant)

4. **StockService**: ¿Shared o Product?
   - **Recomendación**: Product (gestión de stock es dominio de negocio)

5. **id-validation.ts**: ¿Shared o Product?
   - **Recomendación**: Shared (utilidad neutral de validación)

6. **sidebar-context.tsx y auth-context.tsx**: ¿Shared o Product?
   - **Recomendación**: Product (contextos específicos del dominio multi-tenant)

---

## 5. HITOS DE COMPILACIÓN - S+ GRADE

### 5.1 Hitos de Compilación por Fase

#### FASE 1: Estructura Base (Sin Migración de Código)
- ✅ Crear estructura de directorios `./src`
- ✅ Crear proyectos .csproj y package.json base
- ✅ Configurar referencias entre proyectos
- ✅ **Hito**: Compilación exitosa de estructura vacía

#### FASE 2: Migración Shared
- ✅ Migrar ValueObjects (Email, TaxId)
- ✅ Migrar BaseEntity
- ✅ Migrar entidades geográficas (Country, State, City, PostalCode, Language)
- ✅ Migrar componentes UI compartidos
- ✅ **Hito**: Compilación exitosa de Shared (0 errores, 0 warnings)

#### FASE 3: Migración Product
- ✅ Migrar controladores Product
- ✅ Migrar DTOs Product
- ✅ Migrar entidades Product
- ✅ Migrar servicios Product
- ✅ Migrar frontend Product
- ✅ **Hito**: Compilación exitosa de Product (0 errores, 0 warnings)

#### FASE 4: Migración Admin
- ✅ Migrar controladores Admin
- ✅ Migrar DTOs Admin
- ✅ Migrar entidades Admin
- ✅ Migrar servicios Admin
- ✅ Migrar frontend Admin
- ✅ **Hito**: Compilación exitosa de Admin (0 errores, 0 warnings)

#### FASE 5: Migración Utils y Seeds
- ✅ Migrar consola a `src/Utils/Console`
- ✅ Migrar servicios de seeding
- ✅ Reorganizar seeds según taxonomía
- ✅ **Hito**: Compilación exitosa de Utils (0 errores, 0 warnings)

#### FASE 6: Integración y Tests
- ✅ Actualizar tests de integración
- ✅ Verificar que todos los tests pasan
- ✅ **Hito**: Todos los tests pasando (100% green)

### 5.2 Criterios de Éxito S+ Grade

1. **Compilación Limpia**: 0 errores, 0 warnings en todos los proyectos
2. **Tests Verdes**: 100% de tests pasando
3. **Sin Dependencias Circulares**: Verificación de referencias entre proyectos
4. **Shared Sagrado**: Verificación de que Shared no depende de Product ni Admin
5. **Nomenclatura**: Verificación de que "empresa" → "company" en todo el código
6. **Dualidad**: Verificación de correspondencia Backend ↔ Frontend

---

## 6. PLAN DE ACCIÓN RECOMENDADO

### Orden de Ejecución

1. **Crear estructura `./src`** (sin código, solo directorios y proyectos base)
2. **Migrar Shared** (base sólida)
3. **Migrar Product** (dominio principal)
4. **Migrar Admin** (dominio secundario)
5. **Migrar Utils** (orquestación)
6. **Integración y validación**

### Riesgos Identificados

1. **ApplicationDbContext**: Decisión sobre estrategia de división
2. **Componentes pendientes**: Requieren análisis adicional
3. **Tests de integración**: Requieren actualización de rutas y referencias
4. **Docker y scripts**: Requieren actualización de rutas

---

## 7. PRÓXIMOS PASOS

1. **Revisión de este análisis** por el equipo
2. **Decisión sobre componentes pendientes** (LogController, SetupController, etc.)
3. **Decisión sobre ApplicationDbContext** (Opción A vs Opción B)
4. **Creación de rama**: `feat/domain-separation`
5. **Inicio de FASE 1**: Creación de estructura base

---

**FIN DEL ANÁLISIS INICIAL**
