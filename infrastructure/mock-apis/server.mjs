/**
 * Servidor de APIs mock para GesFer
 *
 * Simula Product API (puerto 5002) y Admin API (puerto 5012) para:
 * - Validar clientes (frontends) cuando no hay acceso a las APIs reales
 * - Testear las APIs como cliente (p. ej. Playwright tests/api contra mock)
 *
 * Contratos alineados con:
 * - Product: POST /api/auth/login, GET/POST/DELETE /api/user (ver PROPUESTA_CORRECCION_MOCK_USUARIOS.md)
 * - Admin:   POST /api/admin/auth/login (Usuario, Contraseña)
 *
 * Seguridad (agente seguridad): validación de entrada en POST /api/user; solo datos ficticios.
 */

import express from 'express';
import { randomUUID } from 'crypto';

const PORT_PRODUCT = parseInt(process.env.MOCK_PORT_PRODUCT || '5002', 10);
const PORT_ADMIN = parseInt(process.env.MOCK_PORT_ADMIN || '5012', 10);

const productOnly = process.argv.includes('--product-only');
const adminOnly = process.argv.includes('--admin-only');

// Credenciales mock aceptadas (mismo formato que seeds de desarrollo)
const MOCK_PRODUCT = { empresa: 'Empresa Demo', usuario: 'admin', contraseña: 'admin123' };
const MOCK_ADMIN = { usuario: 'admin', contraseña: 'admin' };

// GUID válido: formato 8-4-4-4-12 (acepta cualquier UUID y NIL para compatibilidad con tests/APIs)
const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
function isValidGuid(id) {
  return typeof id === 'string' && GUID_REGEX.test(id);
}

// Validación email básica (seguridad: no aceptar cualquier string como email)
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
function isValidEmail(str) {
  return typeof str === 'string' && str.length > 0 && EMAIL_REGEX.test(str);
}

// Usuario stub para GET list / GET by id (datos ficticios, sin PII real)
const STUB_USER_ID = '00000000-0000-0000-0000-000000000003';
function stubUser(overrides = {}) {
  return {
    id: overrides.id ?? STUB_USER_ID,
    companyId: '00000000-0000-0000-0000-000000000002',
    companyName: 'Empresa Demo',
    username: 'mock-user',
    firstName: 'Mock',
    lastName: 'User',
    email: 'mock-user@mock.local',
    phone: null,
    isActive: true,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

function productApp() {
  const app = express();
  app.use(express.json());

  app.get('/health', (_, res) => {
    res.json({ status: 'healthy', mock: true, service: 'product' });
  });

  app.get('/api/health', (_, res) => {
    res.json({ status: 'healthy', mock: true, service: 'product' });
  });

  app.post('/api/auth/login', (req, res) => {
    const { empresa, usuario, contraseña } = req.body || {};
    if (
      empresa === MOCK_PRODUCT.empresa &&
      usuario === MOCK_PRODUCT.usuario &&
      contraseña === MOCK_PRODUCT.contraseña
    ) {
      return res.status(200).json({
        userId: '00000000-0000-0000-0000-000000000001',
        username: 'admin',
        firstName: 'Admin',
        lastName: 'Mock',
        companyId: '00000000-0000-0000-0000-000000000002',
        companyName: MOCK_PRODUCT.empresa,
        permissions: ['Users.Read', 'Users.Write', 'Companies.Read'],
        token: 'mock-jwt-token-product-' + Date.now(),
        cursorId: '00000000-0000-0000-0000-000000000001',
      });
    }
    return res.status(401).json({ message: 'Credenciales inválidas' });
  });

  // --- Usuarios (contrato alineado con UserController; solo datos ficticios) ---
  app.get('/api/user', (_, res) => {
    res.status(200).json([stubUser()]);
  });

  app.get('/api/user/:id', (req, res) => {
    const id = req.params.id;
    if (!isValidGuid(id)) {
      return res.status(400).json({ message: 'Formato de ID inválido' });
    }
    res.status(200).json(stubUser({ id }));
  });

  app.post('/api/user', (req, res) => {
    const body = req.body || {};
    const { companyId, username, password, firstName, lastName, email } = body;
    if (
      !companyId ||
      !username ||
      !password ||
      !firstName ||
      !lastName ||
      typeof companyId !== 'string' ||
      typeof username !== 'string' ||
      typeof firstName !== 'string' ||
      typeof lastName !== 'string'
    ) {
      return res.status(400).json({ message: 'Faltan campos requeridos: companyId, username, password, firstName, lastName' });
    }
    if (email !== undefined && email !== null && email !== '' && !isValidEmail(email)) {
      return res.status(400).json({ message: 'Formato de email inválido' });
    }
    const id = randomUUID();
    res.status(201).json(
      stubUser({
        id,
        companyId: String(companyId).trim(),
        companyName: body.companyName || 'Empresa Demo',
        username: String(username).trim(),
        firstName: String(firstName).trim(),
        lastName: String(lastName).trim(),
        email: email ? String(email).trim() : null,
        phone: body.phone != null ? String(body.phone) : null,
      })
    );
  });

  app.delete('/api/user/:id', (req, res) => {
    const id = req.params.id;
    if (!isValidGuid(id)) {
      return res.status(400).json({ message: 'Formato de ID inválido' });
    }
    res.status(204).send();
  });

  return app;
}

function adminApp() {
  const app = express();
  app.use(express.json());

  app.get('/health', (_, res) => {
    res.json({ status: 'healthy', mock: true, service: 'admin' });
  });

  app.post('/api/admin/auth/login', (req, res) => {
    const body = req.body || {};
    const usuario = body.Usuario ?? body.usuario;
    const contraseña = body.Contraseña ?? body.contraseña;
    if (usuario === MOCK_ADMIN.usuario && contraseña === MOCK_ADMIN.contraseña) {
      return res.status(200).json({
        UserId: '00000000-0000-0000-0000-000000000001',
        CursorId: '00000000-0000-0000-0000-000000000001',
        Username: 'admin',
        FirstName: 'Admin',
        LastName: 'Mock',
        Email: 'admin@mock.local',
        Role: 'Admin',
        Token: 'mock-jwt-token-admin-' + Date.now(),
      });
    }
    return res.status(401).json({ message: 'Credenciales administrativas inválidas' });
  });

  return app;
}

function listen(app, port, name) {
  return new Promise((resolve) => {
    const server = app.listen(port, () => {
      console.log(`[mock-apis] ${name} escuchando en http://localhost:${port}`);
      resolve(server);
    });
  });
}

(async () => {
  if (!adminOnly) {
    await listen(productApp(), PORT_PRODUCT, 'Product API');
  }
  if (!productOnly) {
    await listen(adminApp(), PORT_ADMIN, 'Admin API');
  }
  console.log('[mock-apis] Listo. Usa NEXT_PUBLIC_API_URL=http://localhost:' + PORT_PRODUCT + ' y ADMIN_API_URL=http://localhost:' + PORT_ADMIN);
})();
