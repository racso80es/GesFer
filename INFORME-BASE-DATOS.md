# Informe Exhaustivo: Situación Actual de la Base de Datos - GesFer

**Fecha de Análisis:** 10 de Enero de 2025  
**Analista:** Senior .NET Database Architect  
**Solución:** GesFer - Sistema de Gestión de Compra/Venta de Chatarra

---

## 1. ENTITY FRAMEWORK & CONFIGURACIÓN

### 1.1 Versión de Entity Framework Core

**Versión Instalada:** `8.0.0`

**Ubicación de Referencias:**
- **Proyecto Principal:** `Api/src/Infrastructure/GesFer.Infrastructure.csproj`
  - `Microsoft.EntityFrameworkCore` Version="8.0.0"
  - `Microsoft.EntityFrameworkCore.Design` Version="8.0.0"
  - `Pomelo.EntityFrameworkCore.MySql` Version="8.0.0"

- **Proyecto API:** `Api/src/Api/GesFer.Api.csproj`
  - `Microsoft.EntityFrameworkCore.Design` Version="8.0.0" (solo para herramientas de diseño)

**Framework Target:** `.NET 8.0`

### 1.2 DbContext y Configuración del Proveedor

#### DbContext Principal

**Clase:** `ApplicationDbContext`  
**Ubicación:** `Api/src/Infrastructure/Data/ApplicationDbContext.cs`  
**Namespace:** `GesFer.Infrastructure.Data`

**Características:**
- Hereda de `DbContext`
- Constructor con `DbContextOptions<ApplicationDbContext>`
- Implementa Soft Delete global
- Configuración automática de Sequential GUIDs
- Gestión automática de campos de auditoría

#### Configuración del Proveedor de Base de Datos

**Ubicación:** `Api/src/Api/DependencyInjection.cs` (método `AddApplicationServices`)

**Proveedor:** MySQL 8.0 (mediante Pomelo.EntityFrameworkCore.MySql)

**Cadena de Conexión:**
- **Origen:** `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **Valor por Defecto (fallback):** 
  ```
  Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;
  CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;
  ```

**Configuración Detallada:**
```csharp
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mysqlOptions =>
        {
            mysqlOptions.EnableStringComparisonTranslations();
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    if (isDevelopment)
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

**Características Configuradas:**
- ✅ **Retry on Failure:** Habilitado (5 reintentos, máximo 30 segundos de espera)
- ✅ **String Comparison Translations:** Habilitado (traduce comparaciones de strings)
- ✅ **Sensitive Data Logging:** Solo en desarrollo
- ✅ **Detailed Errors:** Solo en desarrollo

### 1.3 Patrones Avanzados Implementados

#### ✅ **Filtros Globales de Consulta (Global Query Filters)**

**Implementación:** En `ApplicationDbContext.OnModelCreating()`

**Propósito:** Soft Delete global para todas las entidades que heredan de `BaseEntity`

**Código:**
```csharp
private void ConfigureSoftDelete(ModelBuilder modelBuilder)
{
    var entityTypes = modelBuilder.Model.GetEntityTypes()
        .Where(e => typeof(Domain.Common.BaseEntity).IsAssignableFrom(e.ClrType));

    foreach (var entityType in entityTypes)
    {
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var property = Expression.Property(parameter, nameof(Domain.Common.BaseEntity.DeletedAt));
        var nullConstant = Expression.Constant(null, typeof(DateTime?));
        var condition = Expression.Equal(property, nullConstant);
        var lambda = Expression.Lambda(condition, parameter);

        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
    }
}
```

**Efecto:** Todas las consultas automáticamente filtran entidades con `DeletedAt != null`

**Desactivación:** Usar `.IgnoreQueryFilters()` cuando se necesite incluir entidades eliminadas

#### ✅ **Value Generators Personalizados (Sequential GUIDs)**

**Implementación:** En `ApplicationDbContext.OnModelCreating()`

**Propósito:** Generar GUIDs secuenciales (COMB GUIDs) para mejorar el rendimiento de índices agrupados

**Código:**
```csharp
private void ConfigureSequentialGuids(ModelBuilder modelBuilder)
{
    var entityTypes = modelBuilder.Model.GetEntityTypes()
        .Where(e => typeof(Domain.Common.BaseEntity).IsAssignableFrom(e.ClrType));

    foreach (var entityType in entityTypes)
    {
        var idProperty = entityType.FindProperty(nameof(Domain.Common.BaseEntity.Id));
        
        if (idProperty != null && idProperty.ClrType == typeof(Guid))
        {
            idProperty.SetValueGeneratorFactory((property, entityType) => 
                new SequentialGuidValueGenerator());
            idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
        }
    }
}
```

**Clases Relacionadas:**
- `SequentialGuidValueGenerator` - Generador de GUIDs secuenciales
- `ISequentialGuidGenerator` - Interfaz para generadores
- `MySqlSequentialGuidGenerator` - Implementación específica para MySQL

**Beneficio:** Reduce la fragmentación de índices y mejora el ordenamiento natural por fecha de creación

#### ✅ **Actualización Automática de Campos de Auditoría**

**Implementación:** Override de `SaveChanges()` y `SaveChangesAsync()` en `ApplicationDbContext`

**Código:**
```csharp
private void UpdateAuditFields()
{
    var entries = ChangeTracker.Entries<Domain.Common.BaseEntity>();

    foreach (var entry in entries)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.IsActive = true;
                break;

            case EntityState.Modified:
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                break;

            case EntityState.Deleted:
                // Soft Delete
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.IsActive = false;
                break;
        }
    }
}
```

**Campos Gestionados:**
- `CreatedAt` - Se establece automáticamente al crear
- `UpdatedAt` - Se actualiza automáticamente al modificar
- `DeletedAt` - Se establece automáticamente al eliminar (soft delete)
- `IsActive` - Se establece a `true` al crear, `false` al eliminar

#### ❌ **Interceptores**

**Estado:** NO se encontraron interceptores configurados

**Búsqueda Realizada:**
- No se encontraron clases que implementen `IInterceptor`, `IDbCommandInterceptor`, o `IDbConnectionInterceptor`
- No se encontró registro de interceptores en `DependencyInjection.cs`

#### ❌ **Shadow Properties**

**Estado:** NO se encontraron Shadow Properties configuradas

**Observación:** Todas las propiedades de auditoría (`CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsActive`) están definidas explícitamente en `BaseEntity` como propiedades normales, no como Shadow Properties.

---

## 2. ESTRUCTURA DE DATOS (Mapeo)

### 2.1 Entidad Base

**Clase:** `BaseEntity`  
**Ubicación:** `Api/src/domain/BaseEntity.cs`  
**Namespace:** `GesFer.Domain.Common`

**Propiedades:**
- `Guid Id` - Identificador único (GUID secuencial generado automáticamente)
- `DateTime CreatedAt` - Fecha de creación (UTC)
- `DateTime? UpdatedAt` - Fecha de última actualización (UTC, nullable)
- `DateTime? DeletedAt` - Fecha de eliminación para Soft Delete (UTC, nullable)
- `bool IsActive` - Indica si la entidad está activa
- `bool IsDeleted` - Propiedad calculada que retorna `DeletedAt.HasValue`

**Todas las entidades del dominio heredan de `BaseEntity`**

### 2.2 Tablas Principales y Relaciones

#### 2.2.1 Entidades Maestras (Sin Relación con Company)

| Entidad | Tabla | Descripción | Relaciones |
|---------|-------|-------------|-------------|
| `Language` | `Languages` | Idiomas del sistema (es, en, ca) | 1:N con `Country`, `Company`, `User` |
| `Country` | `Countries` | Países | 1:N con `State` |
| `State` | `States` | Provincias/Estados | 1:N con `City` |
| `City` | `Cities` | Ciudades | 1:N con `PostalCode` |
| `PostalCode` | `PostalCodes` | Códigos postales | N:1 con `City` |
| `Permission` | `Permissions` | Permisos del sistema | M:N con `Group` (vía `GroupPermission`), M:N con `User` (vía `UserPermission`) |
| `Group` | `Groups` | Grupos de usuarios | M:N con `User` (vía `UserGroup`), M:N con `Permission` (vía `GroupPermission`) |
| `AdminUser` | `AdminUsers` | Usuarios administrativos del sistema | Sin relaciones (tabla independiente) |
| `AuditLog` | `AuditLogs` | Registro de auditoría | Sin relaciones (tabla independiente) |

#### 2.2.2 Entidades Multi-Tenant (Relacionadas con Company)

| Entidad | Tabla | Descripción | Relaciones |
|---------|-------|-------------|-------------|
| `Company` | `Companies` | Empresas (Tenants) | 1:N con `User`, `Family`, `Article`, `Tariff`, `Supplier`, `Customer`, `PurchaseInvoice`, `SalesInvoice` |
| `User` | `Users` | Usuarios de la empresa | N:1 con `Company`, N:1 con `Language`, M:N con `Group` (vía `UserGroup`), M:N con `Permission` (vía `UserPermission`) |
| `Family` | `Families` | Familias de artículos | N:1 con `Company`, 1:N con `Article` |
| `Article` | `Articles` | Artículos del catálogo | N:1 con `Company`, N:1 con `Family`, 1:N con `TariffItem`, 1:N con `PurchaseDeliveryNoteLine`, 1:N con `SalesDeliveryNoteLine` |
| `Tariff` | `Tariffs` | Tarifas de compra/venta | N:1 con `Company`, 1:N con `TariffItem`, 1:N con `Supplier` (BuyTariff), 1:N con `Customer` (SellTariff) |
| `TariffItem` | `TariffItems` | Items de tarifa | N:1 con `Tariff`, N:1 con `Article` |
| `Supplier` | `Suppliers` | Proveedores | N:1 con `Company`, N:1 con `Tariff` (BuyTariff), 1:N con `PurchaseDeliveryNote` |
| `Customer` | `Customers` | Clientes | N:1 con `Company`, N:1 con `Tariff` (SellTariff), 1:N con `SalesDeliveryNote` |
| `PurchaseInvoice` | `PurchaseInvoices` | Facturas de compra | N:1 con `Company`, 1:N con `PurchaseDeliveryNote` |
| `PurchaseDeliveryNote` | `PurchaseDeliveryNotes` | Albaranes de compra | N:1 con `Company`, N:1 con `Supplier`, N:1 con `PurchaseInvoice`, 1:N con `PurchaseDeliveryNoteLine` |
| `PurchaseDeliveryNoteLine` | `PurchaseDeliveryNoteLines` | Líneas de albarán de compra | N:1 con `PurchaseDeliveryNote`, N:1 con `Article` |
| `SalesInvoice` | `SalesInvoices` | Facturas de venta | N:1 con `Company`, 1:N con `SalesDeliveryNote` |
| `SalesDeliveryNote` | `SalesDeliveryNotes` | Albaranes de venta | N:1 con `Company`, N:1 con `Customer`, N:1 con `SalesInvoice`, 1:N con `SalesDeliveryNoteLine` |
| `SalesDeliveryNoteLine` | `SalesDeliveryNoteLines` | Líneas de albarán de venta | N:1 con `SalesDeliveryNote`, N:1 con `Article` |

#### 2.2.3 Tablas de Relación Many-to-Many

| Entidad | Tabla | Descripción | Relaciones |
|---------|-------|-------------|-------------|
| `UserGroup` | `UserGroups` | Relación User ↔ Group | N:1 con `User`, N:1 con `Group` |
| `UserPermission` | `UserPermissions` | Relación User ↔ Permission | N:1 con `User`, N:1 con `Permission` |
| `GroupPermission` | `GroupPermissions` | Relación Group ↔ Permission | N:1 con `Group`, N:1 con `Permission` |

### 2.3 Resumen de Relaciones

#### Relaciones 1:N (One-to-Many)

1. **Country → State** (1:N)
   - `State.CountryId` → `Country.Id`
   - DeleteBehavior: `Restrict`

2. **State → City** (1:N)
   - `City.StateId` → `State.Id`
   - DeleteBehavior: `Restrict`

3. **City → PostalCode** (1:N)
   - `PostalCode.CityId` → `City.Id`
   - DeleteBehavior: `Restrict`

4. **Language → Country** (1:N)
   - `Country.LanguageId` → `Language.Id`
   - DeleteBehavior: `Restrict`

5. **Company → User** (1:N)
   - `User.CompanyId` → `Company.Id`
   - DeleteBehavior: `Restrict`

6. **Company → Family** (1:N)
   - `Family.CompanyId` → `Company.Id`
   - DeleteBehavior: `Restrict`

7. **Company → Article** (1:N)
   - `Article.CompanyId` → `Company.Id`
   - DeleteBehavior: `Restrict`

8. **Company → Tariff** (1:N)
   - `Tariff.CompanyId` → `Company.Id`
   - DeleteBehavior: `Restrict`

9. **Company → Supplier** (1:N)
   - `Supplier.CompanyId` → `Company.Id`
   - DeleteBehavior: `Restrict`

10. **Company → Customer** (1:N)
    - `Customer.CompanyId` → `Company.Id`
    - DeleteBehavior: `Restrict`

11. **Company → PurchaseInvoice** (1:N)
    - `PurchaseInvoice.CompanyId` → `Company.Id`
    - DeleteBehavior: `Restrict`

12. **Company → SalesInvoice** (1:N)
    - `SalesInvoice.CompanyId` → `Company.Id`
    - DeleteBehavior: `Restrict`

13. **Company → PurchaseDeliveryNote** (1:N)
    - `PurchaseDeliveryNote.CompanyId` → `Company.Id`
    - DeleteBehavior: `Restrict`

14. **Company → SalesDeliveryNote** (1:N)
    - `SalesDeliveryNote.CompanyId` → `Company.Id`
    - DeleteBehavior: `Restrict`

15. **Family → Article** (1:N)
    - `Article.FamilyId` → `Family.Id`
    - DeleteBehavior: `Restrict`

16. **Tariff → TariffItem** (1:N)
    - `TariffItem.TariffId` → `Tariff.Id`
    - DeleteBehavior: `Cascade`

17. **Tariff → Supplier** (1:N, opcional)
    - `Supplier.BuyTariffId` → `Tariff.Id` (nullable)
    - DeleteBehavior: `SetNull`

18. **Tariff → Customer** (1:N, opcional)
    - `Customer.SellTariffId` → `Tariff.Id` (nullable)
    - DeleteBehavior: `SetNull`

19. **Supplier → PurchaseDeliveryNote** (1:N)
    - `PurchaseDeliveryNote.SupplierId` → `Supplier.Id`
    - DeleteBehavior: `Restrict`

20. **Customer → SalesDeliveryNote** (1:N)
    - `SalesDeliveryNote.CustomerId` → `Customer.Id`
    - DeleteBehavior: `Restrict`

21. **PurchaseInvoice → PurchaseDeliveryNote** (1:N, opcional)
    - `PurchaseDeliveryNote.PurchaseInvoiceId` → `PurchaseInvoice.Id` (nullable)
    - DeleteBehavior: `SetNull`

22. **SalesInvoice → SalesDeliveryNote** (1:N, opcional)
    - `SalesDeliveryNote.SalesInvoiceId` → `SalesInvoice.Id` (nullable)
    - DeleteBehavior: `SetNull`

23. **PurchaseDeliveryNote → PurchaseDeliveryNoteLine** (1:N)
    - `PurchaseDeliveryNoteLine.PurchaseDeliveryNoteId` → `PurchaseDeliveryNote.Id`
    - DeleteBehavior: `Cascade`

24. **SalesDeliveryNote → SalesDeliveryNoteLine** (1:N)
    - `SalesDeliveryNoteLine.SalesDeliveryNoteId` → `SalesDeliveryNote.Id`
    - DeleteBehavior: `Cascade`

25. **Article → TariffItem** (1:N)
    - `TariffItem.ArticleId` → `Article.Id`
    - DeleteBehavior: `Restrict`

26. **Article → PurchaseDeliveryNoteLine** (1:N)
    - `PurchaseDeliveryNoteLine.ArticleId` → `Article.Id`
    - DeleteBehavior: `Restrict`

27. **Article → SalesDeliveryNoteLine** (1:N)
    - `SalesDeliveryNoteLine.ArticleId` → `Article.Id`
    - DeleteBehavior: `Restrict`

28. **Language → Company** (1:N, opcional)
    - `Company.LanguageId` → `Language.Id` (nullable)
    - DeleteBehavior: `Restrict`

29. **Language → User** (1:N, opcional)
    - `User.LanguageId` → `Language.Id` (nullable)
    - DeleteBehavior: `Restrict`

#### Relaciones M:N (Many-to-Many)

1. **User ↔ Group** (M:N)
   - Tabla intermedia: `UserGroups`
   - `UserGroup.UserId` → `User.Id`
   - `UserGroup.GroupId` → `Group.Id`
   - DeleteBehavior: `Cascade` en ambas direcciones
   - Índice único compuesto: `(UserId, GroupId)`

2. **User ↔ Permission** (M:N)
   - Tabla intermedia: `UserPermissions`
   - `UserPermission.UserId` → `User.Id`
   - `UserPermission.PermissionId` → `Permission.Id`
   - DeleteBehavior: `Cascade` en ambas direcciones
   - Índice único compuesto: `(UserId, PermissionId)`

3. **Group ↔ Permission** (M:N)
   - Tabla intermedia: `GroupPermissions`
   - `GroupPermission.GroupId` → `Group.Id`
   - `GroupPermission.PermissionId` → `Permission.Id`
   - DeleteBehavior: `Cascade` en ambas direcciones
   - Índice único compuesto: `(GroupId, PermissionId)`

### 2.4 Configuraciones Fluent API Destacadas

#### Configuraciones de Índices

**Índices Únicos Compuestos:**
- `Articles`: `(CompanyId, Code)` - Código único por empresa
- `Users`: `(CompanyId, Username)` - Username único por empresa
- `Groups`: `Name` - Nombre único global
- `Languages`: `Code` - Código único global
- `Countries`: `Code` - Código único global
- `Permissions`: `Key` - Key único global
- `Cities`: `(StateId, Name)` - Nombre único por provincia
- `States`: `(CountryId, Name)` - Nombre único por país
- `PostalCodes`: `(CityId, Code)` - Código único por ciudad
- `PurchaseInvoices`: `(CompanyId, InvoiceNumber)` - Número de factura único por empresa
- `SalesInvoices`: `(CompanyId, InvoiceNumber)` - Número de factura único por empresa
- `TariffItems`: `(TariffId, ArticleId)` - Item único por tarifa y artículo
- `UserGroups`: `(UserId, GroupId)` - Relación única
- `UserPermissions`: `(UserId, PermissionId)` - Relación única
- `GroupPermissions`: `(GroupId, PermissionId)` - Relación única

**Índices No Únicos:**
- Múltiples índices en claves foráneas para optimizar joins
- Índices en campos de búsqueda frecuente (`Name`, `Code`, `Reference`, etc.)

#### Check Constraints

**Article:**
- `CK_Article_BuyPrice`: `BuyPrice >= 0`
- `CK_Article_SellPrice`: `SellPrice >= 0`

**TariffItem:**
- `CK_TariffItem_Price`: `Price >= 0`

**PurchaseDeliveryNoteLine:**
- `CK_PurchaseDeliveryNoteLine_Price`: `Price >= 0`
- `CK_PurchaseDeliveryNoteLine_Quantity`: `Quantity > 0`

**SalesDeliveryNoteLine:**
- `CK_SalesDeliveryNoteLine_Price`: `Price >= 0`
- `CK_SalesDeliveryNoteLine_Quantity`: `Quantity > 0`

#### Precisiones Decimales

Todas las propiedades de tipo `decimal` usan:
- **Precision:** 18
- **Scale:** 4

**Ejemplos:**
- `Article.BuyPrice`, `Article.SellPrice`, `Article.Stock`
- `TariffItem.Price`
- `PurchaseDeliveryNoteLine.Quantity`, `Price`, `Subtotal`, `IvaAmount`, `Total`
- `SalesDeliveryNoteLine.Quantity`, `Price`, `Subtotal`, `IvaAmount`, `Total`
- `PurchaseInvoice.Subtotal`, `IvaAmount`, `Total`
- `SalesInvoice.Subtotal`, `IvaAmount`, `Total`
- `Family.IvaPercentage`

#### Configuración de Columnas String

Todas las propiedades `string` se mapean a `varchar` con:
- **Charset:** `utf8mb4` (configurado a nivel de base de datos)
- **Collation:** `utf8mb4_unicode_ci` (por defecto en MySQL 8.0)

**Configuración en OnModelCreating:**
```csharp
private void ConfigureUtf8(ModelBuilder modelBuilder)
{
    var entityTypes = modelBuilder.Model.GetEntityTypes();
    foreach (var entityType in entityTypes)
    {
        var properties = entityType.GetProperties()
            .Where(p => p.ClrType == typeof(string));
        foreach (var property in properties)
        {
            property.SetColumnType("varchar");
        }
    }
}
```

### 2.5 Ubicación de Configuraciones

**Patrón:** Cada entidad tiene su propia clase de configuración que implementa `IEntityTypeConfiguration<T>`

**Ubicación:** `Api/src/Infrastructure/Data/Configurations/`

**Archivos de Configuración:**
- `AdminUserConfiguration.cs`
- `ArticleConfiguration.cs`
- `AuditLogConfiguration.cs`
- `CityConfiguration.cs`
- `CompanyConfiguration.cs`
- `CountryConfiguration.cs`
- `CustomerConfiguration.cs`
- `FamilyConfiguration.cs`
- `GroupConfiguration.cs`
- `GroupPermissionConfiguration.cs`
- `LanguageConfiguration.cs`
- `PermissionConfiguration.cs`
- `PostalCodeConfiguration.cs`
- `PurchaseDeliveryNoteConfiguration.cs`
- `PurchaseDeliveryNoteLineConfiguration.cs`
- `PurchaseInvoiceConfiguration.cs`
- `SalesDeliveryNoteConfiguration.cs`
- `SalesDeliveryNoteLineConfiguration.cs`
- `SalesInvoiceConfiguration.cs`
- `StateConfiguration.cs`
- `SupplierConfiguration.cs`
- `TariffConfiguration.cs`
- `TariffItemConfiguration.cs`
- `UserConfiguration.cs`
- `UserGroupConfiguration.cs`
- `UserPermissionConfiguration.cs`

**Aplicación Automática:**
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

---

## 3. ESTRATEGIA DE SEEDING (Semillas)

### 3.1 Métodos de Seeding Identificados

#### 3.1.1 Seeding desde Archivos JSON (Principal)

**Clase:** `JsonDataSeeder`  
**Ubicación:** `Api/src/Infrastructure/Services/JsonDataSeeder.cs`

**Archivos JSON de Seed:**
1. **`master-data.json`**
   - **Ubicación:** `Api/src/Infrastructure/Seeds/master-data.json`
   - **Contenido:** Datos maestros del sistema
     - Languages (idiomas)
     - Permissions (permisos)
     - Groups (grupos)
     - GroupPermissions (relaciones grupo-permiso)

2. **`demo-data.json`**
   - **Ubicación:** `Api/src/Infrastructure/Seeds/demo-data.json`
   - **Contenido:** Datos de demostración
     - Companies (empresas)
     - Users (usuarios)
     - UserGroups (relaciones usuario-grupo)
     - UserPermissions (relaciones usuario-permiso)
     - Families (familias de artículos)
     - Articles (artículos)
     - Suppliers (proveedores)
     - Customers (clientes)

3. **`test-data.json`**
   - **Ubicación:** `Api/src/Infrastructure/Seeds/test-data.json`
   - **Contenido:** Datos de prueba para tests de integración
     - Companies, Users, Groups, Permissions
     - UserGroups, GroupPermissions

**Métodos de Seeding:**
- `SeedMasterDataAsync()` - Carga desde `master-data.json`
- `SeedDemoDataAsync()` - Carga desde `demo-data.json`
- `SeedTestDataAsync()` - Carga desde `test-data.json`

**Características:**
- ✅ Usa `System.Text.Json` para deserialización
- ✅ Maneja entidades existentes (no duplica)
- ✅ Restaura entidades soft-deleted si existen
- ✅ Usa `IgnoreQueryFilters()` para buscar entidades eliminadas
- ✅ Hash BCrypt para contraseñas (hash fijo conocido para "admin123")

#### 3.1.2 Seeding Programático (Datos Maestros de España)

**Clase:** `MasterDataSeeder`  
**Ubicación:** `Api/src/Infrastructure/Services/MasterDataSeeder.cs`

**Métodos:**
- `SeedLanguagesAsync()` - Carga idiomas maestros (es, en, ca) programáticamente
- `SeedSpainDataAsync()` - Carga datos maestros de España
  - Crea el país España
  - Carga todas las provincias españolas (52 provincias/ciudades autónomas)
  - Carga ciudades principales y códigos postales

**Características:**
- ✅ Usa `ISequentialGuidGenerator` para generar IDs
- ✅ Carga datos geográficos de España (países, provincias, ciudades, códigos postales)
- ✅ Maneja entidades existentes y soft-deleted

#### 3.1.3 Seeding mediante SetupService

**Clase:** `SetupService`  
**Ubicación:** `Api/src/Api/Services/SetupService.cs`

**Método Principal:** `SeedInitialDataAsync()`

**Proceso:**
1. Carga datos maestros desde JSON (`JsonDataSeeder.SeedMasterDataAsync()`)
2. Carga datos de demostración desde JSON (`JsonDataSeeder.SeedDemoDataAsync()`)
3. Crea usuario administrativo (`AdminUser`) manualmente
   - Username: "admin"
   - Password: "admin123" (hash BCrypt fijo)
   - Role: "Admin"

**Endpoint:** `POST /api/setup/initialize` (en `SetupController`)

#### 3.1.4 Seeding mediante Extensiones de Base de Datos

**Clase:** `DatabaseExtensions`  
**Ubicación:** `Api/src/Infrastructure/Extensions/DatabaseExtensions.cs`

**Métodos de Extensión:**
- `MigrateAndSeedAsync()` - Aplica migraciones y ejecuta seeding de datos maestros
- `SeedDataAsync()` - Ejecuta seeding completo (maestros, demo, opcionalmente test)

**Uso:**
- Llamado desde `SeedRunner` (proyecto separado para ejecutar seeds)
- Llamado desde scripts de inicialización

#### 3.1.5 Seeding para Tests de Integración

**Clase:** `TestDataSeeder`  
**Ubicación:** `Api/src/IntegrationTests/Helpers/TestDataSeeder.cs`

**Método:** `SeedTestDataAsync()`

**Características:**
- Limpia datos existentes antes de insertar
- Carga desde `test-data.json` mediante `JsonDataSeeder`
- Usa `IgnoreQueryFilters()` para limpiar entidades soft-deleted

### 3.2 Rutas Exactas de Archivos de Datos

**Directorio Base:** `Api/src/Infrastructure/Seeds/`

**Archivos:**
1. `Api/src/Infrastructure/Seeds/master-data.json`
2. `Api/src/Infrastructure/Seeds/demo-data.json`
3. `Api/src/Infrastructure/Seeds/test-data.json`

**Resolución de Rutas:**
El `JsonDataSeeder` busca los archivos en el siguiente orden:
1. `bin/Debug/net8.0/Seeds/` (directorio de salida)
2. `Infrastructure/Seeds/` (desde el código fuente)
3. `Api/src/Infrastructure/Seeds/` (fallback desde la raíz del proyecto)

### 3.3 Lógica de Procesamiento

**Flujo de Seeding Principal:**

1. **Inicialización de Base de Datos:**
   ```
   SetupService.InitializeDatabaseAsync()
   ├── Crea la base de datos si no existe
   ├── Aplica migraciones (Database.MigrateAsync())
   └── Ejecuta SeedInitialDataAsync()
   ```

2. **SeedInitialDataAsync:**
   ```
   SetupService.SeedInitialDataAsync()
   ├── JsonDataSeeder.SeedMasterDataAsync()
   │   ├── Lee master-data.json
   │   ├── SeedLanguagesAsync()
   │   ├── SeedPermissionsAsync()
   │   ├── SeedGroupsAsync()
   │   └── SeedGroupPermissionsAsync()
   ├── JsonDataSeeder.SeedDemoDataAsync()
   │   ├── Lee demo-data.json
   │   ├── SeedCompaniesAsync()
   │   ├── SeedUsersAsync()
   │   ├── SeedUserGroupsAsync()
   │   ├── SeedUserPermissionsAsync()
   │   ├── SeedFamiliesAsync()
   │   ├── SeedArticlesAsync()
   │   ├── SeedSuppliersAsync()
   │   └── SeedCustomersAsync()
   └── Crea AdminUser manualmente
   ```

**Estrategia de Inserción:**
- ✅ **Idempotente:** Verifica existencia antes de insertar
- ✅ **Respeto a Soft Delete:** Restaura entidades eliminadas si existen
- ✅ **Orden de Dependencias:** Respeta el orden de inserción (países → provincias → ciudades → códigos postales)
- ✅ **GUIDs Fijos:** Usa GUIDs predefinidos en JSON para datos maestros
- ✅ **GUIDs Secuenciales:** Usa `ISequentialGuidGenerator` para datos generados programáticamente

### 3.4 Scripts SQL Externos (Legacy)

**Ubicación:** `Api/scripts/seed-data.sql`

**Estado:** ⚠️ **LEGACY - No se usa actualmente**

**Nota:** Según comentarios en `setup-database.ps1`:
```
# NOTA: Los datos ahora se cargan desde archivos JSON, no desde SQL
# Los datos se cargan automáticamente cuando se inicia la API o mediante el endpoint /api/setup/initialize
```

**Contenido:** Script SQL con INSERTs directos (mantenido por compatibilidad/historial)

---

## 4. MIGRACIONES

### 4.1 Estado Actual de Migraciones

**Directorio:** `Api/src/Infrastructure/Migrations/`

**Migraciones Existentes:**

1. **`20260109104825_InitialCreate`**
   - **Fecha:** 09 de Enero de 2025, 10:48:25
   - **Descripción:** Migración inicial que crea todas las tablas base del sistema
   - **Tablas Creadas:**
     - Groups, Languages, Permissions
     - Countries, States, Cities, PostalCodes
     - Companies
     - Families, Articles
     - Tariffs, TariffItems
     - Users, UserGroups, UserPermissions, GroupPermissions
     - Suppliers, Customers
     - PurchaseInvoices, PurchaseDeliveryNotes, PurchaseDeliveryNoteLines
     - SalesInvoices, SalesDeliveryNotes, SalesDeliveryNoteLines
   - **Archivos:**
     - `20260109104825_InitialCreate.cs`
     - `20260109104825_InitialCreate.Designer.cs`

2. **`20260110064152_AddAdminUsersAndAuditLogs`**
   - **Fecha:** 10 de Enero de 2025, 06:41:52
   - **Descripción:** Agrega tablas para usuarios administrativos y logs de auditoría
   - **Tablas Creadas:**
     - AdminUsers
     - AuditLogs
   - **Archivos:**
     - `20260110064152_AddAdminUsersAndAuditLogs.cs`
     - `20260110064152_AddAdminUsersAndAuditLogs.Designer.cs`

3. **`ApplicationDbContextModelSnapshot.cs`**
   - **Estado:** Snapshot actual del modelo (después de todas las migraciones)

**Total de Migraciones:** 2 migraciones aplicadas

**Última Migración:** `20260110064152_AddAdminUsersAndAuditLogs` (10 de Enero de 2025)

### 4.2 Aplicación Automática de Migraciones

#### ❌ **NO se aplican automáticamente al arrancar la aplicación**

**Evidencia:**

1. **En `Program.cs` (líneas 90-92):**
   ```csharp
   // NOTA: La gestión de la base de datos (migraciones, creación de tablas, datos iniciales)
   // se realiza mediante scripts externos (inicializar-completo.bat, scripts SQL, etc.)
   // La API solo se conecta a la base de datos existente sin realizar verificaciones automáticas.
   ```

2. **No se encontró llamada a `context.Database.Migrate()` o `context.Database.MigrateAsync()` en:**
   - `Program.cs`
   - `DependencyInjection.cs`
   - `SetupService.cs` (aunque tiene un método `InitializeDatabaseAsync`, no se llama automáticamente)

#### ✅ **Aplicación Manual mediante SetupService**

**Método:** `SetupService.InitializeDatabaseAsync()`

**Ubicación:** `Api/src/Api/Services/SetupService.cs`

**Código:**
```csharp
public async Task<(bool Success, string? Error)> InitializeDatabaseAsync()
{
    try
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SetupService>>();

        // Aplicar migraciones pendientes
        logger.LogInformation("Aplicando migraciones pendientes...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas correctamente");

        // ... resto del proceso de inicialización
    }
}
```

**Endpoint:** `POST /api/setup/initialize` (en `SetupController`)

**Uso:** Se debe llamar manualmente al endpoint o ejecutar el método programáticamente

#### ✅ **Aplicación mediante Extensiones**

**Método:** `DatabaseExtensions.MigrateAndSeedAsync()`

**Ubicación:** `Api/src/Infrastructure/Extensions/DatabaseExtensions.cs`

**Código:**
```csharp
public static async Task MigrateAndSeedAsync(this ApplicationDbContext context, IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
    
    try
    {
        // Aplicar migraciones pendientes
        logger.LogInformation("Aplicando migraciones pendientes...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas correctamente");

        // Ejecutar seeding SOLO de datos maestros (durante migraciones)
        logger.LogInformation("Ejecutando seeding de datos maestros...");
        var seeder = serviceProvider.GetRequiredService<JsonDataSeeder>();
        await seeder.SeedMasterDataAsync();

        logger.LogInformation("Seeding de datos maestros completado correctamente");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones o ejecutar seeding");
        throw;
    }
}
```

**Uso:** Llamado desde `SeedRunner` o scripts de inicialización

### 4.3 Comandos de Migración

**Herramienta:** `dotnet ef`

**Comandos Típicos:**
```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion --project Api/src/Infrastructure --startup-project Api/src/Api

# Aplicar migraciones
dotnet ef database update --project Api/src/Infrastructure --startup-project Api/src/Api

# Revertir última migración
dotnet ef database update NombreMigracionAnterior --project Api/src/Infrastructure --startup-project Api/src/Api

# Generar script SQL
dotnet ef migrations script --project Api/src/Infrastructure --startup-project Api/src/Api
```

**Contexto de Diseño:** `Api/src/Api` (proyecto de inicio)

**Proyecto de Migraciones:** `Api/src/Infrastructure`

---

## 5. RESUMEN Y OBSERVACIONES

### 5.1 Puntos Fuertes

✅ **Arquitectura Sólida:**
- Separación clara de responsabilidades (Domain, Infrastructure, Application)
- Configuraciones Fluent API bien organizadas
- Patrón Repository implementado

✅ **Patrones Avanzados:**
- Soft Delete global mediante Query Filters
- Sequential GUIDs para optimización de índices
- Actualización automática de campos de auditoría

✅ **Seeding Robusto:**
- Múltiples estrategias (JSON, programático, tests)
- Idempotente y respeta soft delete
- Orden de dependencias bien manejado

✅ **Configuración Completa:**
- Índices únicos compuestos bien definidos
- Check constraints para validación de datos
- Precisiones decimales consistentes

### 5.2 Áreas de Mejora Potencial

⚠️ **Migraciones No Automáticas:**
- Las migraciones no se aplican automáticamente al arrancar
- Requiere llamada manual al endpoint `/api/setup/initialize`
- **Recomendación:** Considerar aplicar migraciones automáticamente en desarrollo

⚠️ **Falta de Interceptores:**
- No hay interceptores configurados
- **Oportunidad:** Podrían usarse para logging de queries, auditoría automática, etc.

⚠️ **Scripts SQL Legacy:**
- Existe `seed-data.sql` que no se usa
- **Recomendación:** Eliminar o documentar claramente como legacy

### 5.3 Información No Encontrada

❌ **Interceptores:** No se encontraron interceptores configurados

❌ **Shadow Properties:** No se encontraron Shadow Properties

❌ **HasData() en Migraciones:** No se usa `modelBuilder.Entity<T>().HasData()` en las configuraciones (el seeding se hace mediante servicios)

---

## 6. CONCLUSIÓN

La base de datos de GesFer está bien estructurada y utiliza patrones modernos de Entity Framework Core 8.0. La arquitectura es sólida, con separación clara de responsabilidades y configuraciones bien organizadas. El sistema de seeding es robusto y flexible, aunque las migraciones requieren aplicación manual.

**Estado General:** ✅ **SALUDABLE** - La base de datos está bien diseñada y mantenida, con oportunidades de mejora menores.

---

**Fin del Informe**
