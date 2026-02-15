# REFACTOR: Product sin dependencia de Company – Fase de Objetivos

> **Contexto:** Se ha eliminado la dependencia de Product sobre Company (dominio Admin). La rotura requiere refactor. Product solo cuenta con UNA empresa: la del usuario actual, obtenida desde Admin API.

**Estado:** Fase de Objetivos  
**Rama sugerida:** `refactor/product-no-company`  
**Base:** SPEC-COMPANY-MANAGED-BY-ADMIN

---

## 1. Objetivo general

Que **Product** no dependa de la entidad Company ni de la tabla Companies. Product consume exclusivamente el **DTO/Modelo de empresa** obtenido desde Admin API vía `IAdminApiClient.AdminCompanyDto`.

---

## 2. Principios

| Principio | Descripción |
|-----------|-------------|
| **Una sola empresa** | En Product solo existe la empresa del usuario actual (CompanyId del token). No hay lista de empresas. |
| **DTO desde Admin** | Product usa `AdminCompanyDto` (o modelo equivalente) obtenido por API. No entidad propia. |
| **CompanyId como FK** | Las entidades Product (User, Article, Supplier, etc.) mantienen `CompanyId` para filtrado tenant, pero **sin** navegación a entidad Company. |
| **Admin es dueño** | La tabla `Companies` se crea y gestiona en Admin. Product no la define ni la migra. |

---

## 3. Objetivos por capa

### 3.1 Dominio Product

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.1 | Eliminar `Product.Back.Domain.Entities.Company` | Quitar clase que hereda de Shared.Company |
| O.2 | Mantener `CompanyId` en entidades Product | User, Article, ArticleFamily, TaxType, Tariff, Supplier, Customer, etc. conservan `CompanyId` como scalar |
| O.3 | Eliminar navegación `Company` en entidades | Quitar `public Company Company { get; set; }` donde exista; no incluir `.Include(c => c.Company)` |

### 3.2 Infraestructura Product

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.4 | Eliminar `DbSet<Company>` de ApplicationDbContext | No hay Companies en Product DbContext |
| O.5 | Eliminar `CompanyConfiguration` | Borrar `Configurations/CompanyConfiguration.cs` |
| O.6 | Ajustar configuraciones que referencian Company | ArticleConfiguration, UserConfiguration, SupplierConfiguration, CustomerConfiguration, TariffConfiguration: relaciones con Companies → eliminar o cambiar a FK sin navegación |
| O.7 | Migraciones: quitar tabla Companies de Product | La tabla Companies es de Admin. Product no la crea. Si está en migraciones Product, mover/eliminar según plan de migraciones. |

### 3.3 Aplicación (Commands / Handlers)

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.8 | Eliminar handlers de Company CRUD | `GetAllCompaniesCommandHandler`, `GetCompanyByIdCommandHandler`, `CreateCompanyCommandHandler`, `UpdateCompanyCommandHandler`, `DeleteCompanyCommandHandler` |
| O.9 | Eliminar commands de Company CRUD | Commands asociados a los handlers anteriores |
| O.10 | Mantener MyCompanyController | Usa `IAdminApiClient` + `AdminCompanyDto`. Es el único punto de acceso a datos de empresa en Product. |
| O.11 | Handlers que usan Company | Reemplazar `_context.Companies` por validación vía `IAdminApiClient.GetCompanyAsync(companyId)` cuando se necesite validar que la empresa existe |

### 3.4 API / Controllers

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.12 | Eliminar `CompanyController` | Si existe en Product, eliminarlo (Company CRUD es de Admin) |
| O.13 | Mantener `MyCompanyController` | GET/PUT `/api/my-company` como proxy a Admin |
| O.14 | Eliminar rutas `/api/company` (CRUD) | Product no expone CRUD de companies |

### 3.5 DTOs

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.15 | Usar `AdminCompanyDto` como modelo de empresa | Ya existe en `Infrastructure/DTOs/AdminCompanyDto.cs` |
| O.16 | Eliminar `CompanyDto` de Application | Si Product tenía `CompanyDto` propio para CRUD, eliminarlo o reemplazar por alias a AdminCompanyDto |
| O.17 | Respuesta de MyCompany | Devolver `AdminCompanyDto` (ya implementado) |

### 3.6 Seeds / Datos iniciales

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.18 | Product no inserta Companies | JsonDataSeeder, DbInitializer: no crear ni insertar companies |
| O.19 | Seeds referencian CompanyIds existentes | Los datos demo (Users, Articles, etc.) usan CompanyIds que deben existir en Admin (seeds de Admin) |

### 3.7 Autenticación / Login

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.20 | Login valida empresa en Admin | Antes de emitir token, validar que la empresa existe vía Admin API |
| O.21 | Token incluye `company_id` | JWT con claim `company_id` para MyCompanyController y filtros tenant |

### 3.8 Frontend Product

| Objetivo | Estado | Acción |
|----------|--------|--------|
| O.22 | Eliminar `companiesApi` CRUD | Si existe API de companies (GET all, POST, PUT, DELETE), eliminarla o restringirla |
| O.23 | Mantener `my-company` / Mi Organización | Página que consume `/api/my-company` (proxy a Admin) |
| O.24 | Rutas `/companies` y `/companies/[id]` | Evaluar: ¿pertenecen a Admin? Si están en Product Front, migrar a Admin o eliminarlas |

---

## 4. Dependencias externas

| Dependencia | Rol |
|-------------|-----|
| `IAdminApiClient` | Obtener y actualizar empresa vía Admin API |
| `AdminCompanyDto` / `AdminUpdateCompanyDto` | Modelo de empresa en Product |
| Admin API: `GET /api/company/{id}`, `PUT /api/company/{id}` | Fuente de verdad de Company |

---

## 5. Criterios de éxito

- [ ] Product compila sin referencias a entidad Company ni DbSet Companies
- [ ] MyCompanyController funciona (GET/PUT delegados a Admin)
- [ ] Login emite token con `company_id` y valida empresa en Admin
- [ ] Entidades Product tienen `CompanyId` pero sin navegación a Company
- [ ] Seeds de Product no insertan Companies; referencian IDs de Admin
- [ ] Tests actualizados o eliminados según handlers/controllers eliminados
- [ ] Sin dependencia Product → Shared.Company (opcional: evaluar si Shared debe mantener Company para Admin)

---

## 6. Fases siguientes (tras objetivos)

1. **Fase Análisis de impacto** – Listar archivos afectados, tests rotos, rutas Front
2. **Fase Plan de migración** – Orden de cambios, migraciones BD si aplica
3. **Fase Implementación** – Ejecución según plan
4. **Fase Validación** – Tests, smoke tests, revisión manual
