# Tech Stack - GesFer

## Backend (.NET)

### Framework y Runtime
- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Framework web
- **Entity Framework Core 8.0** - ORM
- **Pomelo.EntityFrameworkCore.MySql** - Proveedor MySQL para EF Core

### Logging
- **Serilog** - Framework de logging estructurado
- **Serilog.Sinks.Console** - Sink para consola (desarrollo)
- **Serilog.Sinks.MySQL** - Sink para persistencia en base de datos (producción)
- **Serilog.Sinks.Async** - Sink asíncrono para mejor rendimiento
- **Serilog.AspNetCore** - Integración con ASP.NET Core

#### Configuración de Logging

**Desarrollo (appsettings.Development.json):**
- Nivel mínimo: Debug
- Destino: Consola (con formato estructurado)
- Sink asíncrono habilitado

**Producción (appsettings.Production.json):**
- Nivel mínimo: Information
- Destino: Base de datos MySQL (tabla Logs)
- Sink asíncrono habilitado
- Solo logs de Information y superiores se persisten

#### Sistema de Logging Centralizado

El sistema implementa logging estructurado y persistente:

1. **Backend (.NET):**
   - Serilog configurado con sinks de Consola (dev) y MySQL (prod)
   - Logs estructurados con enriquecimiento de contexto
   - Persistencia automática en base de datos

2. **Frontend (Next.js):**
   - Pino como logger centralizado
   - Filtrado por NODE_ENV (debug en dev, info en prod)
   - Transportador automático que envía logs de error/warning al endpoint de telemetría

3. **Telemetría:**
   - Endpoint POST `/api/telemetry/logs` para recibir logs del frontend
   - Mapeo automático de niveles numéricos de Pino a LogEventLevel de Serilog
   - Procesamiento mediante Serilog.Log.Write()

4. **Administración:**
   - UI de administración en `/sistemas/logs`
   - Filtros por fecha, nivel, empresa y usuario
   - Paginación para mantener rendimiento
   - Visualización expandible de JSON para detalles adicionales

### Base de Datos
- **MySQL 8.0** - Base de datos relacional
- **Sequential GUIDs** - Generación optimizada de IDs
- **Soft Delete** - Eliminación lógica global

### Autenticación y Seguridad
- **JWT (JSON Web Tokens)** - Autenticación basada en tokens
- **BCrypt** - Hashing de contraseñas
- **RBAC (Role-Based Access Control)** - Control de acceso basado en roles

### Caché
- **Memcached** - Sistema de caché distribuido

## Frontend (Next.js)

### Framework
- **Next.js 14+** - Framework React con App Router
- **TypeScript** - Tipado estático
- **React 18+** - Biblioteca UI

### Estilos
- **Tailwind CSS** - Framework CSS utilitario
- **Shadcn/UI** - Componentes UI (estilo)

### Estado y Datos
- **TanStack Query (React Query)** - Gestión de estado del servidor
- **Next-Auth** - Autenticación
- **next-intl** - Internacionalización

### Logging
- **Pino** - Logger estructurado y rápido
- **pino-pretty** - Formateo legible para desarrollo
- **Transportador personalizado** - Envío automático de logs de error/warning al backend

### Iconos
- **Lucide React** - Biblioteca de iconos

## Infraestructura

### Contenedores
- **Docker** - Contenedorización
- **Docker Compose** - Orquestación de servicios

### Servicios
- **MySQL 8.0** - Base de datos
- **Memcached** - Caché
- **Adminer** - Interfaz web para MySQL

## Arquitectura

### Backend
- **Clean Architecture** - Separación en capas (Domain, Application, Infrastructure, API)
- **CQRS Pattern** - Separación de comandos y consultas
- **Repository Pattern** - Abstracción de acceso a datos
- **Dependency Injection** - Inversión de dependencias

### Frontend
- **Component-Based Architecture** - Arquitectura basada en componentes
- **Server Components / Client Components** - Renderizado híbrido de Next.js
- **Protected Routes** - Rutas protegidas con autenticación

## Internacionalización

- **next-intl** - Soporte multi-idioma
- Idiomas soportados: Español (es), Inglés (en), Catalán (ca)

## Testing

### Backend
- **xUnit** - Framework de testing
- **Integration Tests** - Tests de integración con WebApplicationFactory

### Frontend
- **Jest** - Framework de testing
- **React Testing Library** - Testing de componentes React
- **Playwright** - Tests end-to-end
