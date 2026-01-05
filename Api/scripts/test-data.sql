-- ============================================
-- DATOS DE PRUEBA PARA TESTS DE INTEGRACIÓN
-- ============================================
-- Este script contiene datos específicos para realizar tests de integridad:
-- - Usuarios de prueba con credenciales conocidas
-- - Clientes y proveedores de prueba
-- - Datos con IDs fijos para tests determinísticos
--
-- IMPORTANTE: 
-- - Este script requiere que master-data.sql haya sido ejecutado primero
-- - Este script puede ejecutarse independientemente de sample-data.sql
-- - Los datos de este script están diseñados para ser limpiados y recreados en cada test
-- ============================================

USE ScrapDb;

-- ============================================
-- 1. EMPRESA DE PRUEBA
-- ============================================
-- Empresa específica para tests (puede coexistir con la empresa demo)
INSERT INTO Companies (Id, Name, TaxId, Address, Phone, Email, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Empresa Demo',
    'B12345678',
    'Calle Demo 123',
    '912345678',
    'demo@empresa.com',
    '10000000-0000-0000-0000-000000000001',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
)
ON DUPLICATE KEY UPDATE
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP();

-- ============================================
-- 2. USUARIOS DE PRUEBA
-- ============================================
-- Usuario admin para tests
-- Contraseña: "admin123"
-- Hash BCrypt: $2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW
INSERT INTO Users (Id, CompanyId, Username, PasswordHash, FirstName, LastName, Email, Phone, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '99999999-9999-9999-9999-999999999999',
    '11111111-1111-1111-1111-111111111111',
    'admin',
    '$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW',
    'Administrador',
    'Sistema',
    'admin@empresa.com',
    '912345678',
    '10000000-0000-0000-0000-000000000001',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
)
ON DUPLICATE KEY UPDATE
    PasswordHash = '$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW',
    IsActive = TRUE,
    DeletedAt = NULL,
    CompanyId = '11111111-1111-1111-1111-111111111111',
    UpdatedAt = UTC_TIMESTAMP();

-- ============================================
-- 3. ASIGNACIÓN DE USUARIOS A GRUPOS (PARA TESTS)
-- ============================================
-- Asignar admin al grupo de Administradores
INSERT IGNORE INTO UserGroups (Id, UserId, GroupId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    '99999999-9999-9999-9999-999999999999',
    '22222222-2222-2222-2222-222222222222',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- Asignar permiso directo al usuario (para tests)
INSERT IGNORE INTO UserPermissions (Id, UserId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    '99999999-9999-9999-9999-999999999999',
    '77777777-7777-7777-7777-777777777777', -- purchases.read
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- ============================================
-- 4. PROVEEDORES DE PRUEBA
-- ============================================
-- Proveedores con IDs fijos para tests determinísticos
INSERT INTO Suppliers (Id, CompanyId, Name, TaxId, Address, Phone, Email, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'aaaaaaaa-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Proveedor Test 1',
        'B11111111',
        'Calle Proveedor 1',
        '911111111',
        'proveedor1@test.com',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'bbbbbbbb-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Proveedor Test 2',
        'B22222222',
        'Calle Proveedor 2',
        '922222222',
        'proveedor2@test.com',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    )
ON DUPLICATE KEY UPDATE
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP();

-- ============================================
-- 5. CLIENTES DE PRUEBA
-- ============================================
-- Clientes con IDs fijos para tests determinísticos
INSERT INTO Customers (Id, CompanyId, Name, TaxId, Address, Phone, Email, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'cccccccc-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Cliente Test 1',
        'B33333333',
        'Calle Cliente 1',
        '933333333',
        'cliente1@test.com',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'dddddddd-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Cliente Test 2',
        'B44444444',
        'Calle Cliente 2',
        '944444444',
        'cliente2@test.com',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    )
ON DUPLICATE KEY UPDATE
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP();

-- ============================================
-- NOTAS PARA TESTS
-- ============================================
-- Este script está diseñado para:
-- 1. Proporcionar datos con IDs fijos y conocidos para tests determinísticos
-- 2. Incluir solo los datos mínimos necesarios para ejecutar tests de integración
-- 3. Ser fácilmente limpiable y recreable antes de cada suite de tests
--
-- Los tests de integración deberían:
-- - Limpiar estos datos antes de ejecutarse (o usar transacciones)
-- - Usar estos IDs fijos para hacer aserciones determinísticas
-- - No depender de datos de sample-data.sql
-- ============================================

