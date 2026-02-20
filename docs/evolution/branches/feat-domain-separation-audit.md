# Auditoría de Invariantes y Verificación de Extracción S+ (Fase 3.5)

**Fecha**: 2026-01-26  
**Rama**: `feat/domain-separation`  
**Autor**: Senior Software Architect / Tekton Governance  
**Estado**: AUDITORÍA COMPLETA

---

## RESUMEN EJECUTIVO

Se ha realizado una auditoría exhaustiva de la purificación de Shared y la separación de dominios. Se han identificado **7 categorías de problemas** que requieren corrección antes de continuar con la migración.

**Nivel de Severidad**: 🔴 **CRÍTICO** - Requiere corrección inmediata antes de continuar.

---

## 1. PROBLEMAS CRÍTICOS EN BACKEND (C#)

### 1.1 ❌ Namespaces Antiguos en Product/Back

**Problema**: Todas las entidades en `src/Product/Back/src/domain/Entities/` todavía usan el namespace antiguo `GesFer.Domain.Entities` en lugar de un namespace específico de Product.

**Archivos Afectados** (23 entidades):
- `AdminUser.cs` → Debería estar en Admin, no en Product
- `AuditLog.cs` → Debería estar en Admin, no en Product
- `Log.cs` → Debería estar en Admin, no en Product
- `Company.cs`, `User.cs`, `Customer.cs`, `Supplier.cs`
- `Article.cs`, `Family.cs`, `Tariff.cs`, `TariffItem.cs`
- `Group.cs`, `Permission.cs`, `UserGroup.cs`, `UserPermission.cs`, `GroupPermission.cs`
- `PurchaseDeliveryNote.cs`, `PurchaseDeliveryNoteLine.cs`, `PurchaseInvoice.cs`
- `SalesDeliveryNote.cs`, `SalesDeliveryNoteLine.cs`, `SalesInvoice.cs`

**Impacto**: 
- Violación de separación de dominios
- Confusión sobre qué pertenece a qué dominio
- Dificulta la migración futura a Admin

**Acción Requerida**: 
- Cambiar namespace a `GesFer.Product.Back.Domain.Entities` para entidades Product
- Mover `AdminUser`, `AuditLog`, `Log` a `src/Admin/Back/src/domain/Entities/` con namespace `GesFer.Admin.Back.Domain.Entities`

---

### 1.2 ❌ Referencias al Namespace Antiguo

**Problema**: 57 archivos en Product/Back todavía usan `using GesFer.Domain.Entities` o `using GesFer.Domain.Services`.

**Archivos Afectados** (muestra):
- `ProductDbContext.cs` (línea 1)
- Todos los Handlers (CreateCountryCommandHandler, CreateCityCommandHandler, etc.)
- Todas las Configurations (CountryConfiguration, StateConfiguration, etc.)
- Todos los Services (JsonDataSeeder, AuthService, etc.)
- IntegrationTests

**Impacto**: 
- Referencias rotas cuando se actualicen los namespaces
- Compilación fallará

**Acción Requerida**: 
- Actualizar todos los `using GesFer.Domain.Entities` → `using GesFer.Product.Back.Domain.Entities` (o `GesFer.Shared.Back.Domain.Entities` para entidades geográficas)
- Actualizar `using GesFer.Domain.Services` → `using GesFer.Product.Back.Domain.Services`

---

### 1.3 ❌ Entidades Admin en Product/Back

**Problema**: Las siguientes entidades están en Product/Back pero pertenecen al dominio Admin:

1. **AdminUser.cs** (`src/Product/Back/src/domain/Entities/AdminUser.cs`)
   - **Ubicación Correcta**: `src/Admin/Back/src/domain/Entities/AdminUser.cs`
   - **Namespace Correcto**: `GesFer.Admin.Back.Domain.Entities`

2. **AuditLog.cs** (`src/Product/Back/src/domain/Entities/AuditLog.cs`)
   - **Ubicación Correcta**: `src/Admin/Back/src/domain/Entities/AuditLog.cs`
   - **Namespace Correcto**: `GesFer.Admin.Back.Domain.Entities`

3. **Log.cs** (`src/Product/Back/src/domain/Entities/Log.cs`)
   - **Análisis**: Según el análisis previo, LogController es Admin exclusivo
   - **Ubicación Correcta**: `src/Admin/Back/src/domain/Entities/Log.cs`
   - **Namespace Correcto**: `GesFer.Admin.Back.Domain.Entities`
   - **Nota**: NO hereda de BaseEntity (requisito de Serilog)

**Impacto**: 
- Violación de la Ley de Invariabilidad Admin ↔ Cliente (Regla 14 de GOLDEN_RULES.md)
- Contaminación del dominio Product con entidades Admin

**Acción Requerida**: 
- Mover las 3 entidades a Admin/Back
- Actualizar ProductDbContext para referenciar desde Admin
- Actualizar todas las referencias

---

### 1.4 ❌ ProductDbContext con Referencias Incorrectas

**Problema**: `ProductDbContext.cs` tiene múltiples problemas:

1. **Using incorrecto** (línea 1):
   ```csharp
   using GesFer.Domain.Entities; // ❌ Namespace antiguo
   ```
   Debería ser:
   ```csharp
   using GesFer.Product.Back.Domain.Entities;
   using GesFer.Shared.Back.Domain.Entities;
   using GesFer.Admin.Back.Domain.Entities; // Cuando se muevan AdminUser, AuditLog, Log
   ```

2. **DbSets de entidades geográficas** (líneas 37-41):
   ```csharp
   public DbSet<Country> Countries => Set<Country>();
   public DbSet<Language> Languages => Set<Language>();
   public DbSet<State> States => Set<State>();
   public DbSet<City> Cities => Set<City>();
   public DbSet<PostalCode> PostalCodes => Set<PostalCode>();
   ```
   **Problema**: Estas entidades ahora están en Shared, pero ProductDbContext no tiene el using correcto.

3. **Referencias a Domain.Common.BaseEntity** (líneas 74, 79, 97, 102, 162):
   ```csharp
   typeof(Domain.Common.BaseEntity) // ❌ Referencia incorrecta
   ```
   Debería ser:
   ```csharp
   typeof(GesFer.Shared.Back.Domain.Common.BaseEntity)
   ```

**Impacto**: 
- Compilación fallará
- Referencias rotas

**Acción Requerida**: 
- Actualizar todos los usings
- Actualizar referencias a BaseEntity
- Agregar usings para entidades geográficas de Shared

---

### 1.5 ❌ Configuraciones EF Core con Namespace Antiguo

**Problema**: Todas las configuraciones de entidades geográficas usan el namespace antiguo:

- `CountryConfiguration.cs` → `using GesFer.Domain.Entities;` (línea 1)
- `StateConfiguration.cs` → `using GesFer.Domain.Entities;` (línea 1)
- `CityConfiguration.cs` → `using GesFer.Domain.Entities;` (línea 1)
- `PostalCodeConfiguration.cs` → `using GesFer.Domain.Entities;` (línea 1)
- `LanguageConfiguration.cs` → `using GesFer.Domain.Entities;` (línea 1)

**Impacto**: 
- Las configuraciones no encontrarán las entidades (están en Shared ahora)

**Acción Requerida**: 
- Actualizar a `using GesFer.Shared.Back.Domain.Entities;`

---

### 1.6 ❌ Referencias Inconsistentes a Entidades Geográficas

**Problema**: Hay inconsistencia en cómo se referencian las entidades geográficas:

1. **En Company.cs** (correcto):
   ```csharp
   public GesFer.Shared.Back.Domain.Entities.PostalCode? PostalCode { get; set; }
   ```

2. **En User.cs** (incorrecto):
   ```csharp
   public PostalCode? PostalCode { get; set; } // ❌ Falta namespace completo
   public City? City { get; set; }
   public State? State { get; set; }
   public Country? Country { get; set; }
   public Language? Language { get; set; }
   ```

**Impacto**: 
- Compilación fallará si no hay using correcto
- Inconsistencia en el código

**Acción Requerida**: 
- Agregar `using GesFer.Shared.Back.Domain.Entities;` en User.cs
- O usar namespace completo como en Company.cs

---

## 2. PROBLEMAS EN FRONTEND (TypeScript/React)

### 2.1 ❌ Componentes Duplicados en Product/Front

**Problema**: Los componentes `shared/` y `ui/` todavía existen en `src/Product/Front/components/` cuando ya están en `src/Shared/Front/components/`.

**Archivos Duplicados**:
- `src/Product/Front/components/shared/Button.tsx`
- `src/Product/Front/components/shared/Input.tsx`
- `src/Product/Front/components/shared/ModalBase.tsx`
- `src/Product/Front/components/shared/DataTable.tsx`
- `src/Product/Front/components/shared/DestructiveActionConfirm.tsx`
- `src/Product/Front/components/ui/button.tsx`
- `src/Product/Front/components/ui/card.tsx`
- `src/Product/Front/components/ui/dialog.tsx`
- `src/Product/Front/components/ui/error-message.tsx`
- `src/Product/Front/components/ui/input.tsx`
- `src/Product/Front/components/ui/label.tsx`
- `src/Product/Front/components/ui/loading.tsx`
- `src/Product/Front/components/ui/overlay-fix.tsx`

**Análisis**: 
- Estos componentes ahora importan desde `@shared`, pero siguen existiendo físicamente en Product/Front
- Esto crea confusión y duplicación innecesaria

**Acción Requerida**: 
- **OPCIÓN A (Recomendada)**: Eliminar los componentes duplicados de Product/Front
- **OPCIÓN B**: Mantenerlos como wrappers que re-exportan desde Shared (no recomendado, añade complejidad)

---

### 2.2 ❌ Tipos TypeScript de Entidades Geográficas en Product/Front

**Problema**: Interfaces de entidades geográficas están en Product/Front cuando deberían estar en Shared/Front.

**Archivo**: `src/Product/Front/lib/types/api.ts`

**Interfaces Encontradas**:
- `Country` (líneas 125-133)
- `City` (líneas 136-144)

**Impacto**: 
- Violación de separación de dominios
- Duplicación de tipos

**Acción Requerida**: 
- Mover `Country` y `City` a `src/Shared/Front/lib/types/api.ts`
- Actualizar importaciones en Product/Front

---

### 2.3 ⚠️ Referencia Residual en Documentación

**Problema**: Una referencia a `@/components/ui` en documentación:
- `src/Product/Front/README-TESTS.md`

**Impacto**: 
- Bajo (solo documentación)
- Puede confundir a desarrolladores

**Acción Requerida**: 
- Actualizar documentación para usar `@shared/components/ui`

---

## 3. ARCHIVOS QUE SE ESCAPARON DE LA MIGRACIÓN

### 3.1 Configuraciones EF Core de Entidades Geográficas

**Archivos que deberían estar en Shared/Back** (o al menos actualizar sus usings):

- `src/Product/Back/src/Infrastructure/Data/Configurations/CountryConfiguration.cs`
- `src/Product/Back/src/Infrastructure/Data/Configurations/StateConfiguration.cs`
- `src/Product/Back/src/Infrastructure/Data/Configurations/CityConfiguration.cs`
- `src/Product/Back/src/Infrastructure/Data/Configurations/PostalCodeConfiguration.cs`
- `src/Product/Back/src/Infrastructure/Data/Configurations/LanguageConfiguration.cs`

**Análisis**: 
- Estas configuraciones configuran entidades que ahora están en Shared
- **Decisión Arquitectónica Requerida**: ¿Dónde deben residir?
  - **OPCIÓN A**: Mover a `src/Shared/Back/src/Infrastructure/Data/Configurations/`
  - **OPCIÓN B**: Mantener en Product/Back pero actualizar usings

**Recomendación**: OPCIÓN A (mover a Shared) para mantener coherencia.

---

### 3.2 Handlers de Entidades Geográficas

**Archivos que manejan entidades geográficas** (deberían actualizar usings):

- `src/Product/Back/src/application/Handlers/Country/CreateCountryCommandHandler.cs`
- `src/Product/Back/src/application/Handlers/State/CreateStateCommandHandler.cs`
- `src/Product/Back/src/application/Handlers/City/CreateCityCommandHandler.cs`
- `src/Product/Back/src/application/Handlers/PostalCode/CreatePostalCodeCommandHandler.cs`

**Análisis**: 
- Estos handlers crean entidades geográficas que ahora están en Shared
- Ya usan namespace completo en la creación (`new GesFer.Shared.Back.Domain.Entities.Country`)
- Pero tienen `using GesFer.Domain.Entities;` que debería actualizarse

**Acción Requerida**: 
- Actualizar usings a `using GesFer.Shared.Back.Domain.Entities;`

---

## 4. ERRORES DE REFERENCIA DETECTADOS

### 4.1 ProductDbContext - Referencias a Entidades Geográficas

**Problema**: `ProductDbContext.cs` declara DbSets de entidades geográficas (líneas 37-41) pero:
- No tiene `using GesFer.Shared.Back.Domain.Entities;`
- Usa `using GesFer.Domain.Entities;` (namespace antiguo)

**Resultado**: Compilación fallará con error CS0246 (tipo no encontrado).

---

### 4.2 User.cs - Referencias a Entidades Geográficas sin Using

**Problema**: `User.cs` tiene propiedades de navegación a entidades geográficas (líneas 29-33):
```csharp
public PostalCode? PostalCode { get; set; }
public City? City { get; set; }
public State? State { get; set; }
public Country? Country { get; set; }
public Language? Language { get; set; }
```

Pero no tiene `using GesFer.Shared.Back.Domain.Entities;`.

**Resultado**: Compilación fallará con error CS0246.

---

### 4.3 Configuraciones EF Core - Referencias Rotas

**Problema**: Todas las configuraciones de entidades geográficas usan:
```csharp
using GesFer.Domain.Entities; // ❌ Namespace antiguo
public class CountryConfiguration : IEntityTypeConfiguration<Country>
```

Pero `Country` ahora está en `GesFer.Shared.Back.Domain.Entities`.

**Resultado**: Compilación fallará.

---

## 5. MATRIZ DE PROBLEMAS Y PRIORIDADES

| # | Problema | Severidad | Archivos Afectados | Acción Requerida |
|---|----------|-----------|-------------------|------------------|
| 1 | Namespaces antiguos en entidades | 🔴 CRÍTICO | 23 entidades | Actualizar a `GesFer.Product.Back.Domain.Entities` |
| 2 | Referencias al namespace antiguo | 🔴 CRÍTICO | 57 archivos | Actualizar todos los `using` |
| 3 | Entidades Admin en Product | 🔴 CRÍTICO | 3 entidades | Mover a Admin/Back |
| 4 | ProductDbContext incorrecto | 🔴 CRÍTICO | 1 archivo | Actualizar usings y referencias |
| 5 | Configuraciones EF Core incorrectas | 🔴 CRÍTICO | 5 archivos | Actualizar usings |
| 6 | Componentes duplicados Frontend | 🟡 MEDIO | 13 archivos | Eliminar duplicados |
| 7 | Tipos TypeScript en Product | 🟡 MEDIO | 1 archivo | Mover a Shared/Front |

---

## 6. PLAN DE CORRECCIÓN RECOMENDADO

### FASE 1: Corrección de Namespaces (CRÍTICO)

1. **Actualizar namespaces de entidades Product**:
   - Cambiar `namespace GesFer.Domain.Entities;` → `namespace GesFer.Product.Back.Domain.Entities;` en todas las entidades Product
   - Actualizar `namespace GesFer.Domain.Services;` → `namespace GesFer.Product.Back.Domain.Services;`

2. **Mover entidades Admin**:
   - Mover `AdminUser.cs`, `AuditLog.cs`, `Log.cs` a `src/Admin/Back/src/domain/Entities/`
   - Cambiar namespace a `GesFer.Admin.Back.Domain.Entities`

3. **Actualizar ProductDbContext**:
   - Agregar usings correctos
   - Actualizar referencias a BaseEntity
   - Actualizar DbSets para usar tipos correctos

### FASE 2: Actualización de Referencias (CRÍTICO)

1. **Actualizar todos los `using` en Product/Back**:
   - `using GesFer.Domain.Entities;` → `using GesFer.Product.Back.Domain.Entities;` o `using GesFer.Shared.Back.Domain.Entities;` según corresponda
   - `using GesFer.Domain.Services;` → `using GesFer.Product.Back.Domain.Services;`

2. **Actualizar configuraciones EF Core**:
   - Todas las configuraciones de entidades geográficas deben usar `using GesFer.Shared.Back.Domain.Entities;`

### FASE 3: Limpieza Frontend (MEDIO)

1. **Eliminar componentes duplicados**:
   - Eliminar `src/Product/Front/components/shared/` (completo)
   - Eliminar `src/Product/Front/components/ui/` (completo)

2. **Mover tipos TypeScript**:
   - Mover `Country` y `City` a `src/Shared/Front/lib/types/api.ts`
   - Actualizar importaciones

---

## 7. VERIFICACIÓN POST-CORRECCIÓN

Después de aplicar las correcciones, verificar:

1. ✅ Compilación exitosa de Product/Back (0 errores, 0 warnings)
2. ✅ Compilación exitosa de Shared/Back (0 errores, 0 warnings)
3. ✅ Compilación exitosa de Admin/Back (0 errores, 0 warnings)
4. ✅ No existen referencias a `GesFer.Domain.*` en Product/Back
5. ✅ No existen componentes duplicados en Product/Front
6. ✅ Shared no tiene dependencias de Product ni Admin

---

## 8. CONCLUSIÓN

La purificación de Shared se ha completado **parcialmente**. Se han identificado **7 categorías de problemas críticos** que deben corregirse antes de continuar con la migración de Admin.

**Estado Actual**: 🟡 **REQUIERE CORRECCIÓN**  
**Próximo Paso**: Aplicar correcciones de FASE 1 y FASE 2 (críticas) antes de continuar.

---

**FIN DEL INFORME DE AUDITORÍA**
