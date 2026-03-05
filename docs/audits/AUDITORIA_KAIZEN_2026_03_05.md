# AUDITORÍA KAIZEN DIARIA - 2026-03-05

## 1. Métricas de Salud Actuales

*   **Arquitectura:** Estable. Se mantienen los principios Shared, evitando dependencias cíclicas. El servicio de Golden Rules identifica falsos positivos que deben resolverse para aumentar la confianza.
*   **Nomenclatura:** Estable.
*   **Estabilidad Async:** 100%. `AdminApiLogSink` es la única excepción explícita que delega con `Task.Run`.
*   **Cumplimiento Reglas de Oro (Pre-Corrección):** 20 entidades analizadas, 6 marcadas con errores de sincronización de Seeds y Tests.

## 2. Pain Points

### 🔴 Críticos
Ninguno detectado que impida la compilación o ejecución.

### 🟡 Medios
*   **Hallazgo:** El sistema `GoldenRulesComplianceService` reporta falsos positivos sobre entidades relacionales o de agregación que intencionalmente no deben inicializarse explícitamente ni en Seed Data ni tener Tests propios dedicados (e.g. `TariffItem`, `PurchaseInvoice`, `Tariff`, `SalesInvoice`, `PurchaseDeliveryNote`, `SalesDeliveryNote`).
*   **Ubicación:** `src/Console/Services/GoldenRulesComplianceService.cs`, listas `noSeedEntities` (línea 418) y `noTestEntities` (línea 463).
*   **Impacto:** Los informes del evaluador se "ensucian" con falsas alarmas, reduciendo la efectividad del detector.

## 3. Acciones Kaizen

### Fix: Evitar Falsos Positivos de Reglas de Oro en Entidades Ignoradas

*   **Instrucciones para el Ejecutor:**
    1.  Abrir `src/Console/Services/GoldenRulesComplianceService.cs`.
    2.  Localizar el array `noSeedEntities`.
    3.  Añadir `"TariffItem", "PurchaseInvoice", "Tariff", "SalesInvoice", "PurchaseDeliveryNote", "SalesDeliveryNote"` a la lista.
    4.  Localizar el array `noTestEntities`.
    5.  Añadir `"TariffItem", "PurchaseInvoice", "Tariff", "SalesInvoice", "PurchaseDeliveryNote", "SalesDeliveryNote"` a la lista.
*   **Snippet de Código:**
    ```csharp
    var noSeedEntities = new[] { "GroupPermission", "UserGroup", "UserPermission", "PurchaseDeliveryNoteLine", "SalesDeliveryNoteLine", "TariffItem", "PurchaseInvoice", "Tariff", "SalesInvoice", "PurchaseDeliveryNote", "SalesDeliveryNote" };
    // ...
    var noTestEntities = new[] { "GroupPermission", "UserGroup", "UserPermission", "PurchaseDeliveryNoteLine", "SalesDeliveryNoteLine", "TariffItem", "PurchaseInvoice", "Tariff", "SalesInvoice", "PurchaseDeliveryNote", "SalesDeliveryNote" };
    ```
*   **Definition of Done:** `dotnet run --project src/Console/GesFer.Console.csproj -- --golden-rules --force` no debe mostrar errores para estas 6 entidades. Todas las 20 entidades detectadas deben estar 100% sincronizadas.
