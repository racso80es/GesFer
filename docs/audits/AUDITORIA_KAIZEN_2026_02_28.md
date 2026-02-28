# Auditoría Kaizen - 2026-02-28

## Estado Actual
El sistema `GesFer.Console` compila correctamente. La verificación de integridad "Reglas de Oro" (`--golden-rules`) reporta que 6 entidades requieren atención porque no están sincronizadas en Seeds y Tests:
- `TariffItem`
- `PurchaseInvoice`
- `Tariff`
- `SalesInvoice`
- `PurchaseDeliveryNote`
- `SalesDeliveryNote`

### 1. Métricas de Salud (Backend)
- **Compilación:** OK.
- **Golden Rules Check:** Warning (6 entidades reportadas como no sincronizadas).

### 2. Pain Points Identificados
- **Falsos Positivos / Falta de Sincronización en Golden Rules:**
  - El servicio `JsonDataSeeder.cs` no contiene las clases anidadas necesarias para la deserialización de estas 6 entidades (`TariffSeed`, `PurchaseInvoiceSeed`, etc.), causando un falso negativo/positivo en la comprobación de los Seeds.
  - La lógica de coincidencia de nombres de tests (`CheckTestsSyncAsync`) en `GoldenRulesComplianceService.cs` no es suficientemente flexible para detectar tests con sufijos o prefijos comunes (ej: tests agrupados bajo `DeliveryNote`), y es muy sensible a los plurales o agrupación por concepto de dominio.

### 3. Acciones Kaizen (Por Realizar)
- **Prioridad Alta:** Actualizar `JsonDataSeeder.cs` para incluir las clases anidadas requeridas para `Tariff`, `TariffItem`, facturas y albaranes.
- **Prioridad Media:** Mejorar la lógica de búsqueda de tests en `GoldenRulesComplianceService.cs` para contemplar mapeos más inteligentes (ej: `*DeliveryNote*Tests.cs` en lugar de exigir `PurchaseDeliveryNote` exacto, y manejo de plurales).
- **Prioridad Baja:** Si siguen faltando tests específicos, generarlos o crear un issue para ello en el Kaizen Backlog.
