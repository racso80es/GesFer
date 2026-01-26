# Script de validación de datos de seed
# Verifica que:
# 1. Todos los datos de master-data.json están en test-data.json
# 2. Los tests utilizan datos existentes en test-data.json

$ErrorActionPreference = "Stop"

Write-Host "=== Validación de Datos de Seed ===" -ForegroundColor Cyan
Write-Host ""

$seedsPath = "src\Infrastructure\Data\Seeds"
$masterDataPath = Join-Path $seedsPath "master-data.json"
$testDataPath = Join-Path $seedsPath "test-data.json"

if (-not (Test-Path $masterDataPath)) {
    Write-Host "ERROR: No se encuentra master-data.json en $masterDataPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $testDataPath)) {
    Write-Host "ERROR: No se encuentra test-data.json en $testDataPath" -ForegroundColor Red
    exit 1
}

Write-Host "Cargando archivos JSON..." -ForegroundColor Yellow
$masterData = Get-Content $masterDataPath | ConvertFrom-Json
$testData = Get-Content $testDataPath | ConvertFrom-Json

$errors = @()
$warnings = @()

Write-Host ""
Write-Host "=== 1. Validación: Datos Maestros en test-data.json ===" -ForegroundColor Cyan
Write-Host ""

# Validar Languages
Write-Host "Validando Languages..." -ForegroundColor Yellow
$masterLanguages = @{}
$testLanguages = @{}

if ($masterData.languages) {
    foreach ($lang in $masterData.languages) {
        $masterLanguages[$lang.id] = $lang
    }
}

if ($testData.languages) {
    foreach ($lang in $testData.languages) {
        $testLanguages[$lang.id] = $lang
    }
}

$missingLanguages = @()
foreach ($langId in $masterLanguages.Keys) {
    if (-not $testLanguages.ContainsKey($langId)) {
        $missingLanguages += $langId
        $errors += "Language con ID $langId no está en test-data.json"
    }
}

if ($missingLanguages.Count -eq 0) {
    Write-Host "  ✓ Todos los Languages de master-data.json están en test-data.json" -ForegroundColor Green
} else {
    Write-Host "  ✗ Faltan $($missingLanguages.Count) Languages en test-data.json" -ForegroundColor Red
}

# Validar Permissions
Write-Host "Validando Permissions..." -ForegroundColor Yellow
$masterPermissions = @{}
$testPermissions = @{}

if ($masterData.permissions) {
    foreach ($perm in $masterData.permissions) {
        $masterPermissions[$perm.id] = $perm
    }
}

if ($testData.permissions) {
    foreach ($perm in $testData.permissions) {
        $testPermissions[$perm.id] = $perm
    }
}

$missingPermissions = @()
foreach ($permId in $masterPermissions.Keys) {
    if (-not $testPermissions.ContainsKey($permId)) {
        $missingPermissions += $permId
        $errors += "Permission con ID $permId ($($masterPermissions[$permId].key)) no está en test-data.json"
    }
}

if ($missingPermissions.Count -eq 0) {
    Write-Host "  ✓ Todos los Permissions de master-data.json están en test-data.json" -ForegroundColor Green
} else {
    Write-Host "  ✗ Faltan $($missingPermissions.Count) Permissions en test-data.json" -ForegroundColor Red
}

# Validar Groups
Write-Host "Validando Groups..." -ForegroundColor Yellow
$masterGroups = @{}
$testGroups = @{}

if ($masterData.groups) {
    foreach ($group in $masterData.groups) {
        $masterGroups[$group.id] = $group
    }
}

if ($testData.groups) {
    foreach ($group in $testData.groups) {
        $testGroups[$group.id] = $group
    }
}

$missingGroups = @()
foreach ($groupId in $masterGroups.Keys) {
    if (-not $testGroups.ContainsKey($groupId)) {
        $missingGroups += $groupId
        $errors += "Group con ID $groupId ($($masterGroups[$groupId].name)) no está en test-data.json"
    }
}

if ($missingGroups.Count -eq 0) {
    Write-Host "  ✓ Todos los Groups de master-data.json están en test-data.json" -ForegroundColor Green
} else {
    Write-Host "  ✗ Faltan $($missingGroups.Count) Groups en test-data.json" -ForegroundColor Red
}

# Validar GroupPermissions
Write-Host "Validando GroupPermissions..." -ForegroundColor Yellow
$masterGroupPermissions = @{}
$testGroupPermissions = @{}

if ($masterData.groupPermissions) {
    foreach ($gp in $masterData.groupPermissions) {
        $key = "$($gp.groupId)-$($gp.permissionId)"
        $masterGroupPermissions[$key] = $gp
    }
}

if ($testData.groupPermissions) {
    foreach ($gp in $testData.groupPermissions) {
        $key = "$($gp.groupId)-$($gp.permissionId)"
        $testGroupPermissions[$key] = $gp
    }
}

$missingGroupPermissions = @()
foreach ($key in $masterGroupPermissions.Keys) {
    if (-not $testGroupPermissions.ContainsKey($key)) {
        $gp = $masterGroupPermissions[$key]
        $missingGroupPermissions += $key
        $errors += "GroupPermission (GroupId: $($gp.groupId), PermissionId: $($gp.permissionId)) no está en test-data.json"
    }
}

if ($missingGroupPermissions.Count -eq 0) {
    Write-Host "  ✓ Todos los GroupPermissions de master-data.json están en test-data.json" -ForegroundColor Green
} else {
    Write-Host "  ✗ Faltan $($missingGroupPermissions.Count) GroupPermissions en test-data.json" -ForegroundColor Red
}

# Validar AdminUsers
Write-Host "Validando AdminUsers..." -ForegroundColor Yellow
$masterAdminUsers = @{}
$testAdminUsers = @{}

if ($masterData.adminUsers) {
    foreach ($au in $masterData.adminUsers) {
        $masterAdminUsers[$au.username] = $au
    }
}

if ($testData.adminUsers) {
    foreach ($au in $testData.adminUsers) {
        $testAdminUsers[$au.username] = $au
    }
}

$missingAdminUsers = @()
foreach ($username in $masterAdminUsers.Keys) {
    if (-not $testAdminUsers.ContainsKey($username)) {
        $missingAdminUsers += $username
        $errors += "AdminUser con username '$username' no está en test-data.json"
    }
}

if ($missingAdminUsers.Count -eq 0) {
    Write-Host "  ✓ Todos los AdminUsers de master-data.json están en test-data.json" -ForegroundColor Green
} else {
    Write-Host "  ✗ Faltan $($missingAdminUsers.Count) AdminUsers en test-data.json" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== 2. Validación: Datos usados en Tests ===" -ForegroundColor Cyan
Write-Host ""

# Validar datos específicos usados en tests
$testDataReferences = @{
    # Usuarios usados en tests
    "99999999-9999-9999-9999-999999999999" = @{ Type = "User"; Property = "Id"; UsedIn = "AuthControllerTests, UserControllerTests, SetupControllerTests" }
    "99999999-9999-9999-9999-999999999998" = @{ Type = "User"; Property = "Id"; UsedIn = "test-data.json" }
    "99999999-9999-9999-9999-999999999997" = @{ Type = "User"; Property = "Id"; UsedIn = "UserControllerTests.Update_WithValidData_ShouldReturnOk" }
    "99999999-9999-9999-9999-999999999996" = @{ Type = "User"; Property = "Id"; UsedIn = "UserControllerTests.Update_WithPassword_ShouldUpdatePassword" }
    
    # Empresas usadas en tests
    "11111111-1111-1111-1111-111111111111" = @{ Type = "Company"; Property = "Id"; UsedIn = "AuthControllerTests, UserControllerTests, CompanyControllerTests" }
    "11111111-1111-1111-1111-111111111112" = @{ Type = "Company"; Property = "Id"; UsedIn = "CompanyControllerTests.Update_WithValidData_ShouldReturnOk" }
    
    # Grupos usados en tests
    "22222222-2222-2222-2222-222222222222" = @{ Type = "Group"; Property = "Id"; UsedIn = "GroupControllerTests, AuthControllerTests" }
    "22222222-2222-2222-2222-222222222225" = @{ Type = "Group"; Property = "Id"; UsedIn = "GroupControllerTests.Update_WithValidData_ShouldReturnOk" }
    
    # AdminUsers usados en tests
    "aaaaaaaa-0000-0000-0000-000000000000" = @{ Type = "AdminUser"; Property = "Id"; UsedIn = "AdminAuthControllerTests, TestDataSeeder" }
}

$testStringReferences = @{
    # Usernames
    "admin" = @{ Type = "User"; Property = "Username"; UsedIn = "AuthControllerTests, UserControllerTests, SetupControllerTests" }
    "gestor" = @{ Type = "User"; Property = "Username"; UsedIn = "test-data.json" }
    
    # Company Names
    "Empresa Demo" = @{ Type = "Company"; Property = "Name"; UsedIn = "AuthControllerTests, UserControllerTests" }
    "Empresa Test Update" = @{ Type = "Company"; Property = "Name"; UsedIn = "CompanyControllerTests.Update_WithValidData_ShouldReturnOk" }
    
    # Group Names
    "Administradores" = @{ Type = "Group"; Property = "Name"; UsedIn = "GroupControllerTests, AuthControllerTests" }
    "Gestores" = @{ Type = "Group"; Property = "Name"; UsedIn = "test-data.json" }
    "Consultores" = @{ Type = "Group"; Property = "Name"; UsedIn = "test-data.json" }
    "Grupo Test Update" = @{ Type = "Group"; Property = "Name"; UsedIn = "GroupControllerTests.Update_WithValidData_ShouldReturnOk" }
    
    # Permission Keys
    "users.read" = @{ Type = "Permission"; Property = "Key"; UsedIn = "AuthControllerTests" }
    "users.write" = @{ Type = "Permission"; Property = "Key"; UsedIn = "AuthControllerTests" }
    "articles.read" = @{ Type = "Permission"; Property = "Key"; UsedIn = "AuthControllerTests" }
}

# Validar IDs de usuarios
Write-Host "Validando IDs de usuarios usados en tests..." -ForegroundColor Yellow
foreach ($userId in $testDataReferences.Keys) {
    $ref = $testDataReferences[$userId]
    if ($ref.Type -eq "User") {
        $found = $false
        if ($testData.users) {
            foreach ($user in $testData.users) {
                if ($user.id -eq $userId) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Usuario con ID $userId (usado en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Usuario ID $userId no encontrado" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Usuario ID $userId encontrado" -ForegroundColor Green
        }
    }
}

# Validar nombres de empresa
Write-Host "Validando nombres de empresa usados en tests..." -ForegroundColor Yellow
foreach ($companyName in $testStringReferences.Keys) {
    $ref = $testStringReferences[$companyName]
    if ($ref.Type -eq "Company") {
        $found = $false
        if ($testData.companies) {
            foreach ($company in $testData.companies) {
                if ($company.name -eq $companyName) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Empresa '$companyName' (usada en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Empresa '$companyName' no encontrada" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Empresa '$companyName' encontrada" -ForegroundColor Green
        }
    }
}

# Validar nombres de grupo
Write-Host "Validando nombres de grupo usados en tests..." -ForegroundColor Yellow
foreach ($groupName in $testStringReferences.Keys) {
    $ref = $testStringReferences[$groupName]
    if ($ref.Type -eq "Group") {
        $found = $false
        if ($testData.groups) {
            foreach ($group in $testData.groups) {
                if ($group.name -eq $groupName) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Grupo '$groupName' (usado en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Grupo '$groupName' no encontrado" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Grupo '$groupName' encontrado" -ForegroundColor Green
        }
    }
}

# Validar permisos
Write-Host "Validando permisos usados en tests..." -ForegroundColor Yellow
foreach ($permissionKey in $testStringReferences.Keys) {
    $ref = $testStringReferences[$permissionKey]
    if ($ref.Type -eq "Permission") {
        $found = $false
        if ($testData.permissions) {
            foreach ($perm in $testData.permissions) {
                if ($perm.key -eq $permissionKey) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Permiso '$permissionKey' (usado en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Permiso '$permissionKey' no encontrado" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Permiso '$permissionKey' encontrado" -ForegroundColor Green
        }
    }
}

# Validar IDs de grupos
Write-Host "Validando IDs de grupos usados en tests..." -ForegroundColor Yellow
foreach ($groupId in $testDataReferences.Keys) {
    $ref = $testDataReferences[$groupId]
    if ($ref.Type -eq "Group") {
        $found = $false
        if ($testData.groups) {
            foreach ($group in $testData.groups) {
                if ($group.id -eq $groupId) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Grupo con ID $groupId (usado en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Grupo ID $groupId no encontrado" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Grupo ID $groupId encontrado" -ForegroundColor Green
        }
    }
}

# Validar IDs de empresas
Write-Host "Validando IDs de empresas usados en tests..." -ForegroundColor Yellow
foreach ($companyId in $testDataReferences.Keys) {
    $ref = $testDataReferences[$companyId]
    if ($ref.Type -eq "Company") {
        $found = $false
        if ($testData.companies) {
            foreach ($company in $testData.companies) {
                if ($company.id -eq $companyId) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "Empresa con ID $companyId (usada en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ Empresa ID $companyId no encontrada" -ForegroundColor Red
        } else {
            Write-Host "  ✓ Empresa ID $companyId encontrada" -ForegroundColor Green
        }
    }
}

# Validar AdminUsers
Write-Host "Validando AdminUsers usados en tests..." -ForegroundColor Yellow
foreach ($adminUserId in $testDataReferences.Keys) {
    $ref = $testDataReferences[$adminUserId]
    if ($ref.Type -eq "AdminUser") {
        $found = $false
        if ($testData.adminUsers) {
            foreach ($au in $testData.adminUsers) {
                if ($au.id -eq $adminUserId) {
                    $found = $true
                    break
                }
            }
        }
        if (-not $found) {
            $errors += "AdminUser con ID $adminUserId (usado en: $($ref.UsedIn)) no está en test-data.json"
            Write-Host "  ✗ AdminUser ID $adminUserId no encontrado" -ForegroundColor Red
        } else {
            Write-Host "  ✓ AdminUser ID $adminUserId encontrado" -ForegroundColor Green
        }
    }
}

# Verificar que el usuario admin tiene el grupo Administradores asignado
Write-Host "Validando asignación de grupos a usuarios..." -ForegroundColor Yellow
$adminUserId = "99999999-9999-9999-9999-999999999999"
$adminGroupId = "22222222-2222-2222-2222-222222222222"
$adminHasGroup = $false

if ($testData.userGroups) {
    foreach ($ug in $testData.userGroups) {
        if ($ug.userId -eq $adminUserId -and $ug.groupId -eq $adminGroupId) {
            $adminHasGroup = $true
            break
        }
    }
}

if ($adminHasGroup) {
    Write-Host "  ✓ Usuario admin tiene grupo Administradores asignado" -ForegroundColor Green
} else {
    $warnings += "Usuario admin ($adminUserId) no tiene grupo Administradores ($adminGroupId) asignado en userGroups"
    Write-Host "  ⚠ Usuario admin no tiene grupo Administradores asignado" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host ""

if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "✓ Validación exitosa: Todos los datos están correctamente sincronizados" -ForegroundColor Green
    exit 0
} else {
    if ($errors.Count -gt 0) {
        Write-Host "✗ Se encontraron $($errors.Count) error(es):" -ForegroundColor Red
        foreach ($error in $errors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
    }
    
    if ($warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "⚠ Se encontraron $($warnings.Count) advertencia(s):" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "  - $warning" -ForegroundColor Yellow
        }
    }
    
    exit 1
}
