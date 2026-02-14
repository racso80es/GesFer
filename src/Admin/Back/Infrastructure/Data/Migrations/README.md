# Migraciones Admin (AdminDbContext)

## Orden de ejecución en nuevos entornos

En **nuevos entornos**, ejecutar las migraciones de Admin **antes** del primer arranque de la API Admin. Así la tabla `Logs` (y el resto de tablas Admin) existirán antes de que Serilog o los endpoints escriban en la BD.

```powershell
cd <raíz del repositorio>
dotnet ef database update --project src/Admin/Back/Infrastructure/GesFer.Admin.Infra.csproj
```

No usar `--startup-project` para evitar arrancar la API; se usa `IDesignTimeDbContextFactory<AdminDbContext>` y la connection string por defecto o la variable de entorno `ConnectionStrings__DefaultConnection`.

## Tablas creadas por Admin

- **AdminUsers**, **AuditLogs**: creadas por `InitialAdmin`.
- **Logs**: creada por `CreateLogsTableIfNotExists` si no existe; si la tabla ya existía (p. ej. por Serilog), `AddMissingColumnsToLogs` añade las columnas faltantes (Source, CompanyId, UserId) de forma idempotente.
- **Companies**: actualmente creada por las migraciones de Product; Admin es dueño lógico (seeds, CRUD). Ver `docs/Feature/company-managed-by-admin/`.
