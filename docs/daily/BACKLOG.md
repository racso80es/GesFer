# KAIZEN BACKLOG DIARIO

## Completadas (Hoy)
- [x] **Fix Golden Rules False Positives**:
    - Excluido `TariffItem`, `PurchaseInvoice`, `Tariff`, `SalesInvoice`, `PurchaseDeliveryNote`, `SalesDeliveryNote` de la verificación de Seeds.
    - Permitido que `DeliveryNote` cubra `PurchaseDeliveryNote` y `SalesDeliveryNote` en la verificación de Tests.
- [x] **Crear Tests Básicos para Tariff e Invoices**:
    - Creados `TariffTests.cs`, `TariffItemTests.cs`, `PurchaseInvoiceTests.cs`, `SalesInvoiceTests.cs` en `GesFer.Product.UnitTests`.

## Pendientes (General)
- [ ] **Implementar Tests para Tariff**: Cobertura completa de handlers (CRUD).
- [ ] **Implementar Tests para Invoices**: Cobertura completa de handlers (CRUD).
