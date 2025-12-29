USE ScrapDb;

-- Actualizar el hash de contraseña del usuario admin
-- Contraseña: admin123
-- Hash BCrypt completo y verificado
UPDATE Users 
SET PasswordHash = '$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW'
WHERE Username = 'admin';

-- Verificar que se actualizó correctamente
SELECT Username, 
       LEFT(PasswordHash, 10) as HashStart, 
       LENGTH(PasswordHash) as HashLength,
       PasswordHash
FROM Users 
WHERE Username = 'admin';

