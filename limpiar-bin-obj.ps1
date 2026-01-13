# Script para limpiar todas las carpetas bin y obj del proyecto
# Esto fuerza que el único test-data.json válido sea el de Infrastructure/Data/Seeds/

Write-Host "=== Limpiando carpetas bin y obj del proyecto ===" -ForegroundColor Cyan

$projectRoot = $PSScriptRoot
$foldersDeleted = 0
$filesDeleted = 0

# Buscar todas las carpetas bin y obj, excluyendo node_modules
$binFolders = Get-ChildItem -Path $projectRoot -Directory -Recurse -Filter "bin" -ErrorAction SilentlyContinue | 
    Where-Object { $_.FullName -notmatch "node_modules" -and $_.FullName -match "\\Api\\" }
$objFolders = Get-ChildItem -Path $projectRoot -Directory -Recurse -Filter "obj" -ErrorAction SilentlyContinue | 
    Where-Object { $_.FullName -notmatch "node_modules" -and $_.FullName -match "\\Api\\" }

# Eliminar carpetas bin
foreach ($folder in $binFolders) {
    try {
        Write-Host "Eliminando: $($folder.FullName)" -ForegroundColor Yellow
        $fileCount = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue).Count
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        $foldersDeleted++
        $filesDeleted += $fileCount
        Write-Host "  OK - Eliminada ($fileCount archivos)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error al eliminar: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Eliminar carpetas obj
foreach ($folder in $objFolders) {
    try {
        Write-Host "Eliminando: $($folder.FullName)" -ForegroundColor Yellow
        $fileCount = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue).Count
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        $foldersDeleted++
        $filesDeleted += $fileCount
        Write-Host "  OK - Eliminada ($fileCount archivos)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error al eliminar: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "Carpetas eliminadas: $foldersDeleted" -ForegroundColor Green
Write-Host "Archivos eliminados: $filesDeleted" -ForegroundColor Green
Write-Host "`nLimpieza completada. El unico test-data.json valido ahora es: Api/src/Infrastructure/Data/Seeds/test-data.json" -ForegroundColor Green
