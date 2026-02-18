# Objetivo de la Rama
Limpiar y estandarizar la infraestructura del backend eliminando scripts heredados redundantes y cumpliendo con las políticas de logging.

## Descripción
Esta rama se centra en la eliminación de scripts antiguos (`InitDatabase`, `GenerateHash`) que utilizaban `Console.WriteLine` de forma incorrecta, reemplazándolos por herramientas centralizadas como `GesFer.Console`. Además, se refactorizan tests de integración para usar métodos de logging apropiados (`Debug.WriteLine`).

## Acciones Realizadas
- Eliminación de `src/Product/Back/scripts/InitDatabase.cs` y su archivo de proyecto `.csproj` (redundante).
- Eliminación de `src/Product/Back/scripts/generate-password-hash.cs` y carpeta `GenerateHash` (legacy).
- Actualización de `src/Product/Back/scripts/recreate-database.ps1` para invocar a `GesFer.Console` con el argumento `--step8`.
- Refactorización de `IntegrationTestWebAppFactory.cs` para reemplazar `Console.WriteLine` con `System.Diagnostics.Debug.WriteLine`.
- Limpieza de referencias a proyectos eliminados en `GesFer.sln`.
