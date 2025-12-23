-- Script para insertar datos de prueba en GesFer API
-- Ejecutar desde Adminer (http://localhost:8080) o desde línea de comandos
-- 
-- IMPORTANTE: Ejecutar primero las migraciones de EF Core antes de insertar datos
-- Las migraciones crean las tablas necesarias

USE ScrapDb;

-- 0. Idiomas maestros
INSERT IGNORE INTO Languages (Id, Name, Code, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('10000000-0000-0000-0000-000000000001', 'Español', 'es', 'Español', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000002', 'English', 'en', 'Inglés', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000003', 'Català', 'ca', 'Catalán', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- 1. Insertar una empresa (Company)
INSERT INTO `Companies` (Id, Name, TaxId, Address, Phone, Email, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
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
);

-- 2. Insertar un grupo de usuarios
INSERT INTO `Groups` (Id, Name, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'Administradores',
    'Grupo de administradores del sistema',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- 3. Insertar permisos de ejemplo
INSERT INTO `Permissions` (Id, `Key`, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('33333333-3333-3333-3333-333333333333', 'users.read', 'Ver usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('44444444-4444-4444-4444-444444444444', 'users.write', 'Crear/editar usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('55555555-5555-5555-5555-555555555555', 'articles.read', 'Ver artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('66666666-6666-6666-6666-666666666666', 'articles.write', 'Crear/editar artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('77777777-7777-7777-7777-777777777777', 'purchases.read', 'Ver compras', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('88888888-8888-8888-8888-888888888888', 'purchases.write', 'Crear/editar compras', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- 4. Asignar permisos al grupo
INSERT INTO `GroupPermissions` (Id, GroupId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333333', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', '44444444-4444-4444-4444-444444444444', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555555', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', '22222222-2222-2222-2222-222222222222', '66666666-6666-6666-6666-666666666666', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- 5. Insertar un usuario de prueba
-- Contraseña: "admin123" 
-- Hash BCrypt generado (puede variar, pero este es válido para "admin123")
INSERT INTO `Users` (Id, CompanyId, Username, PasswordHash, FirstName, LastName, Email, Phone, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '99999999-9999-9999-9999-999999999999',
    '11111111-1111-1111-1111-111111111111',
    'admin',
    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', -- admin123
    'Administrador',
    'Sistema',
    'admin@empresa.com',
    '912345678',
    '10000000-0000-0000-0000-000000000001',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- 6. Asignar usuario al grupo
INSERT INTO `UserGroups` (Id, UserId, GroupId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    '99999999-9999-9999-9999-999999999999',
    '22222222-2222-2222-2222-222222222222',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- 7. Asignar un permiso directo al usuario (opcional)
INSERT INTO `UserPermissions` (Id, UserId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    '99999999-9999-9999-9999-999999999999',
    '77777777-7777-7777-7777-777777777777',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);
