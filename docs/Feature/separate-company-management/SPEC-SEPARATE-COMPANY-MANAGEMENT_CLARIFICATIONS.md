# Clarificaciones Técnica: Separación de Gestión de Empresas

## 1. Dependencias de Arquitectura

### 1.1 Product -> Admin (Resuelto)
Se ha eliminado la referencia de proyecto `GesFer.Admin.Application` desde `GesFer.Infrastructure` (Product).
- **Solución:** Se han creado DTOs locales (`AdminCompanyDto`, `AdminUpdateCompanyDto`) en `Product.Back.Infrastructure.DTOs` para mapear las respuestas de la API de Admin.
- **Beneficio:** Desacoplamiento estricto. Product no necesita compilar Admin para funcionar.

### 1.2 Admin -> Product (Deuda Técnica)
`GesFer.Admin.Api` mantiene una referencia a `GesFer.Infrastructure` (Product).
- **Razón:** El `DashboardController` de Admin consume `ApplicationDbContext` para métricas globales. `ApplicationDbContext` reside en Product Infrastructure.
- **Decisión:** Se mantiene esta dependencia temporalmente.
- **Plan Futuro:** Extraer `ApplicationDbContext` o las interfaces de métricas a una capa `Shared.Infrastructure` o un módulo de persistencia común.

## 2. Autenticación Inter-servicios

### 2.1 Shared Secret
Se utiliza un `SharedSecret` (configurado en `appsettings.json`) para autenticar las llamadas desde Product a Admin.
- **Header:** `X-Internal-Secret`
- **Implementación Admin:** Atributo `[AuthorizeSystemOrAdmin]` que valida el header O el rol de Admin.
- **Implementación Product:** `AdminApiClient` inyecta `IConfiguration` y añade el header automáticamente si existe el secreto.

## 3. Modelo de Datos (Shared)

### 3.1 Entidad Company
La entidad `Company` base se ha movido a `GesFer.Shared.Back.Domain`.
- **Product:** Hereda de `Shared.Company` y añade sus propias colecciones de navegación (`Users`, `Articles`, etc.).
- **Admin:** Utiliza `Shared.Company` directamente.
- **Configuración:** Se ha duplicado la configuración de EF Core (`CompanyConfiguration`) en Admin para mapear correctamente los Value Objects (`TaxId`, `Email`) a la misma tabla física `Companies`.
