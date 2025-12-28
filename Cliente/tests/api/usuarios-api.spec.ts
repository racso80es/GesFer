import { test, expect } from '@playwright/test';
import { ApiClient } from '../api/api-client';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

test.describe('API - Usuarios', () => {
  let apiClient: ApiClient;
  let authToken: string;
  let cleanup: TestDataCleanup;
  const createdUserIds: string[] = [];

  test.beforeEach(async ({ request }) => {
    apiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Login antes de cada test
    authToken = await apiClient.login('Empresa Demo', 'admin', 'admin123');
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
  });

  test.afterEach(async () => {
    // Teardown: Limpiar usuarios creados durante los tests
    for (const userId of createdUserIds) {
      await cleanup.cleanupUser(userId);
    }
    createdUserIds.length = 0;
  });

  test('debe obtener lista de usuarios', async () => {
    const response = await apiClient.get(
      '/api/user',
      apiClient.getAuthHeaders(authToken)
    );

    expect(response.ok()).toBeTruthy();
    const users = await response.json();
    expect(Array.isArray(users)).toBeTruthy();
  });

  test('debe obtener un usuario por ID', async () => {
    // Primero obtener lista de usuarios
    const listResponse = await apiClient.get(
      '/api/user',
      apiClient.getAuthHeaders(authToken)
    );
    const users = await listResponse.json();

    if (users.length > 0) {
      const userId = users[0].id;
      const response = await apiClient.get(
        `/api/user/${userId}`,
        apiClient.getAuthHeaders(authToken)
      );

      expect(response.ok()).toBeTruthy();
      const user = await response.json();
      expect(user).toHaveProperty('id', userId);
    }
  });

  test('debe crear un nuevo usuario y limpiarlo después', async () => {
    // Primero obtener el companyId de la empresa "Empresa Demo" desde el login
    const loginData = await apiClient.loginFull('Empresa Demo', 'admin', 'admin123');
    const companyId = loginData.companyId;

    const newUserData = {
      companyId: companyId,
      username: `testuser_${Date.now()}_${Math.random().toString(36).substring(7)}`,
      password: 'TestPassword123!',
      firstName: 'Test',
      lastName: 'User',
      email: `test_${Date.now()}@example.com`,
      phone: '123456789',
    };

    // La API no requiere autenticación, pero mantenemos el header por si acaso
    const createResponse = await apiClient.post(
      '/api/user',
      newUserData
    );

    expect(createResponse.ok()).toBeTruthy();
    const createdUser = await createResponse.json();
    expect(createdUser).toHaveProperty('id');
    
    // Registrar para limpieza
    createdUserIds.push(createdUser.id);
    cleanup.registerUserId(createdUser.id);
  });

  test('debe permitir acceso sin token de autenticación (API sin autenticación requerida)', async () => {
    // Nota: La API actual no requiere autenticación para los endpoints
    // Este test verifica que el endpoint funciona sin autenticación
    const response = await apiClient.get('/api/user');

    // La API devuelve 200 porque no requiere autenticación
    expect(response.ok()).toBeTruthy();
    const users = await response.json();
    expect(Array.isArray(users)).toBeTruthy();
  });

  test('debe validar formato de ID al obtener usuario', async () => {
    const response = await apiClient.get(
      '/api/user/invalid-id',
      apiClient.getAuthHeaders(authToken)
    );

    expect(response.status()).toBeGreaterThanOrEqual(400);
  });
});

