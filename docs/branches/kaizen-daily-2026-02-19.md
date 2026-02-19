# Objetivo de la Rama

La rama `kaizen/daily-2026-02-19` tiene como objetivo corregir los falsos positivos detectados por la herramienta de cumplimiento de reglas de oro (`GoldenRulesComplianceService`) en `GesFer.Console` y añadir la cobertura de tests unitarios faltantes para entidades transaccionales básicas.

## Descripción

Actualmente, la herramienta de auditoría automatizada reporta errores de sincronización de Seeds y Tests para entidades como `Tariff`, `Invoice` y `DeliveryNote`.
1.  **Seeds:** Estas entidades son transaccionales y no deberían estar en el `JsonDataSeeder` (Master Data), por lo que la herramienta falla incorrectamente al no encontrarlas.
2.  **Tests:** La convención de nombres de tests era demasiado estricta, no detectando tests compartidos (ej. `DeliveryNoteIvaCalculationTests` cubriendo `PurchaseDeliveryNote` y `SalesDeliveryNote`).

## Acciones Realizadas

1.  **Corrección de GoldenRulesComplianceService:**
    *   Se han excluido `TariffItem`, `PurchaseInvoice`, `Tariff`, `SalesInvoice`, `PurchaseDeliveryNote`, `SalesDeliveryNote` de la verificación de Seeds en `JsonDataSeeder`.
    *   Se ha actualizado la lógica de búsqueda de tests para aceptar patrones como `*DeliveryNote*Tests.cs` para las entidades de albaranes.

2.  **Implementación de Tests Unitarios:**
    *   Se han creado tests básicos de construcción y propiedades para:
        *   `Tariff` y `TariffItem`
        *   `PurchaseInvoice` y `SalesInvoice`
    *   Esto eleva la cobertura y cumple con el requisito de existencia de tests de la auditoría.
