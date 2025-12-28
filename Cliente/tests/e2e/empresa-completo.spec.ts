import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { EmpresasPage } from '../page-objects/EmpresasPage';
import { CompanyFormPage } from '../page-objects/CompanyFormPage';
import { ApiClient } from '../api/api-client';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

/**
 * Test integral del flujo completo de creación de empresa
 * 
 * Este test verifica:
 * 1. Que la empresa no existe vía API (pre-condición)
 * 2. El flujo completo en la UI (llenar formulario, enviar, verificar redirección)
 * 3. Que la empresa se creó correctamente vía API (post-condición)
 * 4. Limpieza de datos de prueba (teardown)
 */
test.describe('Flujo Integral - Creación de Empresa', () => {
  let apiClient: ApiClient;
  let cleanup: TestDataCleanup;
  let authToken: string;
  const createdCompanyIds: string[] = [];

  // IDs de lenguajes según seed-data.sql
  const LANGUAGE_IDS = {
    es: '10000000-0000-0000-0000-000000000001', // Español
    en: '10000000-0000-0000-0000-000000000002', // English
    ca: '10000000-0000-0000-0000-000000000003', // Català
  };

  /**
   * Genera datos de prueba únicos para evitar conflictos
   */
  function generateTestCompanyData(languageId?: string | undefined) {
    const timestamp = Date.now();
    const random = Math.random().toString(36).substring(7);
    return {
      name: `Test Company ${timestamp}_${random}`,
      taxId: `B${Math.floor(Math.random() * 100000000)}`,
      address: 'Calle Test 123',
      phone: '123456789',
      email: `test_${timestamp}_${random}@example.com`,
      languageId: languageId, // Puede ser undefined para "por defecto"
    };
  }

  test.beforeEach(async ({ page, request }) => {
    apiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener token de autenticación
    authToken = await apiClient.login('Empresa Demo', 'admin', 'admin123');
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');

    // Login en la UI
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('Empresa Demo', 'admin', 'admin123');
    await loginPage.verifyLoginSuccess();
    
    // Esperar a que la navegación al dashboard se complete
    await page.waitForURL(/\/dashboard/, { timeout: 10000 });
    
    // Esperar un poco más para asegurar que la autenticación se haya establecido
    await page.waitForTimeout(500);
  });

  test.afterEach(async () => {
    // Teardown: Limpiar empresas creadas durante los tests
    for (const companyId of createdCompanyIds) {
      await cleanup.cleanupCompany(companyId);
    }
    createdCompanyIds.length = 0;
  });

  test('debe crear una empresa completa: verificación API → UI → verificación API', async ({ page, request }) => {
    // Recrear apiClient y cleanup con el request del test
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const testCleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener token de autenticación
    const testAuthToken = await testApiClient.login('Empresa Demo', 'admin', 'admin123');
    await testCleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
    
    // Generar datos únicos para este test
    const testCompanyData = generateTestCompanyData();

    // ============================================
    // PASO 1: Verificación Pre-condición (API)
    // ============================================
    const companiesResponse = await testApiClient.get(
      '/api/company',
      testApiClient.getAuthHeaders(testAuthToken)
    );
    
    test.step('Verificar que la empresa no existe vía API', async () => {
      expect(companiesResponse.ok()).toBeTruthy();
      const companies = await companiesResponse.json();
      
      // Verificar que la empresa de prueba no existe
      const existingCompany = companies.find((c: any) => c.name === testCompanyData.name);
      expect(existingCompany).toBeUndefined();
    });

    // ============================================
    // PASO 2: Flujo en la UI
    // ============================================
    test.step('Navegar a la página de empresas', async () => {
      const empresasPage = new EmpresasPage(page);
      await empresasPage.goto();
      await expect(empresasPage.title).toBeVisible();
    });

    test.step('Abrir modal de crear empresa', async () => {
      const empresasPage = new EmpresasPage(page);
      await empresasPage.openCreateModal();
      await expect(empresasPage.createModal).toBeVisible();
    });

    test.step('Llenar formulario de empresa', async () => {
      const companyFormPage = new CompanyFormPage(page, false);
      await companyFormPage.verifyFormVisible();
      
      // Verificar que el formulario está listo antes de llenarlo
      await companyFormPage.nameInput.waitFor({ state: 'visible', timeout: 10000 });
      
      // Esperar un momento para que el formulario se renderice completamente
      await page.waitForTimeout(1000);
      
      await companyFormPage.fillForm({
        name: testCompanyData.name,
        taxId: testCompanyData.taxId,
        address: testCompanyData.address,
        phone: testCompanyData.phone,
        email: testCompanyData.email,
        languageId: testCompanyData.languageId, // Incluir languageId (puede ser undefined)
      });
      
      // Esperar un momento para que los cambios se apliquen
      await page.waitForTimeout(500);
      
      // Verificar que los campos se llenaron correctamente
      const nameValue = await companyFormPage.nameInput.inputValue();
      expect(nameValue).toBe(testCompanyData.name);
    });

    test.step('Enviar formulario y verificar redirección', async () => {
      const companyFormPage = new CompanyFormPage(page, false);
      const empresasPage = new EmpresasPage(page);
      
      // Esperar a que el botón de submit esté habilitado
      await companyFormPage.submitButton.waitFor({ state: 'visible', timeout: 10000 });
      
      // Esperar a que la red esté inactiva antes de enviar
      await page.waitForLoadState('networkidle');
      
      // Interceptar la petición ANTES de hacer click
      const createRequestPromise = page.waitForResponse(
        (response) => {
          const url = response.url();
          const method = response.request().method();
          return url.includes('/api/company') && method === 'POST' && response.status() < 500;
        },
        { timeout: 20000 }
      ).catch(() => null);
      
      // Enviar formulario
      await companyFormPage.submit();
      
      // Esperar a que la petición se complete
      const createResponse = await createRequestPromise;
      
      // Si hay respuesta del servidor, verificar su estado
      if (createResponse) {
        if (!createResponse.ok()) {
          const errorBody = await createResponse.text().catch(() => '');
          let errorMessage = `Error del servidor: ${createResponse.status()}`;
          try {
            const errorJson = JSON.parse(errorBody);
            errorMessage = errorJson.message || errorJson.error || errorMessage;
          } catch {
            if (errorBody) {
              errorMessage = errorBody;
            }
          }
          throw new Error(`Error del servidor al crear empresa: ${errorMessage}`);
        }
        // Si la respuesta es exitosa, esperar a que el modal se cierre
        await empresasPage.createModal.waitFor({ state: 'hidden', timeout: 15000 });
      } else {
        // Si no hay respuesta, esperar a que el modal se cierre
        await empresasPage.createModal.waitFor({ state: 'hidden', timeout: 15000 });
      }
      
      // Si llegamos aquí, el modal se cerró (éxito)
      // Esperar a que la tabla se actualice
      await page.waitForTimeout(2000);
      
      // Verificar que estamos en la página de empresas
      await expect(page).toHaveURL(/\/empresas/, { timeout: 5000 });
      
      // Verificar que la empresa aparece en la tabla (con retry)
      let companyRow = null;
      for (let attempt = 0; attempt < 10; attempt++) {
        await empresasPage.companiesTable.waitFor({ state: 'visible', timeout: 5000 });
        companyRow = await empresasPage.findCompanyByName(testCompanyData.name);
        if (companyRow) {
          break;
        }
        await page.waitForTimeout(1000);
      }
      expect(companyRow).not.toBeNull();
    });

    // ============================================
    // PASO 3: Verificación Post-condición (API)
    // ============================================
    // Esperar un momento para que la empresa se persista en la base de datos
    await page.waitForTimeout(3000);
    
    // Usar retry para asegurar que la empresa esté disponible
    let createdCompany: any = null;
    let companies: any[] = [];
    
    for (let attempt = 0; attempt < 30; attempt++) {
      const companiesResponseAfter = await testApiClient.get(
        '/api/company',
        testApiClient.getAuthHeaders(testAuthToken)
      );
      expect(companiesResponseAfter.ok()).toBeTruthy();
      companies = await companiesResponseAfter.json();
      
      // Buscar la empresa creada (comparación exacta del name)
      createdCompany = companies.find((c: any) => c.name === testCompanyData.name);
      if (createdCompany) {
        break;
      }
      
      // Esperar un poco antes de reintentar
      if (attempt < 29) {
        await page.waitForTimeout(1000 + (attempt * 100));
      }
    }
    
    test.step('Verificar que la empresa se creó correctamente vía API', async () => {
      // Si la empresa no se encontró después de todos los intentos, mostrar información de debug
      if (!createdCompany) {
        console.log('Empresas encontradas:', companies.map((c: any) => c.name));
        console.log('Buscando empresa:', testCompanyData.name);
      }
      expect(createdCompany).toBeDefined();
      expect(createdCompany).not.toBeNull();
      
      // Verificar los datos de la empresa
      expect(createdCompany.name).toBe(testCompanyData.name);
      expect(createdCompany.taxId).toBe(testCompanyData.taxId);
      expect(createdCompany.address).toBe(testCompanyData.address);
      expect(createdCompany.id).toBeTruthy();
      expect(typeof createdCompany.id).toBe('string');
      
      // Verificar campos opcionales
      if (testCompanyData.email && createdCompany.email !== undefined && createdCompany.email !== '') {
        expect(createdCompany.email).toBe(testCompanyData.email);
      }
      if (testCompanyData.phone && createdCompany.phone !== undefined && createdCompany.phone !== '') {
        expect(createdCompany.phone).toBe(testCompanyData.phone);
      }
      
      // Verificar languageId: si no se proporcionó (undefined), debe ser null en la respuesta
      if (testCompanyData.languageId === undefined) {
        // Cuando se selecciona "por defecto", el languageId debe ser null (la resolución jerárquica se hace en lectura)
        expect(createdCompany.languageId === null || createdCompany.languageId === undefined).toBeTruthy();
      } else if (testCompanyData.languageId) {
        // Si se proporcionó un languageId, debe coincidir
        expect(createdCompany.languageId).toBe(testCompanyData.languageId);
      }
      
      // Registrar para limpieza
      createdCompanyIds.push(createdCompany.id);
      testCleanup.registerCompanyId(createdCompany.id);
      
      // Verificar que la empresa se puede obtener por ID
      const companyByIdResponse = await testApiClient.get(
        `/api/company/${createdCompany.id}`,
        testApiClient.getAuthHeaders(testAuthToken)
      );
      
      expect(companyByIdResponse.ok()).toBeTruthy();
      const companyById = await companyByIdResponse.json();
      expect(companyById.id).toBe(createdCompany.id);
      expect(companyById.name).toBe(testCompanyData.name);
      
      // Verificar languageId también en la respuesta individual
      if (testCompanyData.languageId === undefined) {
        // Cuando se selecciona "por defecto", debe ser null
        expect(companyById.languageId === null || companyById.languageId === undefined).toBeTruthy();
      } else if (testCompanyData.languageId) {
        expect(companyById.languageId).toBe(testCompanyData.languageId);
      }
    });
  });

  test('debe validar campos requeridos en el formulario', async ({ page }) => {
    const empresasPage = new EmpresasPage(page);
    const companyFormPage = new CompanyFormPage(page, false);

    await empresasPage.goto();
    await empresasPage.openCreateModal();
    await companyFormPage.verifyFormVisible();

    // Intentar enviar sin llenar campos requeridos
    await companyFormPage.submit();

    // Verificar que el formulario no se envió (el modal sigue visible)
    await expect(empresasPage.createModal).toBeVisible();
  });

  test('debe cancelar la creación de empresa', async ({ page, request }) => {
    // Recrear apiClient con el request del test
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const testAuthToken = await testApiClient.login('Empresa Demo', 'admin', 'admin123');
    
    // Generar datos únicos para este test
    const testCompanyData = generateTestCompanyData();
    
    const empresasPage = new EmpresasPage(page);
    const companyFormPage = new CompanyFormPage(page, false);

    await empresasPage.goto();
    await empresasPage.openCreateModal();
    await companyFormPage.verifyFormVisible();

    // Llenar algunos campos
    await companyFormPage.fillForm({
      name: testCompanyData.name,
      address: testCompanyData.address,
    });

    // Cancelar
    await companyFormPage.cancel();

    // Verificar que el modal se cerró
    await empresasPage.createModal.waitFor({ state: 'hidden', timeout: 5000 });

    // Verificar que la empresa NO se creó vía API
    const companiesResponse = await testApiClient.get(
      '/api/company',
      testApiClient.getAuthHeaders(testAuthToken)
    );
    const companies = await companiesResponse.json();
    const createdCompany = companies.find((c: any) => c.name === testCompanyData.name);
    expect(createdCompany).toBeUndefined();
  });

  test('debe persistir languageId como null/undefined cuando se selecciona "por defecto"', async ({ request }) => {
    // Este test verifica directamente vía API que cuando no se envía languageId,
    // el backend persiste null (no asigna valores por defecto)
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const testCleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener token de autenticación
    const testAuthToken = await testApiClient.login('Empresa Demo', 'admin', 'admin123');
    await testCleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
    
    // Generar datos únicos SIN languageId (undefined = "por defecto")
    const testCompanyData = generateTestCompanyData(undefined);
    
    // Crear empresa vía API SIN languageId (no incluir el campo en el request)
    const createData: any = {
      name: testCompanyData.name,
      taxId: testCompanyData.taxId,
      address: testCompanyData.address,
      phone: testCompanyData.phone,
      email: testCompanyData.email,
      // languageId NO se incluye - debe persistirse como null
    };
    
    const createResponse = await testApiClient.post(
      '/api/company',
      createData,
      testApiClient.getAuthHeaders(testAuthToken)
    );
    
    expect(createResponse.ok()).toBeTruthy();
    const createdCompany = await createResponse.json();
    
    // VERIFICACIÓN PRINCIPAL: languageId debe ser null cuando no se envía
    // (la resolución jerárquica se hace en tiempo de lectura, no en persistencia)
    console.log('languageId recibido en empresa:', createdCompany.languageId, 'tipo:', typeof createdCompany.languageId);
    // El test DEBE fallar si languageId tiene un valor (debe ser estrictamente null)
    expect(createdCompany.languageId).toBeNull();
    
    // Verificar también en la respuesta individual
    const companyByIdResponse = await testApiClient.get(
      `/api/company/${createdCompany.id}`,
      testApiClient.getAuthHeaders(testAuthToken)
    );
    expect(companyByIdResponse.ok()).toBeTruthy();
    const companyById = await companyByIdResponse.json();
    
    // Verificar que se persistió null (no debe asignar valores por defecto)
    expect(companyById.languageId).toBeNull();
    
    // Limpiar
    createdCompanyIds.push(createdCompany.id);
    testCleanup.registerCompanyId(createdCompany.id);
  });
});

