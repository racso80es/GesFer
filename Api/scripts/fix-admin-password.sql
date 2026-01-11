-- Script para corregir el hash de la contraseña del usuario admin
-- Ejecutar desde Adminer (http://localhost:8080) o desde línea de comandos
-- 
-- Este script actualiza el hash de la contraseña del usuario admin
-- para que coincida con el hash correcto de "admin123"

USE ScrapDb;

-- Hash BCrypt correcto para "admin123" (verificado)
-- Este hash debe coincidir con el usado en SetupService.cs y master-data.json
SET @correct_hash = '$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW';

-- Actualizar el hash del usuario admin si existe
UPDATE `Users` 
SET 
    PasswordHash = @correct_hash,
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP()
WHERE 
    Username = 'admin' 
    AND CompanyId = '11111111-1111-1111-1111-111111111111';

-- Verificar que se actualizó correctamente
SELECT 
    Username, 
    FirstName,
    LastName,
    IsActive,
    PasswordHash,
    CASE 
        WHEN PasswordHash = @correct_hash THEN '✓ Hash correcto'
        ELSE '✗ Hash incorrecto'
    END as HashStatus
FROM `Users` 
WHERE Username = 'admin';

-- También actualizar AdminUser si existe (para el login administrativo)
UPDATE `AdminUsers`
SET 
    PasswordHash = @correct_hash,
    IsActive = TRUE,
    UpdatedAt = UTC_TIMESTAMP()
WHERE 
    Username = 'admin';

-- Verificar AdminUser
SELECT 
    Username,
    Role,
    IsActive,
    PasswordHash,
    CASE 
        WHEN PasswordHash = @correct_hash THEN '✓ Hash correcto'
        ELSE '✗ Hash incorrecto'
    END as HashStatus
FROM `AdminUsers`
WHERE Username = 'admin';
