# Seeds para Dominio Admin

Esta carpeta contiene los archivos de seed data para el dominio Admin. **Admin es SSOT (Source of Truth) para Companies.**

## Archivos

- **admin-users.json**: Usuarios administrativos del sistema.
- **companies.json**: Empresas (Companies). Cargado por `AdminJsonDataSeeder.SeedCompaniesAsync()` usando `AdminDbContext`.

## Orden de ejecución (BD compartida)

En entornos donde Admin y Product comparten la misma base de datos:

1. **Ejecutar primero los seeds de Admin** (companies y admin-users), para que la tabla `Companies` exista y esté poblada.
2. **Después**, ejecutar los seeds de Product (Languages, Users, etc.). El seed de Product ya no inserta companies; obtiene los `validCompanyIds` desde la BD (companies creadas por Admin).

Así se garantiza que las empresas existan antes de que Product cree usuarios, proveedores o clientes que referencien `CompanyId`.

## Niveles de Seed

### Master
Datos administrativos esenciales del sistema:
- Usuarios administrativos base
- Configuraciones globales
- Permisos administrativos

### Demo
Datos de demostración:
- Usuarios admin de ejemplo
- Logs de auditoría de ejemplo
- Configuraciones de demo

### Test
Datos para testing:
- Usuarios admin de prueba
- Logs de auditoría de prueba
- Configuraciones de test

## Uso

Los seeds se cargan mediante el servicio de seeding correspondiente, que debe ser configurado en el `Program.cs` o mediante un servicio de inicialización.
