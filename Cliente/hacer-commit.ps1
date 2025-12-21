# Script para hacer commit y push de los tests de integridad

Write-Host "========================================" -ForegroundColor Green
Write-Host "  COMMIT Y PUSH DE TESTS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Verificar que git esta disponible
try {
    $gitVersion = git --version 2>&1
    Write-Host "[OK] Git encontrado: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Git no esta disponible en el PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Por favor, ejecuta los siguientes comandos manualmente:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "cd C:\Proyectos\GesFer" -ForegroundColor Cyan
    Write-Host "git add Cliente/__tests__/integration/" -ForegroundColor Cyan
    Write-Host "git add Cliente/package.json" -ForegroundColor Cyan
    Write-Host 'git commit -m "Anadiendo test a cliente" -m "Descripcion completa..."' -ForegroundColor Cyan
    Write-Host "git push origin master" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

# Cambiar al directorio del proyecto
Set-Location "C:\Proyectos\GesFer"

Write-Host "Agregando archivos al staging..." -ForegroundColor Yellow
git add Cliente/__tests__/integration/
git add Cliente/package.json

Write-Host ""
Write-Host "Estado de los archivos:" -ForegroundColor Yellow
git status --short

Write-Host ""
Write-Host "Creando commit..." -ForegroundColor Yellow

$descripcion = "Se han anadido tests de integridad completos para auditar todas las funcionalidades del cliente:`n`n" +
               "Tests de integridad (integrity.test.tsx): 26 tests que cubren autenticacion, CRUD de usuarios y empresas, flujos completos, validaciones, manejo de errores y gestion de cache`n" +
               "Tests E2E (e2e-flows.test.tsx): 5 tests que verifican flujos completos de operaciones CRUD`n" +
               "Tests de contratos API (api-contracts.test.ts): 9 tests que validan interfaces y tipos`n" +
               "Total: 40 tests pasando correctamente`n" +
               "Scripts anadidos: test:integrity y test:all"

git commit -m "Anadiendo test a cliente" -m $descripcion

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[OK] Commit creado exitosamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "Haciendo push al repositorio remoto..." -ForegroundColor Yellow
    git push origin master
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "[OK] Push completado exitosamente" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "[ERROR] Error al hacer push" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "[ERROR] Error al crear commit" -ForegroundColor Red
}

Write-Host ""
