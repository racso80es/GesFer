# Documento de implementación: CRUD ArticleFamily y reemplazo de Family

**Plan:** [PLAN-ARTICLE-FAMILY-CRUD.md](PLAN-ARTICLE-FAMILY-CRUD.md)  
**SPEC:** [SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md](SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md)  
**Clarificaciones:** [SPEC-ARTICLE-FAMILY-CRUD_CLARIFICATIONS-PROPUESTA.md](SPEC-ARTICLE-FAMILY-CRUD_CLARIFICATIONS-PROPUESTA.md)  
**Rama:** feat-spec-article-family-*  
**Generado según:** `openspecs/actions/implementation.md`

Este documento unifica todos los touchpoints en el código. No aplica cambios; es la guía para el implementador.

---

## 1. Ítems de implementación (por fase/tarea)

### Fase 0: Preparación

#### 0.1 – Modificar: demo-data.json – TaxTypes
- **Id:** 0.1
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json`
- **Ubicación:** Raíz del JSON; clave `taxTypes`.
- **Propuesta:** Añadir o completar clave `taxTypes` con array de objetos: `id`, `companyId`, `code`, `name`, `description`, `value`. Incluir IVA21 (21), IVA10 (10), IVA4 (4), EXENTO (0). Usar mismo `companyId` que empresas demo (ej. `550e8400-e29b-41d4-a716-446655440000`).
- **Dependencias:** —

#### 0.2 – Revisar: orden de seeds
- **Id:** 0.2
- **Acción:** Revisar (sin cambio obligatorio)
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
- **Ubicación:** Método `SeedDemoDataAsync`; orden de llamadas (TaxTypes, ArticleFamilies, Articles).
- **Propuesta:** Verificar carga de seeds genérica. Maestros, (demo si corresponde) orden contexto Admin, contexto Product; en este caso concreto, TaxTypes antes que ArticleFamilies; ArticleFamilies antes que Articles.
- **Dependencias:** —

---

### Fase 1: Backend – ArticleFamily (nuevo maestro)

#### 1.1 – Crear: entidad ArticleFamily
- **Id:** 1.1
- **Acción:** Crear
- **Ruta:** `src/Product/Back/domain/Entities/ArticleFamily.cs`
- **Ubicación:** Archivo nuevo.
- **Propuesta:** Clase `ArticleFamily : BaseEntity`. Propiedades: `CompanyId` (Guid), `Code` (string), `Name` (string), `Description` (string?), `TaxTypeId` (Guid). Navegación: `Company`, `TaxType`. Namespace `GesFer.Product.Back.Domain.Entities`.
- **Dependencias:** —

#### 1.2 – Crear: ArticleFamilyConfiguration
- **Id:** 1.2
- **Acción:** Crear
- **Ruta:** `src/Product/Back/Infrastructure/Data/Configurations/ArticleFamilyConfiguration.cs`
- **Ubicación:** Archivo nuevo.
- **Propuesta:** `IEntityTypeConfiguration<ArticleFamily>`. Tabla `ArticleFamilies`. Configurar propiedades (Code MaxLength 50, Name MaxLength 100, Description MaxLength 500). Índice único (CompanyId, Code). FK a Company y TaxType. OnDelete Restrict.
- **Dependencias:** 1.1

#### 1.3a – Modificar: ApplicationDbContext – DbSet ArticleFamily
- **Id:** 1.3
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`
- **Ubicación:** Región de DbSets (junto a TaxTypes, Families, etc.).
- **Propuesta:** Añadir `public DbSet<ArticleFamily> ArticleFamilies => Set<ArticleFamily>();` y using al namespace de la entidad.
- **Dependencias:** 1.1

#### 1.3b – Crear: migración EF (solo tabla ArticleFamilies)
- **Id:** 1.3
- **Acción:** Crear (vía herramienta EF)
- **Ruta:** `src/Product/Back/Infrastructure/Migrations/` (nuevo archivo de migración)
- **Ubicación:** Generar con `dotnet ef migrations add AddArticleFamilies --project ...`.
- **Propuesta:** Migración que solo cree tabla `ArticleFamilies` (no tocar Articles ni Families). Revisar Up/Down.
- **Dependencias:** 1.2, 1.3a

#### 1.4 – Crear: DTOs ArticleFamily
- **Id:** 1.4
- **Acción:** Crear
- **Ruta:** `src/Product/Back/application/DTOs/ArticleFamilies/ArticleFamilyDto.cs` (y Create, Update)
- **Ubicación:** Carpeta y archivos nuevos.
- **Propuesta:** `ArticleFamilyDto`: Id, CompanyId, Code, Name, Description, TaxTypeId (y opcionalmente TaxTypeName/Value para lectura). `CreateArticleFamilyDto`: Code, Name, Description, TaxTypeId. `UpdateArticleFamilyDto`: Code, Name, Description, TaxTypeId. Namespace coherente con proyecto (GesFer.Application o GesFer.Product.Application según estructura).
- **Dependencias:** —

#### 1.5 – Crear: Commands ArticleFamilies
- **Id:** 1.5
- **Acción:** Crear
- **Ruta:** `src/Product/Back/application/Commands/ArticleFamilies/CreateArticleFamilyCommand.cs`, `UpdateArticleFamilyCommand.cs`, `DeleteArticleFamilyCommand.cs` (+ validadores)
- **Ubicación:** Archivos nuevos en carpeta ArticleFamilies.
- **Propuesta:** Commands que implementen ICommand/ICommand<T> según convención del proyecto. Validadores FluentValidation: Code único por CompanyId, Name/Code no vacíos, TaxTypeId existente y de la compañía. Delete por Id.
- **Dependencias:** 1.4

#### 1.6 – Crear: Queries ArticleFamilies
- **Id:** 1.6
- **Acción:** Crear
- **Ruta:** `src/Product/Back/application/Queries/ArticleFamilies/GetArticleFamiliesQuery.cs`, `GetArticleFamilyByIdQuery.cs`
- **Ubicación:** Archivos nuevos.
- **Propuesta:** GetArticleFamiliesQuery (sin parámetros o con filtros); GetArticleFamilyByIdQuery(Guid Id). Retorno tipo DTO o Result<ArticleFamilyDto> según convención.
- **Dependencias:** 1.4

#### 1.7 – Crear: Handlers ArticleFamilies
- **Id:** 1.7
- **Acción:** Crear
- **Ruta:** `src/Product/Back/application/Handlers/ArticleFamilies/CreateArticleFamilyCommandHandler.cs`, `UpdateArticleFamilyCommandHandler.cs`, `DeleteArticleFamilyCommandHandler.cs`, `GetArticleFamiliesQueryHandler.cs`, `GetArticleFamilyByIdQueryHandler.cs`
- **Ubicación:** Archivos nuevos.
- **Propuesta:** Handlers que inyecten ApplicationDbContext e IUserContext. Filtrar siempre por CompanyId del usuario. Create/Update: validar unicidad Code, existencia TaxTypeId. Delete: soft delete (IsActive = false, DeletedAt = UtcNow). Queries: solo registros de la compañía.
- **Dependencias:** 1.5, 1.6

#### 1.8 – Crear: ArticleFamiliesController
- **Id:** 1.8
- **Acción:** Crear
- **Ruta:** `src/Product/Back/Api/Controllers/ArticleFamiliesController.cs`
- **Ubicación:** Archivo nuevo (namespace GesFer.Api o según estructura).
- **Propuesta:** Controller con [Authorize], rutas `api/article-families` o `api/[controller]`. GET list (permiso Consultar), GET by id, POST (Gestionar), PUT (Gestionar), DELETE (Gestionar). Inyectar ICommandHandler/ISender o equivalentes. Retornos HTTP 200/201/400/404 según Result. Confirmación destructiva en front; backend solo soft delete.
- **Dependencias:** 1.7

#### 1.9 – Modificar: permisos (master-data o configuración)
- **Id:** 1.9
- **Acción:** Modificar
- **Ruta:** Según proyecto: `src/Product/Back/Infrastructure/Data/Seeds/master-data.json` o donde se definan permisos (Groups/Permissions).
- **Ubicación:** Clave permissions o equivalente.
- **Propuesta:** Añadir dos permisos: uno para Consultar (lectura) ArticleFamily, otro para Gestionar (crear, editar, eliminar). Asociar a grupos según reglas de negocio.
- **Dependencias:** —

#### 1.10 – Crear: tests unitarios ArticleFamilies
- **Id:** 1.10
- **Acción:** Crear
- **Ruta:** `src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/` (o equivalente)
- **Ubicación:** Archivos de test nuevos.
- **Propuesta:** Tests para validadores (Create/Update); tests para handlers (Create, Update, Delete, Get) con mocks de DbContext e IUserContext. Casos: éxito, Code duplicado, TaxTypeId inválido, CompanyId ajeno.
- **Dependencias:** 1.7

#### 1.11 – Crear: tests integración ArticleFamiliesController
- **Id:** 1.11
- **Acción:** Crear
- **Ruta:** `src/Product/Back/tests/GesFer.Product.IntegrationTests/Controllers/ArticleFamiliesControllerTests.cs`
- **Ubicación:** Archivo nuevo.
- **Propuesta:** Tests de integración: GET list, GET by id, POST create, PUT update, DELETE (soft). Usar WebApplicationFactory, BD en memoria si aplica. Autenticación y permisos según setup del proyecto.
- **Dependencias:** 1.8

---

### Fase 2: Backend – Migración Article y reemplazo de Family

#### 2.1 – Modificar: Article – añadir ArticleFamilyId
- **Id:** 2.1
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/domain/Entities/Article.cs`
- **Ubicación:** Propiedades y navegación.
- **Propuesta:** Añadir `public Guid ArticleFamilyId { get; set; }` y `public ArticleFamily ArticleFamily { get; set; } = null!;`. Mantener temporalmente `FamilyId` y `Family` (se quitan en 2.4).
- **Dependencias:** 1.1

#### 2.2 – Modificar: ArticleConfiguration – FK ArticleFamily
- **Id:** 2.2
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Configurations/ArticleConfiguration.cs`
- **Ubicación:** Después de la relación con Family; bloque HasOne/WithMany.
- **Propuesta:** Añadir `HasOne(a => a.ArticleFamily).WithMany().HasForeignKey(a => a.ArticleFamilyId).OnDelete(DeleteBehavior.Restrict)`. Generar migración que añada columna `ArticleFamilyId` (nullable) a Articles. No eliminar aún la relación con Family.
- **Dependencias:** 2.1, 1.2

#### 2.3 – Sin cambio (datos vía seeds)
- **Id:** 2.3
- **Acción:** N/A
- **Ruta:** —
- **Propuesta:** No hay migración de datos; los seeds reemplazarán. Marcar como hecho cuando 2.2 y seeds estén listos.
- **Dependencias:** —

#### 2.4 – Modificar: Article – quitar FamilyId
- **Id:** 2.4
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/domain/Entities/Article.cs`
- **Ubicación:** Propiedades FamilyId y Family.
- **Propuesta:** Eliminar `FamilyId` y navegación `Family`. Dejar `ArticleFamilyId` como obligatorio (no nullable).
- **Dependencias:** 2.3

#### 2.5 – Crear: migración EF – quitar FamilyId y tabla Families
- **Id:** 2.5
- **Acción:** Crear (vía EF)
- **Ruta:** `src/Product/Back/Infrastructure/Migrations/`
- **Ubicación:** Nueva migración.
- **Propuesta:** Migración que: elimine FK y columna `FamilyId` de Articles; elimine tabla `Families` e índices/FK relacionados. Down debe restaurar Families y FamilyId si se requiere rollback.
- **Dependencias:** 2.4

#### 2.6 – Modificar: ArticleConfiguration – solo ArticleFamily
- **Id:** 2.6
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Configurations/ArticleConfiguration.cs`
- **Ubicación:** Bloque HasOne Family.
- **Propuesta:** Eliminar `HasOne(a => a.Family).WithMany(...).HasForeignKey(a => a.FamilyId)`. Dejar solo la relación HasOne ArticleFamily.
- **Dependencias:** 2.4

#### 2.7 – Eliminar: entidad Family
- **Id:** 2.7
- **Acción:** Eliminar
- **Ruta:** `src/Product/Back/domain/Entities/Family.cs`
- **Ubicación:** Archivo completo.
- **Propuesta:** Borrar el archivo.
- **Dependencias:** 2.5

#### 2.8 – Eliminar: FamilyConfiguration
- **Id:** 2.8
- **Acción:** Eliminar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Configurations/FamilyConfiguration.cs`
- **Ubicación:** Archivo completo.
- **Propuesta:** Borrar el archivo.
- **Dependencias:** 2.7

#### 2.9 – Modificar: ApplicationDbContext – quitar DbSet Families
- **Id:** 2.9
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`
- **Ubicación:** Propiedad DbSet<Family> Families.
- **Propuesta:** Eliminar la línea `public DbSet<Family> Families => Set<Family>();` y el using de Family si ya no se usa.
- **Dependencias:** 2.8

#### 2.10 – Modificar: Company – quitar colección Families
- **Id:** 2.10
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/domain/Entities/Company.cs`
- **Ubicación:** Propiedad de navegación ICollection<Family> Families.
- **Propuesta:** Eliminar `public ICollection<Family> Families { get; set; } = ...` (y using de Family si aplica).
- **Dependencias:** 2.7

#### 2.11a – Modificar: CreateSalesDeliveryNoteCommandHandler
- **Id:** 2.11
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/application/Handlers/SalesDeliveryNote/CreateSalesDeliveryNoteCommandHandler.cs`
- **Ubicación:** Consulta de artículos (Include) y cálculo de IVA.
- **Propuesta:** Sustituir `.Include(a => a.Family)` por `.Include(a => a.ArticleFamily).ThenInclude(af => af.TaxType)`. Sustituir `article.Family.IvaPercentage` por `article.ArticleFamily.TaxType.Value` (porcentaje, ej. 21 para 21%).
- **Dependencias:** 2.4

#### 2.11b – Modificar: CreatePurchaseDeliveryNoteCommandHandler
- **Id:** 2.11
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/application/Handlers/PurchaseDeliveryNote/CreatePurchaseDeliveryNoteCommandHandler.cs`
- **Ubicación:** Consulta de artículos (Include) y cálculo de IVA.
- **Propuesta:** Igual que 2.11a: Include ArticleFamily + TaxType; usar TaxType.Value para el porcentaje de IVA.
- **Dependencias:** 2.4

#### 2.12 – Modificar: InitDatabase
- **Id:** 2.12
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/scripts/InitDatabase.cs`
- **Ubicación:** Lista de tablas o referencias a "Families".
- **Propuesta:** Sustituir la cadena "Families" por "ArticleFamilies" donde se listen tablas o se haga referencia a la tabla de familias.
- **Dependencias:** 2.9

---

### Fase 3: Seeds – ArticleFamilies y limpieza de Family

#### 3.1 – Modificar: JsonDataSeeder – ArticleFamilySeed y DemoDataSeed
- **Id:** 3.1
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
- **Ubicación:** Clase DemoDataSeed (propiedades); región de clases Seed privadas (añadir ArticleFamilySeed).
- **Propuesta:** En la clase que deserializa demo-data, añadir `public List<ArticleFamilySeed>? ArticleFamilies { get; set; }`. Crear clase privada `ArticleFamilySeed` con Id, CompanyId, Code, Name, Description, TaxTypeId (strings para JSON; Guid.Parse en seeder).
- **Dependencias:** 1.1

#### 3.2 – Modificar: JsonDataSeeder – SeedArticleFamiliesAsync y llamada
- **Id:** 3.2
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
- **Ubicación:** Método SeedDemoDataAsync (orden de llamadas); nuevo método privado SeedArticleFamiliesAsync.
- **Propuesta:** Implementar `SeedArticleFamiliesAsync(List<ArticleFamilySeed> articleFamilies)`: validar Guid.Parse, que TaxTypeId exista y CompanyId coherente; por cada ítem, si no existe por Id, crear entidad ArticleFamily y Add; SaveChangesAsync. En SeedDemoDataAsync, tras SeedTaxTypesAsync, llamar a SeedArticleFamiliesAsync si data.ArticleFamilies != null; añadir a result.Entities el count.
- **Dependencias:** 3.1

#### 3.3 – Modificar: demo-data.json – clave articleFamilies
- **Id:** 3.3
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json`
- **Ubicación:** Raíz del JSON; nueva clave `articleFamilies`.
- **Propuesta:** Añadir `"articleFamilies": [ { "id": "...", "companyId": "...", "code": "MET", "name": "Metales", "description": "...", "taxTypeId": "<id de taxType IVA21>" }, ... ]` con 2–3 ejemplos (Metales, Plásticos, Consumibles). Usar mismos companyId que taxTypes; taxTypeId = id de un ítem de taxTypes.
- **Dependencias:** 0.1

#### 3.4 – Modificar: JsonDataSeeder – quitar Families
- **Id:** 3.4
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
- **Ubicación:** DemoDataSeed (propiedad Families); SeedDemoDataAsync (llamada SeedFamiliesAsync); método SeedFamiliesAsync; clase FamilySeed.
- **Propuesta:** Eliminar de DemoDataSeed la propiedad Families. En SeedDemoDataAsync, quitar el bloque que comprueba data.Families y llama a SeedFamiliesAsync. Eliminar el método SeedFamiliesAsync completo. Eliminar la clase privada FamilySeed.
- **Dependencias:** 2.9

#### 3.5 – Modificar: ArticleSeed y demo-data articles
- **Id:** 3.5
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs` (clase ArticleSeed); `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json` (clave articles)
- **Ubicación:** ArticleSeed: propiedad FamilyId; JSON: cada ítem de articles.
- **Propuesta:** En ArticleSeed: renombrar/quitar FamilyId; añadir ArticleFamilyId (string). En demo-data.json, en cada artículo: usar `articleFamilyId` con un Guid de articleFamilies; quitar `familyId` si existe.
- **Dependencias:** 3.2, 3.3

#### 3.6 – Modificar: SeedArticlesAsync – ArticleFamilyId
- **Id:** 3.6
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
- **Ubicación:** Método SeedArticlesAsync; construcción del objeto Article.
- **Propuesta:** Al crear la entidad Article, asignar `ArticleFamilyId = Guid.Parse(articleData.ArticleFamilyId)`. No usar FamilyId. Validar que ArticleFamilyId exista en BD si es necesario.
- **Dependencias:** 3.5

#### 3.7 – Modificar: demo-data.json – quitar families
- **Id:** 3.7
- **Acción:** Modificar
- **Ruta:** `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json`
- **Ubicación:** Clave `families` si existe.
- **Propuesta:** Eliminar la clave `families` y su array del JSON.
- **Dependencias:** 3.4

---

### Fase 4: Frontend – Familias de Artículos

#### 4.1 – Crear: tipos e API article-family
- **Id:** 4.1
- **Acción:** Crear
- **Ruta:** `src/Product/Front/lib/types/article-family.ts`, `src/Product/Front/lib/api/article-families.ts`
- **Ubicación:** Archivos nuevos.
- **Propuesta:** En types: interfaz ArticleFamily (id, companyId, code, name, description, taxTypeId; opcional taxTypeName/value). CreateArticleFamilyDto, UpdateArticleFamilyDto. En api: funciones getArticleFamilies, getArticleFamilyById, createArticleFamily, updateArticleFamily, deleteArticleFamily (llamadas a endpoints del controller). Usar fetch o cliente HTTP del proyecto.
- **Dependencias:** 1.8

#### 4.2 – Modificar: i18n (es, en, ca)
- **Id:** 4.2
- **Acción:** Modificar
- **Ruta:** `src/Product/Front/locales/es/translation.json`, `src/Product/Front/locales/en/translation.json`, `src/Product/Front/locales/ca/translation.json`
- **Ubicación:** Sección maestros o nueva clave (ej. articleFamily).
- **Propuesta:** Añadir claves para "Familia de Artículo", "Familias de Artículos", "Código", "Nombre", "Descripción", "Tipo de Tasa", mensajes de error (crear, editar, eliminar, validación). Tres idiomas.
- **Dependencias:** —

#### 4.3 – Crear: página listado familias-articulos
- **Id:** 4.3
- **Acción:** Crear
- **Ruta:** `src/Product/Front/app/[locale]/(app)/maestros/familias-articulos/page.tsx`
- **Ubicación:** Archivo nuevo (crear carpeta maestros si no existe).
- **Propuesta:** Página que muestre título, botón "Nueva familia" (abre modal), y componente ArticleFamilyTable con datos obtenidos de getArticleFamilies(). Protección por permiso (Consultar o Gestionar). Locale desde params.
- **Dependencias:** 4.1

#### 4.4 – Crear: ArticleFamilyTable
- **Id:** 4.4
- **Acción:** Crear
- **Ruta:** `src/Product/Front/components/` (ruta según estructura; ej. `maestros/ArticleFamilyTable.tsx`)
- **Ubicación:** Archivo nuevo.
- **Propuesta:** Tabla con columnas: Code, Name, TaxType (nombre o valor), acciones. Botones Editar (abre modal con formulario) y Eliminar (confirmación destructiva: componente de confirmación explícita según regla seguridad). Llamar a API delete solo tras confirmar.
- **Dependencias:** 4.3

#### 4.5 – Crear: ArticleFamilyForm (modal)
- **Id:** 4.5
- **Acción:** Crear
- **Ruta:** `src/Product/Front/components/` (ej. `maestros/ArticleFamilyForm.tsx`)
- **Ubicación:** Archivo nuevo.
- **Propuesta:** Formulario en modal (drawer o sheet): campos Code, Name, Description (opcional), selector TaxType (cargar tipos de la compañía desde API tax-types). Modo crear/editar; al guardar llamar create o update. Cerrar modal y refrescar tabla al éxito. Validación (Zod o equivalente) y mensajes i18n.
- **Dependencias:** 4.1, 4.2

#### 4.6 – Modificar: Sidebar – menú Familias de Artículos
- **Id:** 4.6
- **Acción:** Modificar
- **Ruta:** `src/Product/Front/components/layout/Sidebar.tsx`
- **Ubicación:** Sección "Maestros" (o equivalente); ítems de menú.
- **Propuesta:** Añadir entrada "Familias de Artículos" (o clave i18n) con ruta `/maestros/familias-articulos`. Mostrar solo si el usuario tiene permiso Consultar o Gestionar para ArticleFamily (según cómo se expongan permisos en el front).
- **Dependencias:** 4.3

#### 4.7 – Revisar: UI de artículos (selector familia)
- **Id:** 4.7
- **Acción:** Modificar (si existe)
- **Ruta:** Buscar en `src/Product/Front` componentes o páginas que muestren/editen Article y usen Family o familyId.
- **Ubicación:** Formularios de artículo, listados que muestren familia.
- **Propuesta:** Si existe tal UI, sustituir selector de Family por selector de ArticleFamily (lista desde getArticleFamilies); campo articleFamilyId. Si no existe, marcar ítem como N/A.
- **Dependencias:** 2.4, 4.1

---

### Fase 5: Auditoría y cierre

#### 5.1 – Implementar: log de operaciones CRUD en BD
- **Id:** 5.1
- **Acción:** Crear/Modificar
- **Ruta:** A determinar según proyecto: servicio de auditoría fuera de Product DbContext (ej. API a servicio de auditoría, o BD de auditoría separada).
- **Ubicación:** Donde se ejecuten Create/Update/Delete de ArticleFamily (handlers o controller); llamada a servicio de log.
- **Propuesta:** Registrar quién (UserId), cuándo (UtcNow), qué (CreateArticleFamily, UpdateArticleFamily, DeleteArticleFamily) y opcionalmente identificador de entidad. No añadir DbSets de Audit/Log en ApplicationDbContext (respetar auditor.back).
- **Dependencias:** 1.8

#### 5.2 – Revisar: migraciones
- **Id:** 5.2
- **Acción:** Revisar
- **Ruta:** `src/Product/Back/Infrastructure/Migrations/` (archivos de migración generados)
- **Ubicación:** Migración AddArticleFamilies; migración que elimina FamilyId y Families.
- **Propuesta:** Comprobar que ArticleFamilies tenga índices (CompanyId, Code único) y FK a TaxType y Company; que la migración de eliminación de Family esté correcta.
- **Dependencias:** 2.5, 1.3b

#### 5.3 – Ejecutar: build y tests
- **Id:** 5.3
- **Acción:** Ejecutar
- **Ruta:** —
- **Propuesta:** `dotnet build src/Product/Back/GesFer.Product.sln`, `dotnet test src/Product/Back/GesFer.Product.sln`, `npm run build --prefix src/Product/Front`. Corregir errores hasta que todo pase.
- **Dependencias:** Todas las anteriores

#### 5.4 – Ejecutar: validate-pr
- **Id:** 5.4
- **Acción:** Ejecutar
- **Ruta:** —
- **Propuesta:** `scripts/validate-pr.ps1` (o equivalente). Resolver incidencias que reporte.
- **Dependencias:** 5.3

#### 5.5 – Documentación y auditoría
- **Id:** 5.5
- **Acción:** Modificar/Crear
- **Ruta:** `docs/audits/ACCESS_LOG.md`; opcional `docs/audits/YYYYMMDD_HHMM_<BRANCH>_CLOSE.md`
- **Ubicación:** Según convención del proyecto.
- **Propuesta:** Registrar en ACCESS_LOG la generación de este IMPL y/o el cierre de la feature. Si se cierra la rama, generar documento de cierre según QA Judge.
- **Dependencias:** 5.4

---

## 2. Resumen por archivo

| Ruta | Ítems |
|------|--------|
| `src/Product/Back/domain/Entities/ArticleFamily.cs` | 1.1 (Crear) |
| `src/Product/Back/domain/Entities/Article.cs` | 2.1, 2.4 |
| `src/Product/Back/domain/Entities/Company.cs` | 2.10 |
| `src/Product/Back/domain/Entities/Family.cs` | 2.7 (Eliminar) |
| `src/Product/Back/Infrastructure/Data/Configurations/ArticleFamilyConfiguration.cs` | 1.2 (Crear) |
| `src/Product/Back/Infrastructure/Data/Configurations/ArticleConfiguration.cs` | 2.2, 2.6 |
| `src/Product/Back/Infrastructure/Data/Configurations/FamilyConfiguration.cs` | 2.8 (Eliminar) |
| `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` | 1.3a, 2.9 |
| `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json` | 0.1, 3.3, 3.5, 3.7 |
| `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs` | 0.2, 3.1, 3.2, 3.4, 3.5, 3.6 |
| `src/Product/Back/Infrastructure/Migrations/*` | 1.3b, 2.5 |
| `src/Product/Back/application/DTOs/ArticleFamilies/*` | 1.4 |
| `src/Product/Back/application/Commands/ArticleFamilies/*` | 1.5 |
| `src/Product/Back/application/Queries/ArticleFamilies/*` | 1.6 |
| `src/Product/Back/application/Handlers/ArticleFamilies/*` | 1.7 |
| `src/Product/Back/application/Handlers/SalesDeliveryNote/CreateSalesDeliveryNoteCommandHandler.cs` | 2.11a |
| `src/Product/Back/application/Handlers/PurchaseDeliveryNote/CreatePurchaseDeliveryNoteCommandHandler.cs` | 2.11b |
| `src/Product/Back/Api/Controllers/ArticleFamiliesController.cs` | 1.8 (Crear) |
| `src/Product/Back/scripts/InitDatabase.cs` | 2.12 |
| `src/Product/Back/tests/.../ArticleFamilies/*` | 1.10 |
| `src/Product/Back/tests/.../ArticleFamiliesControllerTests.cs` | 1.11 |
| master-data o configuración permisos | 1.9 |
| `src/Product/Front/lib/types/article-family.ts` | 4.1 (Crear) |
| `src/Product/Front/lib/api/article-families.ts` | 4.1 (Crear) |
| `src/Product/Front/locales/{es,en,ca}/translation.json` | 4.2 |
| `src/Product/Front/app/[locale]/(app)/maestros/familias-articulos/page.tsx` | 4.3 (Crear) |
| `src/Product/Front/components/.../ArticleFamilyTable.tsx` | 4.4 (Crear) |
| `src/Product/Front/components/.../ArticleFamilyForm.tsx` | 4.5 (Crear) |
| `src/Product/Front/components/layout/Sidebar.tsx` | 4.6 |
| Servicio/BD auditoría (log operaciones) | 5.1 |
| `docs/audits/ACCESS_LOG.md` (y cierre) | 5.5 |

---

## 3. Orden sugerido de aplicación

1. **Fase 0** → 0.1, 0.2  
2. **Fase 1** → 1.1 → 1.2 → 1.3a → 1.3b (aplicar migración) → 1.4 → 1.5 → 1.6 → 1.7 → 1.8 → 1.9 → 1.10 → 1.11  
3. **Fase 2** → 2.1 → 2.2 → migración (columna ArticleFamilyId) → 2.4 → 2.6 → 2.5 (migración eliminar Family) → 2.7 → 2.8 → 2.9 → 2.10 → 2.11a, 2.11b → 2.12  
4. **Fase 3** → 3.1 → 3.2 → 3.3 → 3.5 (ArticleSeed + demo articles) → 3.6 → 3.4 → 3.7  
5. **Fase 4** → 4.1 → 4.2 → 4.3 → 4.4 → 4.5 → 4.6 → 4.7 (si aplica)  
6. **Fase 5** → 5.1 → 5.2 → 5.3 → 5.4 → 5.5  

**Crítico:** Las migraciones 1.3b y 2.5 deben aplicarse en ese orden; 2.4 y 2.6 deben hacerse antes de generar 2.5 para que el modelo no tenga Family.
