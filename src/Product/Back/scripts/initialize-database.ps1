# Script para inicializar la base de datos delegando a GesFer.Console
Write-Host "=== Inicialización de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host "Delegando operación a GesFer.Console..." -ForegroundColor Cyan

# Ejecutar GesFer.Console con la opción de inicialización de base de datos (Opción 3 / --step8)
# Nota: Usamos --initialize para asegurar todo el flujo, o podríamos usar --step8 para solo DB.
# Dado el nombre del script, parece más alineado con la inicialización completa.
dotnet run --project src/Console/GesFer.Console.csproj -- --initialize

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error durante la inicialización." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Inicialización completada exitosamente." -ForegroundColor Green
