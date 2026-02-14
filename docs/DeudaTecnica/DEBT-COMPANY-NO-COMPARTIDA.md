# Deuda técnica: Company no debe estar compartida

## Identificador
**DEBT-COMPANY-NO-COMPARTIDA**

## Descripción
Actualmente la entidad **Company** (empresa/tenant) vive en **Shared** (`GesFer.Shared.Back.Domain.Entities.Company`). Admin y Product la referencian: Admin usa la entidad Shared directamente; Product extiende Shared.Company con navegaciones propias (Users, Articles, etc.).

**Deuda:** Company **no ha de estar compartida**. Cada bounded context debe tener su propio modelo de dominio:
- **Admin:** Su dominio y su modelo de Company (gestión de clientes/tenants, CRUD, tabla Companies).
- **Clientes API (Product):** Su dominio y su modelo de Company (o vista/DTO de “mi empresa”) para operaciones de negocio; consume datos vía API de Admin, sin depender de una entidad Company compartida en tiempo de compilación.

## Impacto
- Acoplamiento a Shared para un concepto que es responsabilidad de Admin (y consumo desde Product).
- Cambios en el modelo Company obligan a tocar Shared y pueden afectar a ambos dominios.
- Dificulta evolución independiente de Admin y Product.

## Criterios de resolución
- Admin define y posee su entidad Company (en Admin.Domain o equivalente) y la tabla Companies.
- Product no referencia una entidad Company de Shared; consume Company vía contrato (API Admin) y usa DTOs o modelos de lectura propios (ej. `AdminCompanyDto` ya existente).
- Shared deja de exponer `Company`; solo conserva lo verdaderamente común (ValueObjects, utilidades, etc.) si aplica.

## Relación con features
- **SPEC-COMPANY-MANAGED-BY-ADMIN:** La spec actual asume Company en Shared. Al saldar esta deuda, la spec se cumpliría con dominios separados (Admin con su Company, Product sin entidad Company compartida).
- **docs/Feature/separate-company-management/:** Misma dirección: Admin SSOT, Product consumidor.

## Prioridad
Media-alta. No bloquea la gestión actual (tabla en Admin, CRUD en Admin, Product consumidor), pero debe tenerse en cuenta en planificación y refactors.

## Fecha de registro
2026-02-14
