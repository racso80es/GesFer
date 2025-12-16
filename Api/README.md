# GesFer API - Sistema de Gestión de Compra/Venta de Chatarra

API RESTful desarrollada en .NET 8.0 para la gestión de un negocio de compra/venta de chatarra.

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** y **SOLID**, organizado en capas:

- **Domain**: Entidades de dominio y servicios de dominio
- **Application**: DTOs y servicios de aplicación
- **Infrastructure**: Acceso a datos (EF Core), repositorios y servicios de infraestructura
- **Api**: Controladores REST y configuración de la API

## 🚀 Tecnologías

- **.NET 8.0**
- **Entity Framework Core 8.0** (Code First)
- **MySQL 8.0** (con UTF8MB4)
- **Memcached** (para caché)
- **Docker & Docker Compose**
- **Swagger/OpenAPI**

## 📋 Requisitos Previos

- .NET 8.0 SDK
- Docker Desktop (para MySQL y Memcached)
- Visual Studio 2022 o Visual Studio Code

## 🔧 Configuración Inicial

### 1. Iniciar servicios con Docker

```bash
docker-compose up -d
```

Esto iniciará:
- MySQL en el puerto 3306
- Memcached en el puerto 11211
- Adminer (opcional) en el puerto 8080 para gestión visual de BD

### 2. Crear la base de datos

```bash
# Desde la raíz del proyecto
cd src/Api
dotnet ef migrations add InitialCreate --project ../Infrastructure/GesFer.Infrastructure.csproj
dotnet ef database update --project ../Infrastructure/GesFer.Infrastructure.csproj
```

### 3. Ejecutar la API

```bash
cd src/Api
dotnet run
```

O desde Visual Studio:
1. Abrir `GesFer.sln`
2. Establecer `GesFer.Api` como proyecto de inicio
3. Presionar F5

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger` o `https://localhost:5001/swagger`

## 📁 Estructura del Proyecto

```
Api/
├── src/
│   ├── Api/                    # Proyecto API (Controladores, Program.cs)
│   ├── application/            # Capa de aplicación (DTOs, Servicios)
│   ├── domain/                 # Capa de dominio (Entidades, Servicios de dominio)
│   └── Infrastructure/          # Capa de infraestructura (EF Core, Repositorios)
├── docker-compose.yml          # Configuración de Docker
└── GesFer.sln                  # Solución de Visual Studio
```

## 🔐 Autenticación

El sistema implementa autenticación multi-tenant con RBAC (Role-Based Access Control).

### Endpoint de Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "empresa": "NombreEmpresa",
  "usuario": "usuario",
  "contraseña": "password"
}
```

### Respuesta

```json
{
  "userId": "guid",
  "username": "usuario",
  "firstName": "Nombre",
  "lastName": "Apellido",
  "companyId": "guid",
  "companyName": "NombreEmpresa",
  "permissions": ["permiso1", "permiso2"],
  "token": ""
}
```

## 📊 Módulos Principales

### 1. Autenticación y Seguridad
- Login multi-tenant (Empresa + Usuario + Contraseña)
- Sistema RBAC con permisos directos y por grupos
- Cálculo automático de permisos combinados

### 2. Inventario y Catálogo
- **Family**: Familias de artículos con % IVA
- **Article**: Artículos con código único, precios y stock

### 3. Tarifas
- **Tariff**: Tarifas de compra/venta
- **TariffItem**: Precios específicos por artículo en cada tarifa

### 4. Terceros
- **Supplier**: Proveedores con tarifa de compra opcional
- **Customer**: Clientes con tarifa de venta opcional

### 5. Operaciones de Compra
- **PurchaseDeliveryNote**: Albaranes de compra (aumentan stock)
- **PurchaseInvoice**: Facturas de compra

### 6. Operaciones de Venta
- **SalesDeliveryNote**: Albaranes de venta (disminuyen stock)
- **SalesInvoice**: Facturas de venta

## 🎯 Características Implementadas

✅ **Soft Delete** global en todas las entidades  
✅ **Multi-tenant** con CompanyId en todas las entidades de negocio  
✅ **Precisión decimal** decimal(18,4) para todos los importes  
✅ **UTF8** configurado para soportar caracteres especiales  
✅ **Gestión automática de stock** en albaranes  
✅ **Cálculo automático de precios** desde tarifas  
✅ **Cálculo automático de IVA** según familia del artículo  
✅ **RBAC** con permisos directos y por grupos  

## 🔍 Endpoints Disponibles

### Health Check
- `GET /api/health` - Verifica el estado de la API

### Autenticación
- `POST /api/auth/login` - Login de usuario
- `GET /api/auth/permissions/{userId}` - Obtener permisos de usuario

## 📝 Notas Importantes

1. **Stock**: Se actualiza automáticamente al crear albaranes:
   - Albaranes de compra → Aumentan stock
   - Albaranes de venta → Disminuyen stock (con validación previa)

2. **Precios**: Se calculan en este orden:
   - Precio del DTO (si se proporciona)
   - Precio de la tarifa del proveedor/cliente
   - Precio base del artículo

3. **IVA**: Se calcula automáticamente según el porcentaje de la familia del artículo

4. **Soft Delete**: Todas las eliminaciones son lógicas (no físicas)

## 🛠️ Desarrollo

### Agregar una nueva migración

```bash
cd src/Api
dotnet ef migrations add NombreMigracion --project ../Infrastructure/GesFer.Infrastructure.csproj
```

### Aplicar migraciones

```bash
dotnet ef database update --project ../Infrastructure/GesFer.Infrastructure.csproj
```

## 📚 Próximos Pasos

- [ ] Implementar JWT para autenticación
- [ ] Agregar validación con FluentValidation
- [ ] Implementar logging con Serilog
- [ ] Crear tests unitarios e integración
- [ ] Agregar más controladores (Articles, Suppliers, Customers, etc.)
- [ ] Implementar paginación en endpoints de listado
- [ ] Agregar filtros y búsqueda avanzada

## 📄 Licencia

Este proyecto es privado y de uso interno.


