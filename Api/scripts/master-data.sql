-- ============================================
-- DATOS MAESTROS
-- ============================================
-- Este script contiene los datos maestros del sistema:
-- - Idiomas
-- - Permisos base del sistema
-- - Grupos base del sistema
-- - Localizaciones (países, estados, ciudades, códigos postales)
--
-- IMPORTANTE: Este script debe ejecutarse PRIMERO antes que sample-data.sql y test-data.sql
-- ============================================

USE ScrapDb;

-- ============================================
-- 1. IDIOMAS MAESTROS
-- ============================================
-- Idiomas soportados por el sistema
INSERT IGNORE INTO Languages (Id, Name, Code, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('10000000-0000-0000-0000-000000000001', 'Español', 'es', 'Español', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000002', 'English', 'en', 'Inglés', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('10000000-0000-0000-0000-000000000003', 'Català', 'ca', 'Catalán', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 2. PERMISOS BASE DEL SISTEMA
-- ============================================
-- Permisos fundamentales que definen las capacidades del sistema
INSERT IGNORE INTO Permissions (Id, `Key`, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    -- Permisos de usuarios
    ('33333333-3333-3333-3333-333333333333', 'users.read', 'Ver usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('44444444-4444-4444-4444-444444444444', 'users.write', 'Crear/editar usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('44444445-4444-4444-4444-444444444444', 'users.delete', 'Eliminar usuarios', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de artículos
    ('55555555-5555-5555-5555-555555555555', 'articles.read', 'Ver artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('66666666-6666-6666-6666-666666666666', 'articles.write', 'Crear/editar artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('66666667-6666-6666-6666-666666666666', 'articles.delete', 'Eliminar artículos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de compras
    ('77777777-7777-7777-7777-777777777777', 'purchases.read', 'Ver compras', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('88888888-8888-8888-8888-888888888888', 'purchases.write', 'Crear/editar compras', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('88888889-8888-8888-8888-888888888888', 'purchases.delete', 'Eliminar compras', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de ventas
    ('99999999-9999-9999-9999-999999999991', 'sales.read', 'Ver ventas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('99999999-9999-9999-9999-999999999992', 'sales.write', 'Crear/editar ventas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('99999999-9999-9999-9999-999999999993', 'sales.delete', 'Eliminar ventas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de clientes
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'customers.read', 'Ver clientes', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', 'customers.write', 'Crear/editar clientes', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'customers.delete', 'Eliminar clientes', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de proveedores
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'suppliers.read', 'Ver proveedores', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'suppliers.write', 'Crear/editar proveedores', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'suppliers.delete', 'Eliminar proveedores', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de empresas
    ('cccccccc-cccc-cccc-cccc-cccccccccc01', 'companies.read', 'Ver empresas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02', 'companies.write', 'Crear/editar empresas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('cccccccc-cccc-cccc-cccc-cccccccccc03', 'companies.delete', 'Eliminar empresas', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    
    -- Permisos de grupos
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'groups.read', 'Ver grupos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'groups.write', 'Crear/editar grupos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'groups.delete', 'Eliminar grupos', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 3. GRUPOS BASE DEL SISTEMA
-- ============================================
-- Grupos de usuarios predefinidos
INSERT IGNORE INTO `Groups` (Id, Name, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    ('22222222-2222-2222-2222-222222222222', 'Administradores', 'Grupo de administradores del sistema con todos los permisos', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('22222222-2222-2222-2222-222222222223', 'Gestores', 'Grupo de gestores con permisos de lectura y escritura', UTC_TIMESTAMP(), NULL, NULL, TRUE),
    ('22222222-2222-2222-2222-222222222224', 'Consultores', 'Grupo de consultores con permisos de solo lectura', UTC_TIMESTAMP(), NULL, NULL, TRUE);

-- ============================================
-- 4. ASIGNACIÓN DE PERMISOS A GRUPOS BASE
-- ============================================
-- Asignar todos los permisos al grupo de Administradores
INSERT IGNORE INTO GroupPermissions (Id, GroupId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
SELECT 
    UUID() as Id,
    '22222222-2222-2222-2222-222222222222' as GroupId,
    Id as PermissionId,
    UTC_TIMESTAMP() as CreatedAt,
    NULL as UpdatedAt,
    NULL as DeletedAt,
    TRUE as IsActive
FROM Permissions
WHERE DeletedAt IS NULL AND IsActive = TRUE
AND NOT EXISTS (
    SELECT 1 FROM GroupPermissions 
    WHERE GroupId = '22222222-2222-2222-2222-222222222222' 
    AND PermissionId = Permissions.Id
);

-- Asignar permisos de lectura al grupo de Consultores
INSERT IGNORE INTO GroupPermissions (Id, GroupId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
SELECT 
    UUID() as Id,
    '22222222-2222-2222-2222-222222222224' as GroupId,
    Id as PermissionId,
    UTC_TIMESTAMP() as CreatedAt,
    NULL as UpdatedAt,
    NULL as DeletedAt,
    TRUE as IsActive
FROM Permissions
WHERE `Key` LIKE '%.read'
AND DeletedAt IS NULL AND IsActive = TRUE
AND NOT EXISTS (
    SELECT 1 FROM GroupPermissions 
    WHERE GroupId = '22222222-2222-2222-2222-222222222224' 
    AND PermissionId = Permissions.Id
);

-- Asignar permisos de lectura y escritura al grupo de Gestores
INSERT IGNORE INTO GroupPermissions (Id, GroupId, PermissionId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
SELECT 
    UUID() as Id,
    '22222222-2222-2222-2222-222222222223' as GroupId,
    Id as PermissionId,
    UTC_TIMESTAMP() as CreatedAt,
    NULL as UpdatedAt,
    NULL as DeletedAt,
    TRUE as IsActive
FROM Permissions
WHERE (`Key` LIKE '%.read' OR `Key` LIKE '%.write')
AND DeletedAt IS NULL AND IsActive = TRUE
AND NOT EXISTS (
    SELECT 1 FROM GroupPermissions 
    WHERE GroupId = '22222222-2222-2222-2222-222222222223' 
    AND PermissionId = Permissions.Id
);

-- ============================================
-- NOTA SOBRE LOCALIZACIONES
-- ============================================
-- Las localizaciones (países, estados, ciudades, códigos postales) se cargan
-- automáticamente mediante el servicio MasterDataSeeder en el código C#.
-- Este servicio carga los datos de España y sus provincias, ciudades y códigos postales.
-- 
-- Si necesitas cargar localizaciones manualmente, puedes usar el endpoint de setup
-- o ejecutar el servicio MasterDataSeeder desde el código.
-- ============================================

