# Objetivo: Estabilización de GesFer.Console

## Contexto
El proyecto `GesFer.Console` es crítico para la gestión del sistema, inicialización de datos y validación de reglas de oro. Actualmente no compila debido a errores en `GesFer.Infrastructure` relacionados con el acceso a la entidad `Company` a través de `ApplicationDbContext`.

## Análisis
- **Error Principal:** `CS1061 'ApplicationDbContext' does not contain a definition for 'Companies'`.
- **Causa Raíz:** Se eliminó o no se incluyó `DbSet<Company>` en `ApplicationDbContext.cs`.
- **Impacto:** Falla la compilación de `GesFer.Infrastructure` y, por ende, `GesFer.Console`.

## Plan de Acción
1.  **Restaurar DbSet:** Añadir `public DbSet<Company> Companies => Set<Company>();` en `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`.
2.  **Verificar Compilación:** Asegurar que `dotnet build src/Console/GesFer.Console.csproj` sea exitoso.
3.  **Verificar Ejecución:** Ejecutar la consola en modo no interactivo (`--validate`) para confirmar que el contenedor de inyección de dependencias se construye correctamente.

## Criterios de Aceptación
- [ ] `GesFer.Console` compila sin errores.
- [ ] `ApplicationDbContext` expone `Companies`.
- [ ] La consola puede ejecutarse y completar una tarea básica (e.g., validación o help).
