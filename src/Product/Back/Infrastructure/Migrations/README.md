# Migraciones EF Core – Product

## Migración única (InitialCreate)

Las migraciones se unificaron en una sola **InitialCreate** que refleja el modelo actual del `ApplicationDbContext` (tablas Product: Companies, Users, ArticleFamilies, TaxTypes, Families, Articles, etc.).

## Uso

```powershell
# Desde la carpeta Api
dotnet ef database update --project ../Infrastructure/GesFer.Infrastructure.csproj --context ApplicationDbContext

# Añadir una nueva migración (si cambias el modelo)
dotnet ef migrations add NombreMigracion --project ../Infrastructure/GesFer.Infrastructure.csproj --context ApplicationDbContext
```

## Bases de datos existentes

Si ya tenías una base de datos creada con migraciones anteriores (varias migraciones), tienes dos opciones:

1. **Entorno nuevo / desarrollo:** Eliminar la base de datos y volver a crearla con `dotnet ef database update`. La nueva InitialCreate creará todo el esquema.
2. **Conservar datos:** No elimines la BD. El historial de migraciones (`__EFMigrationsHistory`) tendrá nombres distintos; si el esquema ya está al día, puedes insertar manualmente una fila en `__EFMigrationsHistory` con el nombre de la nueva migración (`20260213141112_InitialCreate`) para que EF considere la BD actualizada. Revisa antes que el esquema coincida con el modelo.

## Tests

Los tests de integración usan `EnsureCreatedAsync()` (no migraciones), por lo que la unificación no los afecta.
