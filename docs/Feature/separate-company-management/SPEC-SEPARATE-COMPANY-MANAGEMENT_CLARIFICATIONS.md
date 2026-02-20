# Clarificaciones Técnica: Separación de Gestión de Empresas

## 1. Dependencias de Arquitectura

### 1.1 Product -> Admin (Resuelto)
Se ha eliminado la referencia de proyecto `GesFer.Admin.Application` desde `GesFer.Infrastructure` (Product).
- **Solución:** Se han creado DTOs locales (`AdminCompanyDto`, `AdminUpdateCompanyDto`) en `Product.Back.Infrastructure.DTOs` para mapear las respuestas de la API de Admin.
- **Beneficio:** Desacoplamiento estricto. Product no necesita compilar Admin para funcionar.

### 1.2 Admin -> Product (Resuelto)
Se ha eliminado la referencia de proyecto de `GesFer.Admin.Api` (y de `GesFer.Admin.Infra`) a `GesFer.Infrastructure` (Product).
- **Solución:** El `DashboardController` de Admin ya no usa `ProductDbContext` de Product. Las métricas propias de Admin (p. ej. TotalCompanies) se obtienen de `AdminDbContext`. Las métricas de Product (Users, Articles, Suppliers, Customers) se obtienen vía **HTTP** mediante `IProductApiClient` / `ProductApiClient`, que llama a `GET api/dashboard/stats` de la Product API (autenticación por header `X-Internal-Secret`). No existe dependencia en tiempo de compilación Admin → Product.
- **Beneficio:** Frontera de dominio respetada; comunicación entre dominios solo por contrato HTTP.

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
