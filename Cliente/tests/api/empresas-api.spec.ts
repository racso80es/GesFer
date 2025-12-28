import { test, expect } from '@playwright/test';
import { ApiClient } from './api-client';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

test.describe('API - Empresas', () => {
  let apiClient: ApiClient;
  let authToken: string;
  let cleanup: TestDataCleanup;
  const createdCompanyIds: string[] = [];

  // IDs de lenguajes según seed-data.sql
  const LANGUAGE_IDS = {
    es: '10000000-0000-0000-0000-000000000001', // Español
    en: '10000000-0000-0000-0000-000000000002', // English
    ca: '10000000-0000-0000-0000-000000000003', // Català
  };

  test.beforeEach(async ({ request }) => {
    apiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Login antes de cada test
    authToken = await apiClient.login('Empresa Demo', 'admin', 'admin123');
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
  });

  test.afterEach(async () => {
    // Teardown: Limpiar empresas creadas durante los tests
    for (const companyId of createdCompanyIds) {
      await cleanup.cleanupCompany(companyId);
    }
    createdCompanyIds.length = 0;
  });

  test('debe obtener lista de empresas', async () => {
    const response = await apiClient.get(
      '/api/company',
      apiClient.getAuthHeaders(authToken)
    );

    expect(response.ok()).toBeTruthy();
    const companies = await response.json();
    expect(Array.isArray(companies)).toBeTruthy();
  });

  test('debe obtener una empresa por ID', async () => {
    // Primero obtener lista de empresas
    const listResponse = await apiClient.get(
      '/api/company',
      apiClient.getAuthHeaders(authToken)
    );
    const companies = await listResponse.json();

    if (companies.length > 0) {
      const companyId = companies[0].id;
      const response = await apiClient.get(
        `/api/company/${companyId}`,
        apiClient.getAuthHeaders(authToken)
      );

      expect(response.ok()).toBeTruthy();
      const company = await response.json();
      expect(company).toHaveProperty('id', companyId);
    }
  });

  test('debe crear una nueva empresa y limpiarla después', async () => {
    const newCompanyData = {
      name: `Test Company ${Date.now()}_${Math.random().toString(36).substring(7)}`,
      taxId: `B${Math.floor(Math.random() * 100000000)}`,
      address: 'Calle Test 123',
      phone: '123456789',
      email: `test_${Date.now()}@example.com`,
      languageId: LANGUAGE_IDS.es,
    };

    const createResponse = await apiClient.post(
      '/api/company',
      newCompanyData,
      apiClient.getAuthHeaders(authToken)
    );

    expect(createResponse.ok()).toBeTruthy();
    const createdCompany = await createResponse.json();
    expect(createdCompany).toHaveProperty('id');
    expect(createdCompany.name).toBe(newCompanyData.name);
    
    // Registrar para limpieza
    createdCompanyIds.push(createdCompany.id);
    cleanup.registerCompanyId(createdCompany.id);
  });

  test('debe actualizar una empresa existente', async () => {
    // Primero crear una empresa
    const newCompanyData = {
      name: `Test Company Update ${Date.now()}_${Math.random().toString(36).substring(7)}`,
      taxId: `B${Math.floor(Math.random() * 100000000)}`,
      address: 'Calle Test 123',
      phone: '123456789',
      email: `test_update_${Date.now()}@example.com`,
      languageId: LANGUAGE_IDS.es,
    };

    const createResponse = await apiClient.post(
      '/api/company',
      newCompanyData,
      apiClient.getAuthHeaders(authToken)
    );

    expect(createResponse.ok()).toBeTruthy();
    const createdCompany = await createResponse.json();
    const companyId = createdCompany.id;
    
    // Registrar para limpieza
    createdCompanyIds.push(companyId);
    cleanup.registerCompanyId(companyId);

    // Actualizar la empresa
    const updateData = {
      name: `Updated Company ${Date.now()}`,
      taxId: createdCompany.taxId,
      address: 'Calle Actualizada 456',
      phone: '987654321',
      email: `updated_${Date.now()}@example.com`,
      languageId: LANGUAGE_IDS.en,
      isActive: true,
    };

    const updateResponse = await apiClient.put(
      `/api/company/${companyId}`,
      updateData,
      apiClient.getAuthHeaders(authToken)
    );

    expect(updateResponse.ok()).toBeTruthy();
    const updatedCompany = await updateResponse.json();
    expect(updatedCompany.name).toBe(updateData.name);
    expect(updatedCompany.address).toBe(updateData.address);
    expect(updatedCompany.languageId).toBe(updateData.languageId);
  });

  test('debe validar formato de ID al obtener empresa', async () => {
    const response = await apiClient.get(
      '/api/company/invalid-id',
      apiClient.getAuthHeaders(authToken)
    );

    expect(response.status()).toBeGreaterThanOrEqual(400);
  });
});

