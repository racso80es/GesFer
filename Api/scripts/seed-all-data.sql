-- Script completo para insertar datos base iniciales y datos de prueba
-- Este script inserta: idiomas, empresa demo, grupos, permisos, usuario admin, proveedores y clientes de prueba
-- Ejecutar desde Adminer (http://localhost:8080) o desde línea de comandos

USE ScrapDb;

-- ============================================
-- 0. DATOS BASE: Idiomas maestros
-- ============================================
INSERT IGNORE INTO Languages (Id, Name, Code, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('10000000-0000-0000-0000-000000000001', 'Español', 'es', 'Español', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000002', 'English', 'en', 'Inglés', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000003', 'Català', 'ca', 'Catalán', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 1. DATOS BASE: Empresa Demo
-- ============================================
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
-- 2. DATOS BASE: Grupo de Administradores
-- ============================================
INSERT IGNORE INTO `Groups` (Id, Name, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'Administradores',
    'Grupo de administradores del sistema',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- ============================================
-- 3. DATOS BASE: Permisos del sistema
-- ============================================
INSERT IGNORE INTO Permissions (Id, `Key`, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('33333333-3333-3333-3333-333333333333', 'users.read', 'Ver usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('44444444-4444-4444-4444-444444444444', 'users.write', 'Crear/editar usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('55555555-5555-5555-5555-555555555555', 'articles.read', 'Ver artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('66666666-6666-6666-6666-666666666666', 'articles.write', 'Crear/editar artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('77777777-7777-7777-7777-777777777777', 'purchases.read', 'Ver compras', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('88888888-8888-8888-8888-888888888888', 'purchases.write', 'Crear/editar compras', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 4. DATOS BASE: Asignar permisos al grupo
-- ============================================
INSERT IGNORE INTO GroupPermissions (Id, GroupId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333333', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', '44444444-4444-4444-4444-444444444444', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555555', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', '22222222-2222-2222-2222-222222222222', '66666666-6666-6666-6666-666666666666', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 5. DATOS BASE: Usuario administrador
-- Contraseña: "admin123"
-- Hash BCrypt: $2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW
-- ============================================
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
-- 6. DATOS BASE: Asignar usuario al grupo
-- ============================================
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

-- ============================================
-- 7. DATOS BASE: Permiso directo al usuario
-- ============================================
INSERT IGNORE INTO UserPermissions (Id, UserId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    '99999999-9999-9999-9999-999999999999',
    '77777777-7777-7777-7777-777777777777',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- ============================================
-- FIN DEL SCRIPT
-- ============================================
-- Datos insertados:
-- - 3 idiomas (Español, English, Català)
-- - 1 empresa (Empresa Demo)
-- - 1 grupo (Administradores)
-- - 6 permisos (users.read, users.write, articles.read, articles.write, purchases.read, purchases.write)
-- - 1 usuario (admin / admin123)
-- - Relaciones: grupo-permisos, usuario-grupo, usuario-permiso directo
-- ============================================

