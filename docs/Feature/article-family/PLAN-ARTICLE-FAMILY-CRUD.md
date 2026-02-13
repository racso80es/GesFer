# Plan de implementación: CRUD ArticleFamily y reemplazo de Family

**SPEC:** [SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md](SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md)  
**Clarificaciones:** [SPEC-ARTICLE-FAMILY-CRUD_CLARIFICATIONS-PROPUESTA.md](SPEC-ARTICLE-FAMILY-CRUD_CLARIFICATIONS-PROPUESTA.md)  
**Rama:** feat-spec-article-family-*

Este plan incluye el **reemplazo completo de la entidad Family** por ArticleFamily: migración de Article, eliminación de Family y limpieza de referencias.

---

## Fase 0: Preparación (orden de ejecución)

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 0.1 | Asegurar TaxTypes en demo | En `demo-data.json` existir clave `taxTypes` con impuestos España (IVA21, IVA10, IVA4, EXENTO) y `companyId` coherente. Si no existe, añadir. | — |
| 0.2 | Revisar Companies | Companies deben existir antes que TaxTypes/ArticleFamilies (se encarga negocio de Admin). Verificar orden. | — |

---

## Fase 1: Backend – ArticleFamily (nuevo maestro)

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 1.1 | Entidad ArticleFamily | Crear `src/Product/Back/domain/Entities/ArticleFamily.cs`: BaseEntity, CompanyId, Code, Name, Description?, TaxTypeId; navegación Company, TaxType. | — |
| 1.2 | Configuración EF | Crear `ArticleFamilyConfiguration`: tabla `ArticleFamilies`, índices (CompanyId, Code único), FK TaxType, check si aplica. | 1.1 |
| 1.3 | DbContext y migración (tabla nueva) | Añadir `DbSet<ArticleFamily>` en `ApplicationDbContext`. Generar migración que **solo cree** tabla `ArticleFamilies` (sin tocar Article/Families aún). Aplicar. | 1.2 |
| 1.4 | DTOs ArticleFamily | Crear en `application/DTOs/ArticleFamilies/`: `ArticleFamilyDto`, `CreateArticleFamilyDto`, `UpdateArticleFamilyDto`. | — |
| 1.5 | Commands ArticleFamily | Crear en `application/Commands/ArticleFamilies/`: Create, Update, Delete con validadores FluentValidation. | 1.4 |
| 1.6 | Queries ArticleFamily | Crear en `application/Queries/ArticleFamilies/`: GetArticleFamiliesQuery, GetArticleFamilyByIdQuery. | 1.4 |
| 1.7 | Handlers ArticleFamily | Crear en `application/Handlers/ArticleFamilies/`: Create, Update, Delete, GetArticleFamilies, GetArticleFamilyById. Filtro por CompanyId. | 1.5, 1.6 |
| 1.8 | Controller ArticleFamilies | Crear `ArticleFamiliesController`: CRUD, permisos Consultar/Gestionar, retornos Result/HTTP. | 1.7 |
| 1.9 | Permisos | Registrar permisos Consultar y Gestionar para ArticleFamily (master-data o configuración de permisos según proyecto). | — |
| 1.10 | Tests unitarios ArticleFamily | Tests para handlers/validators en `GesFer.Product.UnitTests/ArticleFamilies/`. | 1.7 |
| 1.11 | Tests integración ArticleFamiliesController | `ArticleFamiliesControllerTests.cs`: list, get by id, create, update, delete (soft). | 1.8 |

---

## Fase 2: Backend – Migración Article y reemplazo de Family

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 2.1 | Añadir ArticleFamilyId a Article | En `Article.cs`: añadir propiedad `ArticleFamilyId` (Guid) y navegación `ArticleFamily`. Mantener temporalmente `FamilyId` y `Family` para migración. | 1.1 |
| 2.2 | Configuración Article (doble FK temporal) | En `ArticleConfiguration`: añadir relación HasOne ArticleFamily (FK ArticleFamilyId). Generar migración que añada columna `ArticleFamilyId` (nullable). | 2.1, 1.2 |
| 2.3 | Migración de datos Family → ArticleFamily | No es necesario. Estamos en fase de desarroyo, y los datos serán reemplazdos por lo indicado en las seeds.
| 2.4 | Quitar FamilyId de Article | Quitar `FamilyId` y navegación `Family` de `Article.cs`. Hacer `ArticleFamilyId` obligatorio. | 2.3 |
| 2.5 | Nueva migración: quitar FamilyId y tabla Families | Migración que: elimina FK y columna `FamilyId` de Articles; elimina tabla `Families`; elimina índices/FK relacionados. | 2.4 |
| 2.6 | ArticleConfiguration sin Family | En `ArticleConfiguration`: eliminar HasOne Family; dejar solo HasOne ArticleFamily. | 2.4 |
| 2.7 | Eliminar entidad Family | Borrar `src/Product/Back/domain/Entities/Family.cs`. | 2.5 |
| 2.8 | Eliminar FamilyConfiguration | Borrar `Infrastructure/Data/Configurations/FamilyConfiguration.cs`. | 2.7 |
| 2.9 | DbContext sin Families | En `ApplicationDbContext`: quitar `DbSet<Family> Families`. | 2.8 |
| 2.10 | Company sin Families | En `Company.cs`: quitar colección `Families`. | 2.7 |
| 2.11 | Albaranes: usar ArticleFamily.TaxType | En `CreateSalesDeliveryNoteCommandHandler` y `CreatePurchaseDeliveryNoteCommandHandler`: cambiar `.Include(a => a.Family)` por `.Include(a => a.ArticleFamily).ThenInclude(af => af.TaxType)`; cambiar `article.Family.IvaPercentage` por `article.ArticleFamily.TaxType.Value`. | 2.4 |
| 2.12 | InitDatabase / scripts | En `InitDatabase.cs` (o scripts que listen tablas): sustituir "Families" por "ArticleFamilies" donde corresponda. | 2.9 |

---

## Fase 3: Seeds – ArticleFamilies y limpieza de Family

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 3.1 | DTO ArticleFamilySeed | En `JsonDataSeeder`: clase `ArticleFamilySeed` (Id, CompanyId, Code, Name, Description, TaxTypeId). Añadir en `DemoDataSeed` propiedad `List<ArticleFamilySeed>? ArticleFamilies`. | 1.1 |
| 3.2 | SeedArticleFamiliesAsync | Implementar `SeedArticleFamiliesAsync`; validar Guids, CompanyId, TaxTypeId existente; insertar/actualizar por Id. Llamar tras `SeedTaxTypesAsync` en `SeedDemoDataAsync`. | 3.1 |
| 3.3 | demo-data.json: articleFamilies | Añadir clave `articleFamilies` con datos de ejemplo (Metales, Plásticos, Consumibles) referenciando `taxTypes` por id. Mismo companyId que TaxTypes. | 0.1 |
| 3.4 | Eliminar seeds de Family | Quitar de `DemoDataSeed`: `Families`. Quitar `SeedFamiliesAsync` y su llamada. Quitar clase `FamilySeed`. | 2.9 |
| 3.5 | Articles en demo: articleFamilyId | En `ArticleSeed` (o equivalente): añadir `ArticleFamilyId`; en ítems de `articles` en demo-data.json usar `articleFamilyId` en lugar de `familyId`. Eliminar `familyId` de ArticleSeed y de JSON. | 3.2, 3.3 |
| 3.6 | SeedArticlesAsync con ArticleFamilyId | En `SeedArticlesAsync`: leer `ArticleFamilyId` (o mapear desde articleFamilyId); asignar a Article. Dejar de usar FamilyId. | 3.5 |
| 3.7 | Eliminar clave families de demo-data | Quitar del JSON de demo la clave `families` si existe. | 3.4 |

---

## Fase 4: Frontend – Familias de Artículos

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 4.1 | Tipos e API | Crear `lib/types/article-family.ts` (interfaz) y `lib/api/article-families.ts` (CRUD). | 1.8 |
| 4.2 | I18n | Añadir en `locales/{es,en,ca}/translation.json` claves para "Familia de Artículo", campos, errores. | — |
| 4.3 | Página listado | Crear `app/[locale]/(app)/maestros/familias-articulos/page.tsx`: listado con tabla y acciones. | 4.1 |
| 4.4 | ArticleFamilyTable | Componente tabla: columnas (Code, Name, TaxType, acciones). Botones Editar, Eliminar (con confirmación destructiva). | 4.3 |
| 4.5 | ArticleFamilyForm (modal) | Componente formulario crear/editar en modal (drawer/sheet): Code, Name, Description, selector TaxType (solo de la compañía). Llamada desde página. | 4.1, 4.2 |
| 4.6 | Menú | Añadir en Sidebar bajo "Maestros" entrada "Familias de Artículos" con ruta `/maestros/familias-articulos`. Visibilidad según permiso Consultar/Gestionar. | 4.3 |
| 4.7 | Artículos: selector familia | Si existe UI de artículo (crear/editar) que usaba Family, actualizar a selector de ArticleFamily y campo articleFamilyId. | 2.4, 4.1 |

---

## Fase 5: Auditoría y cierre

| # | Tarea | Detalle | Dependencias |
|---|-------|---------|--------------|
| 5.1 | Log de operaciones en BD | Implementar registro de operaciones CRUD ArticleFamily (quién, cuándo, qué) según SPEC; respetar que Product DbContext no tenga DbSets de Audit/Log (servicio o BD de auditoría). | 1.8 |
| 5.2 | Revisión migraciones | Comprobar índices (CompanyId, Code) en ArticleFamilies; integridad FK Article → ArticleFamily, ArticleFamily → TaxType. | 2.5, 1.3 |
| 5.3 | Build y tests | `dotnet build src/Product/Back/GesFer.Product.sln`, `dotnet test`, `npm run build --prefix src/Product/Front`. | Todas |
| 5.4 | validate-pr | Ejecutar `scripts/validate-pr.ps1`. | 5.3 |
| 5.5 | Documentación y auditoría | Actualizar documentación de rama si aplica; registro en `docs/audits/ACCESS_LOG.md`; si cierre de feature, `docs/audits/YYYYMMDD_HHMM_<BRANCH>_CLOSE.md`. | 5.4 |

---

## Orden sugerido de ejecución (resumen)

1. **Fase 0** (preparar TaxTypes/Companies en demo).
2. **Fase 1** (ArticleFamily completo: entidad, API, tests) sin tocar Article/Family.
3. **Fase 2** (añadir ArticleFamilyId a Article → migrar datos → quitar Family → actualizar albaranes y configs).
4. **Fase 3** (seeds articleFamilies, quitar families, articles con articleFamilyId).
5. **Fase 4** (frontend Familias de Artículos y ajuste de artículos si aplica).
6. **Fase 5** (log auditoría, revisión, build, tests, PR).

---

## Archivos a eliminar o dejar de usar

- `src/Product/Back/domain/Entities/Family.cs`
- `src/Product/Back/Infrastructure/Data/Configurations/FamilyConfiguration.cs`
- Clave `families` y `FamilySeed` / `SeedFamiliesAsync` en `JsonDataSeeder.cs`
- Referencias a `DbSet<Family>`, `Company.Families`, `Article.FamilyId` / `Article.Family`

## Archivos nuevos o modificados (referencia rápida)

- Nuevos: ArticleFamily.cs, ArticleFamilyConfiguration, DTOs/Commands/Queries/Handlers/Controller, tests, types/article-family.ts, api/article-families.ts, page y componentes Familias de Artículos.
- Modificados: Article.cs (ArticleFamilyId, sin FamilyId), ArticleConfiguration, ApplicationDbContext, Company.cs, CreateSalesDeliveryNoteCommandHandler, CreatePurchaseDeliveryNoteCommandHandler, JsonDataSeeder (ArticleFamilies + sin Families), demo-data.json, InitDatabase.cs, Sidebar/menú, permisos.
