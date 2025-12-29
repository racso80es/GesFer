import { test, expect } from '@playwright/test';
import { ApiClient } from '../api/api-client';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

test.describe('API - Autenticación', () => {
  let apiClient: ApiClient;
  let cleanup: TestDataCleanup;

  test.beforeEach(async ({ request }) => {
    apiClient = new ApiClient(request, process.env.API_URL || 'http://127.0.0.1:5000');
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://127.0.0.1:5000');
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
  });

  test.afterEach(async () => {
    // Teardown: Limpiar cualquier dato de prueba creado durante los tests
    await cleanup.cleanup();
  });

  test('debe realizar login exitoso y obtener información del usuario', async () => {
    const loginData = await apiClient.loginFull('Empresa Demo', 'admin', 'admin123');

    expect(loginData).toBeTruthy();
    expect(loginData).toHaveProperty('userId');
    expect(loginData).toHaveProperty('username');
    expect(loginData).toHaveProperty('firstName');
    expect(loginData).toHaveProperty('lastName');
    expect(loginData).toHaveProperty('companyId');
    expect(loginData.username).toBe('admin');
  });

  test('debe rechazar login con credenciales inválidas', async () => {
    const response = await apiClient.post('/api/auth/login', {
      empresa: 'Empresa Demo',
      usuario: 'admin',
      contraseña: 'password-incorrecta',
    });

    expect(response.status()).toBe(401);
  });

  test('debe validar campos requeridos en login', async () => {
    const response = await apiClient.post('/api/auth/login', {
      empresa: '',
      usuario: '',
      contraseña: '',
    });

    expect(response.status()).toBeGreaterThanOrEqual(400);
  });

  test('debe obtener información del usuario autenticado', async () => {
    // La API no tiene endpoint /api/auth/me
    // En su lugar, el login ya devuelve toda la información del usuario
    const loginData = await apiClient.loginFull('Empresa Demo', 'admin', 'admin123');

    expect(loginData).toBeTruthy();
    expect(loginData).toHaveProperty('userId');
    expect(loginData).toHaveProperty('username');
    expect(loginData).toHaveProperty('firstName');
    expect(loginData).toHaveProperty('lastName');
    expect(loginData).toHaveProperty('companyId');
    expect(loginData).toHaveProperty('companyName');
  });
});

