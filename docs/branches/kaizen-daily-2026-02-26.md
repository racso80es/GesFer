# Branch Documentation: kaizen/daily-2026-02-26

## Objective
Synchronize seed data for Tariffs and Financial Documents (Invoices, Delivery Notes) to satisfy Golden Rules compliance.

## Scope
- **Entities:** `Tariff`, `TariffItem`, `PurchaseInvoice`, `SalesInvoice`, `PurchaseDeliveryNote`, `SalesDeliveryNote`.
- **Files:** `JsonDataSeeder.cs`, `demo-data.json`, `test-data.json`.

## Plan
1. Implement seeding logic for Tariffs.
2. Implement seeding logic for Purchase/Sales documents.
3. Add sample data to JSON files.
4. Verify with `GoldenRulesComplianceService`.
