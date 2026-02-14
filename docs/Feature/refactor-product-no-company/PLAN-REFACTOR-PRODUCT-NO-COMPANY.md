# Plan de implementación: Refactor Product sin Company

**SPEC:** SPEC-REFACTOR-PRODUCT-NO-COMPANY  
**Clarificaciones:** REFACTOR-PRODUCT-NO-COMPANY_CLARIFICATIONS.md  
**Rama sugerida:** refactor/product-no-company

---

## Resumen

Este plan ordena las tareas para eliminar la dependencia de Product sobre Company. Product consumirá exclusivamente `AdminCompanyDto` desde Admin API. La tabla Companies es propiedad de Admin; Product mantiene `CompanyId` como FK en sus entidades sin navegación a Company.

---

## Fase 1: Dominio e Infraestructura Product

### 1.1 Eliminar entidad Company de Product
- [ ] Eliminar `src/Product/Back/domain/Entities/Company.cs` (clase que hereda de Shared.Company)
- [ ] Verificar que ninguna otra clase de Product referencia `Product.Back.Domain.Entities.Company`
- **Entregable:** Dominio Product sin entidad Company

### 1.2 Ajustar ApplicationDbContext y configuraciones
- [ ] Confirmar que `ApplicationDbContext` **no** tiene `DbSet<Company>` (si existe, eliminarlo)
- [ ] Eliminar `src/Product/Back/Infrastructure/Data/Configurations/CompanyConfiguration.cs`
- [ ] Ajustar configuraciones que referencian Company:
  - [ ] `ArticleConfiguration`: eliminar relación HasOne(Company); mantener CompanyId como FK
  - [ ] `UserConfiguration`: eliminar navegación Company
  - [ ] `SupplierConfiguration`, `CustomerConfiguration`, `TariffConfiguration`: eliminar navegación Company
  - [ ] `ArticleFamilyConfiguration`, `TaxTypeConfiguration`: eliminar navegación Company si existe
- **Entregable:** ApplicationDbContext sin Company; configuraciones sin navegación Company

### 1.3 Eliminar navegación Company en entidades
- [ ] `Article`: quitar `public Company Company { get; set; }`; mantener `CompanyId`
- [ ] `User`, `Supplier`, `Customer`, `Tariff`, `ArticleFamily`, `TaxType`: quitar navegación Company donde exista
- **Entregable:** Entidades con CompanyId; sin navegación Company

---

## Fase 2: Aplicación (Commands / Handlers / Controllers)

### 2.1 Eliminar Company CRUD en Product
- [ ] Eliminar `CompanyController` (si existe en `src/Product/Back/Api/Controllers/`)
- [ ] Eliminar handlers: `GetAllCompaniesCommandHandler`, `GetCompanyByIdCommandHandler`, `CreateCompanyCommandHandler`, `UpdateCompanyCommandHandler`, `DeleteCompanyCommandHandler`
- [ ] Eliminar commands: `GetAllCompaniesCommand`, `GetCompanyByIdCommand`, `CreateCompanyCommand`, `UpdateCompanyCommand`, `DeleteCompanyCommand`
- [ ] Eliminar `CompanyDto` de Application (si existe en `application/DTOs/Company/`)
- [ ] Desregistrar handlers y rutas en DI/Program
- **Entregable:** Product sin CRUD de companies

### 2.2 Handlers que usan _context.Companies
- [ ] Identificar handlers que validan o consultan Company (ej. LoginCommandHandler, handlers que incluyen .Include(c => c.Company))
- [ ] Reemplazar `_context.Companies` por `IAdminApiClient.GetCompanyAsync(companyId)` cuando se necesite validar que la empresa existe
- [ ] Inyectar `IAdminApiClient` donde sea necesario
- **Entregable:** Sin accesos a _context.Companies

### 2.3 Verificar MyCompanyController
- [ ] Confirmar que MyCompanyController usa exclusivamente `IAdminApiClient` + `AdminCompanyDto`
- [ ] No debe usar ApplicationDbContext para Company
- **Entregable:** MyCompanyController correcto (GET/PUT proxy a Admin)

---

## Fase 3: Autenticación y Login

### 3.1 Validación de empresa en Login
- [ ] LoginCommandHandler: antes de emitir JWT, validar que la empresa existe vía `IAdminApiClient.GetCompanyAsync(companyId)`
- [ ] Si Admin API no disponible: definir comportamiento (timeout, reintentos, rechazar login o modo degradado)
- [ ] Documentar en clarificaciones o código
- **Entregable:** Login valida empresa en Admin antes de emitir token

### 3.2 Token JWT
- [ ] Confirmar que JwtService incluye claim `company_id` en el token
- [ ] Verificar que MyCompanyController y filtros tenant usan `company_id` del token
- **Entregable:** Token con company_id; flujo verificado

---

## Fase 4: Seeds y datos iniciales

### 4.1 JsonDataSeeder / DbInitializer
- [ ] Eliminar inserción de Companies en seeds de Product (JsonDataSeeder, DbInitializer)
- [ ] Los datos demo (Users, Articles, ArticleFamilies, etc.) referencian CompanyIds existentes (seeds de Admin)
- [ ] Verificar que los CompanyIds usados en seeds coinciden con los de AdminJsonDataSeeder
- **Entregable:** Product no inserta Companies; seeds referencian IDs de Admin

### 4.2 InitDatabase / scripts
- [ ] Revisar `InitDatabase.cs`, `validate-seed-data.ps1`: eliminar referencias a tabla Companies si Product las gestionaba
- **Entregable:** Scripts sin inserción de Companies

---

## Fase 5: Frontend Product

### 5.1 Eliminar o migrar rutas /companies
- [ ] Eliminar o restringir `companiesApi` (GET all, POST, PUT, DELETE) en `lib/api/companies.ts`
- [ ] Rutas `/companies` y `/companies/[id]`: eliminar páginas o migrar a Admin Front
- [ ] Evaluar: si existen en Product Front, eliminarlas (Product no gestiona lista de empresas)
- **Entregable:** Frontend Product sin CRUD de companies

### 5.2 Mantener Mi Organización
- [ ] Mantener página `/my-company` (Mi Organización) que consume `/api/my-company`
- [ ] Verificar que usa el tipo/contrato correcto (AdminCompanyDto / Company)
- **Entregable:** Mi Organización funcional

---

## Fase 6: Migraciones (evaluación)

### 6.1 Tabla Companies
- [ ] Verificar quién crea la tabla Companies: Admin vs Product migraciones
- [ ] Si Product tiene migración que crea Companies: no modificar en este refactor; la tabla existe y Admin es dueño. Evaluar en iteración futura si mover creación a Admin.
- [ ] Product mantiene `CompanyId` en entidades; la tabla Companies existe (creada por Admin). No crear nueva migración en Product que altere Companies.
- **Entregable:** Sin cambios en migraciones Product que afecten Companies; estado documentado

---

## Fase 7: Tests

### 7.1 Eliminar tests de Company CRUD
- [ ] Eliminar `CompanyCommandTests`, `CompanyControllerTests` (unit e integración)
- [ ] Eliminar referencias en `IntegrationTestWebAppFactory`, `DatabaseFixture` si aplica
- **Entregable:** Tests de Company CRUD eliminados

### 7.2 Mantener y ajustar MyCompanyControllerTests
- [ ] Mantener `MyCompanyControllerTests` (usa MockAdminApiClient)
- [ ] Verificar que pasan tras cambios
- **Entregable:** MyCompanyControllerTests en verde

### 7.3 Ajustar tests que usan Company
- [ ] AuthControllerTests, UserControllerTests, etc.: reemplazar seeds o mocks que usen _context.Companies
- [ ] Handlers que validan Company: usar MockAdminApiClient o similar
- **Entregable:** Tests actualizados; build y tests en verde

### 7.4 E2E / Frontend tests
- [ ] Eliminar o ajustar tests que usan `companiesApi` (listado, CRUD)
- [ ] Mantener tests de `/my-company` si existen
- **Entregable:** Tests E2E/Frontend coherentes con nuevo modelo

---

## Orden de ejecución sugerido

| Orden | Fase | Dependencias |
|-------|------|--------------|
| 1 | Fase 1 – Dominio e Infraestructura | Ninguna |
| 2 | Fase 2 – Aplicación (eliminar CRUD) | Fase 1 |
| 3 | Fase 4 – Seeds | Fase 1 (evitar inserts de Company) |
| 4 | Fase 3 – Login | Fase 2 (IAdminApiClient ya usado) |
| 5 | Fase 5 – Frontend | Fase 2 |
| 6 | Fase 6 – Migraciones | Verificación; puede hacerse en paralelo |
| 7 | Fase 7 – Tests | Fases 1–5 |

---

## Criterios de cierre del plan

- [ ] Todas las tareas con checkbox cumplido o justificado
- [ ] Product compila sin referencias a entidad Company ni DbSet Companies
- [ ] MyCompanyController funciona (GET/PUT delegados a Admin)
- [ ] Login valida empresa en Admin y emite token con company_id
- [ ] Seeds de Product no insertan Companies
- [ ] Tests en verde (MyCompanyControllerTests, AuthControllerTests, etc.)
- [ ] Frontend sin CRUD de companies; Mi Organización funcional
- [ ] Documentación actualizada (SPEC, Clarificaciones, Plan)

---

## Trazabilidad

- **SPEC:** docs/Feature/refactor-product-no-company/SPEC-REFACTOR-PRODUCT-NO-COMPANY.md
- **Clarificaciones:** docs/Feature/refactor-product-no-company/REFACTOR-PRODUCT-NO-COMPANY_CLARIFICATIONS.md
- **Objetivos:** docs/Feature/refactor-product-no-company/REFACTOR-PRODUCT-NO-COMPANY-PHASE-OBJECTIVES.md
- **Base:** SPEC-COMPANY-MANAGED-BY-ADMIN
- **Deuda:** docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md
