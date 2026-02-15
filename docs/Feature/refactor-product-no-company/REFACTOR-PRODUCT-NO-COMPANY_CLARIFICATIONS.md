# Clarificaciones: REFACTOR Product sin Company

## 1. Rutas documentales (Knowledge Architect)

### 1.1 Canonical path
Según `openspecs/agents/knowledge-architect.json`:
- **Map [SPEC] -> openspecs/** para índices y referencias
- **featurePath -> docs/features/** para documentación de features/refactors

La ubicación canónica de esta refactor es: **`docs/Feature/refactor-product-no-company/`** (siguiendo el patrón de `company-managed-by-admin`).

### 1.2 Estructura de documentos
| Documento | Ubicación |
|-----------|-----------|
| Spec principal | `docs/Feature/refactor-product-no-company/SPEC-REFACTOR-PRODUCT-NO-COMPANY.md` |
| Fase Objetivos | `docs/Feature/refactor-product-no-company/REFACTOR-PRODUCT-NO-COMPANY-PHASE-OBJECTIVES.md` |
| Clarificaciones | `docs/Feature/refactor-product-no-company/REFACTOR-PRODUCT-NO-COMPANY_CLARIFICATIONS.md` |

---

## 2. Relación con deuda técnica

### 2.1 DEBT-COMPANY-NO-COMPARTIDA
El refactor **saldaría** la deuda técnica registrada en `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md`:
- Product deja de depender de Shared.Company
- Product usa exclusivamente DTO desde Admin API
- Admin mantiene entidad Company en su dominio

### 2.2 SPEC-COMPANY-MANAGED-BY-ADMIN
Este refactor es la **implementación concreta** del apartado 3.2 (Product consumidor) de SPEC-COMPANY-MANAGED-BY-ADMIN. La SPEC describe el estado objetivo; este refactor lo ejecuta en Product.

---

## 3. Decisiones técnicas

### 3.1 Tabla Companies en Product
| Decisión | Detalle |
|----------|---------|
| ¿Product migra la tabla Companies? | No. La tabla es propiedad de Admin. Si está creada por migraciones Product por razones históricas, el plan de migración evaluará: (a) mover creación a Admin si aún no está, (b) no crear nueva migración en Product que modifique Companies. |
| ¿Product tiene FK hacia Companies? | Sí. Entidades Product (User, Article, etc.) mantienen `CompanyId` como FK. La tabla Companies existe (creada por Admin) y Product la referencia por integridad referencial. |

### 3.2 Navegación Company en entidades
| Decisión | Detalle |
|----------|---------|
| ¿Include(c => c.Company)? | No. Eliminar navegación. Si se necesita nombre de empresa para UI, obtener vía `IAdminApiClient.GetCompanyAsync(companyId)`. |
| ¿Entidad Company en Product? | No. Eliminar `Product.Back.Domain.Entities.Company`. |

### 3.3 Rutas Frontend Product
| Ruta | Decisión |
|------|----------|
| `/my-company` (Mi Organización) | Mantener. Consume `/api/my-company` (proxy a Admin). |
| `/companies` (listado) | Eliminar o migrar a Admin Front. Product no gestiona lista de empresas. |
| `/companies/[id]` | Eliminar o migrar a Admin Front. Product no gestiona CRUD de empresas. |

### 3.4 Login y validación de empresa
| Decisión | Detalle |
|----------|---------|
| ¿Validar empresa en Admin antes de emitir token? | Sí. Login debe validar que la empresa existe vía Admin API (o Shared Secret) antes de emitir JWT con `company_id`. |
| ¿Fallback si Admin API no disponible? | Definir en plan: timeout, reintentos, comportamiento en mantenimiento. |

---

## 4. Tests a actualizar/eliminar

| Tests | Acción |
|-------|--------|
| CompanyCommandTests, CompanyControllerTests | Eliminar (handlers y controller eliminados) |
| MyCompanyControllerTests | Mantener (usa MockAdminApiClient) |
| Handlers que validan Company | Reemplazar validación `_context.Companies` por `IAdminApiClient.GetCompanyAsync`; ajustar mocks |
| E2E / integración con companiesApi | Eliminar o migrar a Admin Front |

---

## 5. Resumen de decisiones

| Tema | Decisión |
|------|----------|
| Ruta canónica | docs/Feature/refactor-product-no-company/ |
| Entidad Company en Product | Eliminar completamente |
| Navegación Company | Eliminar; usar IAdminApiClient cuando se necesiten datos de empresa |
| Tabla Companies | Propiedad Admin; Product mantiene CompanyId como FK |
| Frontend /companies | Eliminar o migrar a Admin |
| Login | Validar empresa en Admin antes de emitir token |

---

## 6. Trazabilidad

- **SPEC:** SPEC-REFACTOR-PRODUCT-NO-COMPANY
- **Base:** SPEC-COMPANY-MANAGED-BY-ADMIN
- **Deuda:** docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md
- **Feature relacionada:** docs/Feature/company-managed-by-admin/
