# Script para configurar la base de datos delegando a GesFer.Console
Write-Host "=== Configuración de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host "Delegando operación a GesFer.Console..." -ForegroundColor Cyan

# Ejecutar GesFer.Console con la opción de inicialización completa (--initialize)
dotnet run --project src/Console/GesFer.Console.csproj -- --initialize

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error durante la configuración." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Configuración completada exitosamente." -ForegroundColor Green
