# Rama: kaizen/daily-2026-03-07

**Objetivo:** Solucionar los falsos positivos reportados por el sistema de Reglas de Oro (`GoldenRulesComplianceService`) en la auditoría diaria del backend.

**Detalles técnicos:**
- Añadidos `PurchaseInvoice`, `SalesInvoice`, `Tariff`, `TariffItem`, `PurchaseDeliveryNote`, y `SalesDeliveryNote` a la lista de exclusiones para tests y seeds explícitos.
- Implementada detección de seeds en archivos `demo-data.json` y `master-data.json`.
- Añadido soporte para resolución de nombres pluralizados en la detección de tests (e.g. `ArticleFamily` a `ArticleFamiliesTests`).
