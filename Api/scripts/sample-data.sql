-- ============================================
-- DATOS DE MUESTRA
-- ============================================
-- Este script contiene datos de muestra para demostración y desarrollo:
-- - Empresa demo
-- - Usuarios de ejemplo
-- - Clientes de muestra
-- - Proveedores de muestra
-- - Artículos, familias y tarifas (si aplica)
--
-- IMPORTANTE: Este script requiere que master-data.sql haya sido ejecutado primero
-- ============================================

USE ScrapDb;

-- ============================================
-- 1. EMPRESA DEMO
-- ============================================
-- Empresa de demostración con datos completos
INSERT INTO Companies (Id, Name, TaxId, Address, Phone, Email, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Empresa Demo',
    'B12345678',
    'Calle Gran Vía, 1',
    '912345678',
    'demo@empresa.com',
    '10000000-0000-0000-0000-000000000001', -- Español
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
-- 2. USUARIOS DE MUESTRA
-- ============================================
-- Usuario administrador
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

-- Usuario gestor de ejemplo
-- Contraseña: "gestor123"
-- Hash BCrypt: $2a$11$K8vJ8vJ8vJ8vJ8vJ8vJ8vO8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ
-- NOTA: Este hash es un ejemplo, debe generarse con BCrypt
INSERT INTO Users (Id, CompanyId, Username, PasswordHash, FirstName, LastName, Email, Phone, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    '99999999-9999-9999-9999-999999999998',
    '11111111-1111-1111-1111-111111111111',
    'gestor',
    '$2a$11$K8vJ8vJ8vJ8vJ8vJ8vJ8vO8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ8vJ',
    'Gestor',
    'Ejemplo',
    'gestor@empresa.com',
    '912345679',
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
-- 3. ASIGNACIÓN DE USUARIOS A GRUPOS
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

-- Asignar gestor al grupo de Gestores
INSERT IGNORE INTO UserGroups (Id, UserId, GroupId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeed',
    '99999999-9999-9999-9999-999999999998',
    '22222222-2222-2222-2222-222222222223',
    UTC_TIMESTAMP(),
    NULL,
    NULL,
    TRUE
);

-- Asignar permiso directo al usuario admin (ejemplo)
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
-- 4. CLIENTES DE MUESTRA
-- ============================================
-- Clientes de ejemplo para demostración
INSERT INTO Customers (Id, CompanyId, Name, TaxId, Address, Phone, Email, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'cccccccc-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Cliente Madrid S.L.',
        'B11111111',
        'Calle Alcalá, 45',
        '915555555',
        'contacto@clientemadrid.es',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'dddddddd-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Comercial Bilbao',
        'A22222222',
        'Gran Vía, 8',
        '944123456',
        'info@comercialbilbao.es',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'eeeeeeee-3333-3333-3333-333333333333',
        '11111111-1111-1111-1111-111111111111',
        'Distribuidora Málaga',
        'B33333333',
        'Calle Larios, 12',
        '952123456',
        'ventas@distribuidoramalaga.es',
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
-- 5. PROVEEDORES DE MUESTRA
-- ============================================
-- Proveedores de ejemplo para demostración
INSERT INTO Suppliers (Id, CompanyId, Name, TaxId, Address, Phone, Email, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'aaaaaaaa-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Proveedor Barcelona S.A.',
        'B44444444',
        'Avenida Diagonal, 100',
        '934567890',
        'compras@proveedorbarcelona.es',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'bbbbbbbb-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Suministros Valencia',
        'A55555555',
        'Calle Colón, 25',
        '963456789',
        'info@suministrosvalencia.es',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'cccccccc-3333-3333-3333-333333333333',
        '11111111-1111-1111-1111-111111111111',
        'Distribuidora Sevilla',
        'B66666666',
        'Avenida de la Constitución, 5',
        '954123456',
        'pedidos@distribuidorasevilla.es',
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
-- 6. FAMILIAS DE ARTÍCULOS (si existe la tabla)
-- ============================================
-- Familias de productos de ejemplo
-- NOTA: Descomentar si la tabla Families existe
/*
INSERT INTO Families (Id, CompanyId, Name, Description, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'f1111111-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'Electrónica',
        'Productos electrónicos y componentes',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'f2222222-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Herramientas',
        'Herramientas manuales y eléctricas',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    )
ON DUPLICATE KEY UPDATE
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP();
*/

-- ============================================
-- 7. ARTÍCULOS DE MUESTRA (si existe la tabla)
-- ============================================
-- Artículos de ejemplo para demostración
-- NOTA: Descomentar si la tabla Articles existe y ajustar según el esquema
/*
INSERT INTO Articles (Id, CompanyId, Code, Name, Description, Price, Stock, FamilyId, CreatedAt, UpdatedAt, DeletedAt, IsActive)
VALUES 
    (
        'a1111111-1111-1111-1111-111111111111',
        '11111111-1111-1111-1111-111111111111',
        'ART-001',
        'Artículo de Muestra 1',
        'Descripción del artículo de muestra 1',
        19.99,
        100,
        'f1111111-1111-1111-1111-111111111111',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    ),
    (
        'a2222222-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'ART-002',
        'Artículo de Muestra 2',
        'Descripción del artículo de muestra 2',
        29.99,
        50,
        'f2222222-2222-2222-2222-222222222222',
        UTC_TIMESTAMP(),
        NULL,
        NULL,
        TRUE
    )
ON DUPLICATE KEY UPDATE
    IsActive = TRUE,
    DeletedAt = NULL,
    UpdatedAt = UTC_TIMESTAMP();
*/

-- ============================================
-- FIN DEL SCRIPT DE DATOS DE MUESTRA
-- ============================================

