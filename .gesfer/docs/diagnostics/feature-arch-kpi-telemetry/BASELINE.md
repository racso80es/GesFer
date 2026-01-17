# Baseline de Salud del Sistema - FASE 1: Sistema de KPIs

**Rama**: `feature/arch-kpi-telemetry`  
**Fecha de Inicio**: 2025-01-27  
**Estado**: IN_PROGRESS  
**Objetivo**: Establecer el baseline inicial de salud del sistema mediante 3 métricas sagradas

## Contexto

Esta fase corresponde al "Baseline de Salud del Sistema" donde se establecen los valores iniciales de las 3 métricas sagradas que servirán como punto de partida para medir la evolución del sistema.

## Métricas Sagradas - Valores Baseline

### 1. Índice de Sincronización: **53.8%**
- **Entidades Totales**: 26 entidades que heredan de `BaseEntity`
- **DTOs Principales (Response)**: 14 DTOs de respuesta
- **Cálculo**: (14 / 26) × 100 = 53.8%
- **Hallazgo**: Hay 12 entidades sin DTOs principales de respuesta (Article, Family, Tariff, TariffItem, PurchaseDeliveryNote, PurchaseDeliveryNoteLine, SalesDeliveryNote, SalesDeliveryNoteLine, PurchaseInvoice, SalesInvoice, UserGroup, UserPermission, GroupPermission, AdminUser, AuditLog)

### 2. Densidad Kaizen: **0 → 5 (detectado)**
- **Valor Inicial**: 0
- **Basura Técnica Detectada**: 5 usos de `confirm()` que deberían ser `DestructiveActionConfirm` según Vision Zero
  - `Cliente/app/(client)/usuarios/page.tsx`: línea 141
  - `Cliente/app/(client)/empresas/page.tsx`: línea 91
  - `Cliente/app/(client)/clientes/page.tsx`: línea 33
  - `Cliente/app/[locale]/clientes/page.tsx`: línea 34
  - `Cliente/app/[locale]/empresas/page.tsx`: línea 98
- **Nota**: Esta basura técnica no cumple con las reglas de Vision Zero que requieren usar `DestructiveActionConfirm` para acciones destructivas.

### 3. Inmunidad de Test: **0%**
- **Tests Totales**: 16 archivos de test
- **Tests con data-test-id**: 0
- **Cálculo**: (0 / 16) × 100 = 0%
- **Hallazgo**: Ningún test frontend utiliza `data-test-id` como selector estable, lo que reduce la inmunidad a cambios en el HTML/CSS.

## Detalles de Auditoría

### Entidades Identificadas (26):
1. User
2. AdminUser
3. AuditLog
4. Language
5. Company
6. Country
7. Supplier
8. Customer
9. PostalCode
10. City
11. State
12. GroupPermission
13. PurchaseDeliveryNote
14. Article
15. Family
16. PurchaseInvoice
17. PurchaseDeliveryNoteLine
18. Tariff
19. TariffItem
20. SalesDeliveryNoteLine
21. SalesDeliveryNote
22. SalesInvoice
23. UserPermission
24. Permission
25. UserGroup
26. Group

### DTOs Principales Identificados (14):
1. UserDto
2. CompanyDto
3. CustomerDto
4. SupplierDto
5. CountryDto
6. CityDto
7. StateDto
8. PostalCodeDto
9. GroupDto
10. LoginResponseDto
11. AdminLoginResponseDto
12. DashboardSummaryDto
13. LogDto
14. LogsPagedResponseDto / PurgeLogsResponseDto

### Archivos de Test Identificados (16):
- `Cliente/__tests__/app/login/page.test.tsx`
- `Cliente/__tests__/app/usuarios/page.test.tsx`
- `Cliente/__tests__/components/ui/button.test.tsx`
- `Cliente/__tests__/components/ui/input.test.tsx`
- `Cliente/__tests__/integration/e2e-flows.test.tsx`
- `Cliente/__tests__/integration/integrity.test.tsx`
- `Cliente/__tests__/integration/system-integrity.test.ts`
- `Cliente/__tests__/integration/id-validation.test.ts`
- `Cliente/__tests__/integration/users-companies-integrity.test.ts`
- `Cliente/__tests__/integration/language-id-integrity.test.ts`
- `Cliente/__tests__/integration/api-contracts.test.ts`
- `Cliente/__tests__/lib/api/companies.test.ts`
- `Cliente/__tests__/lib/api/client.test.ts`
- `Cliente/__tests__/lib/api/id-validation-api.test.ts`
- `Cliente/__tests__/lib/utils/id-validation.test.ts`
- `Cliente/__tests__/utils/cn.test.ts`

## Basura Técnica Detectada

### Código que no cumple Vision Zero (5 instancias):
- **Tipo**: Uso de `confirm()` nativo en lugar de `DestructiveActionConfirm`
- **Impacto**: Viola las reglas de Vision Zero que requieren confirmación explícita mediante componente dedicado
- **Ubicaciones**:
  1. `Cliente/app/(client)/usuarios/page.tsx:141` - Eliminación de usuario
  2. `Cliente/app/(client)/empresas/page.tsx:91` - Eliminación de empresa
  3. `Cliente/app/(client)/clientes/page.tsx:33` - Eliminación de cliente
  4. `Cliente/app/[locale]/clientes/page.tsx:34` - Eliminación de cliente (versión localizada)
  5. `Cliente/app/[locale]/empresas/page.tsx:98` - Eliminación de empresa (versión localizada)

**Acción Recomendada**: Implementar componente `DestructiveActionConfirm` y reemplazar los 5 usos de `confirm()`. Esto aumentará la Densidad Kaizen a 5.

## Próximos Pasos

1. ✅ Auditoría inicial completa
2. ✅ Cálculo de valores baseline
3. ✅ Identificación de basura técnica
4. ⏳ Limpieza inicial (Densidad Kaizen) - Requiere implementar `DestructiveActionConfirm`
5. ✅ Documentación de hallazgos
