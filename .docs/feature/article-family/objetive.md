# Plantilla: Creación de Maestro (Master Data)

**Referencia:** Tarea: Mantenimiento de Familia de Artículo
**Tipo:** Maestro CRUD

## 1. Definición de Entidad
*   **Nombre:** `ArticleFamily`
*   **Tabla BD:** `ArticleFamilies`
*   **Contexto:** Product (Requiere `CompanyId` obligatorio)
*   **Propiedades:**
    *   `Id` (Guid, PK)
    *   `CompanyId` (Guid, FK) - **Obligatorio** (Tenancy)
    *   `Code` (String, MaxLength 50) - Único por Compañía
    *   `Name` (String, MaxLength 100) - Nombre de la familia (ej. "Metales")
    *   `Description` (String, MaxLength 500, Nullable) - Descripción opcional
    *   `TaxTypeId` (Guid, FK) - Relación con `TaxType` (Tipo de Tasa)

## 2. Checklist de Implementación

### Backend
- [ ] **Domain:** Crear Entidad `ArticleFamily` en `src/Product/Back/domain/Entities/`. Asegurar herencia de `BaseEntity` y propiedad `CompanyId`.
- [ ] **Persistence:** Crear Configuration `ArticleFamilyConfiguration` en `src/Product/Back/Infrastructure/Persistence/Configurations/`.
- [ ] **DbContext:** Añadir `DbSet<ArticleFamily>` en `ApplicationDbContext`.
- [ ] **Migration:** Generar y revisar migración (asegurar índices para `CompanyId` y `Code`).
- [ ] **DTOs:** Crear DTOs en `src/Product/Back/application/DTOs/ArticleFamilies/`:
    - `CreateArticleFamilyDto`
    - `UpdateArticleFamilyDto`
    - `ArticleFamilyDto` (Read)
- [ ] **Commands:** Implementar CQRS en `src/Product/Back/application/Commands/ArticleFamilies/`:
    - `CreateArticleFamilyCommand` + Validator
    - `UpdateArticleFamilyCommand` + Validator
    - `DeleteArticleFamilyCommand`
- [ ] **Queries:** Implementar en `src/Product/Back/application/Queries/ArticleFamilies/`:
    - `GetArticleFamiliesQuery`
    - `GetArticleFamilyByIdQuery`
- [ ] **Controller:** Crear `ArticleFamiliesController` en `src/Product/Back/Api/Controllers/`.
- [ ] **Tests:**
    - Unit Tests: `src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/`
    - Integration Tests: `src/Product/Back/tests/GesFer.Product.IntegrationTests/Controllers/ArticleFamiliesControllerTests.cs`
- [ ] **Seeds:** Añadir datos demo en `src/Product/Back/Infrastructure/Persistence/Seed/demo-data.json` y actualizar `ProductJsonDataSeeder`.

### Frontend
- [ ] **Types:** Definir interfaz `ArticleFamily` en `src/Product/Front/lib/types/article-family.ts`.
- [ ] **API:** Crear servicio `articleFamiliesApi` en `src/Product/Front/lib/api/article-families.ts`.
- [ ] **I18n:** Añadir traducciones para "Familia de Artículo", propiedades y errores en `src/Product/Front/locales/{es,en,ca}/translation.json`.
- [ ] **Page:** Crear página de listado en `src/Product/Front/app/[locale]/(app)/maestros/familias-articulos/page.tsx`.
- [ ] **Components:**
    - `ArticleFamilyTable` (Listado con acciones)
    - `ArticleFamilyForm` (Crear/Editar con selector de `TaxType`)
- [ ] **Menu:** Añadir entrada "Familias de Artículos" en `Sidebar.tsx` bajo la sección "Maestros".

## 3. Verificación
- [ ] Build Backend (`dotnet build src/Product/Back/GesFer.Product.sln`)
- [ ] Tests Backend (`dotnet test src/Product/Back/GesFer.Product.sln`)
- [ ] Build Frontend (`npm run build --prefix src/Product/Front`)
- [ ] Navegación UI correcta a `/maestros/familias-articulos`
- [ ] CRUD funcional:
    - Crear: Validar unicidad de Código y selección de Tasa.
    - Leer: Listar filtrando por CompanyId (automático por contexto).
    - Editar: Modificar nombre/descripción/tasa.
    - Borrar: Soft delete (si aplica) o borrado físico.
