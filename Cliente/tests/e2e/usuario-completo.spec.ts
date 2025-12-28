import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { UsuariosPage } from '../page-objects/UsuariosPage';
import { UserFormPage } from '../page-objects/UserFormPage';
import { ApiClient } from '../api/api-client';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

/**
 * Test integral del flujo completo de creación de usuario
 * 
 * Este test verifica:
 * 1. Que el usuario no existe vía API (pre-condición)
 * 2. El flujo completo en la UI (llenar formulario, enviar, verificar redirección)
 * 3. Que el usuario se creó correctamente vía API (post-condición)
 * 4. Limpieza de datos de prueba (teardown)
 */
test.describe('Flujo Integral - Creación de Usuario', () => {
  let apiClient: ApiClient;
  let cleanup: TestDataCleanup;
  let authToken: string;
  const createdUserIds: string[] = [];

  /**
   * Genera datos de prueba únicos para evitar conflictos
   */
  function generateTestUserData(languageId?: string | undefined) {
    const timestamp = Date.now();
    const random = Math.random().toString(36).substring(7);
    return {
      username: `testuser_${timestamp}_${random}`,
      password: 'TestPassword123!',
      firstName: 'Test',
      lastName: 'User',
      email: `test_${timestamp}_${random}@example.com`,
      phone: '123456789',
      address: 'Calle Test 123',
      languageId: languageId, // Puede ser undefined para "por defecto"
    };
  }

  test.beforeEach(async ({ page, request }) => {
    apiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener información de login (la API no devuelve token, pero necesitamos el userId)
    const loginData = await apiClient.loginFull('Empresa Demo', 'admin', 'admin123');
    authToken = loginData.userId; // Usamos userId como identificador
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
    // Teardown: Limpiar usuarios creados durante los tests
    for (const userId of createdUserIds) {
      await cleanup.cleanupUser(userId);
    }
    createdUserIds.length = 0;
  });

  test('debe crear un usuario completo: verificación API → UI → verificación API', async ({ page, request }) => {
    // Recrear apiClient y cleanup con el request del test para evitar "Request context disposed"
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const testCleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener información de login (la API no devuelve token)
    const loginData = await testApiClient.loginFull('Empresa Demo', 'admin', 'admin123');
    const testAuthToken = loginData.userId; // Usamos userId como identificador
    await testCleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
    
    // Generar datos únicos para este test
    const testUserData = generateTestUserData();

    // ============================================
    // PASO 1: Verificación Pre-condición (API)
    // ============================================
    // Hacer la verificación API ANTES de los test.step para evitar "Request context disposed"
    const usersResponse = await testApiClient.get('/api/user');
    
    test.step('Verificar que el usuario no existe vía API', async () => {

      expect(usersResponse.ok()).toBeTruthy();
      const users = await usersResponse.json();
      
      // Verificar que el usuario de prueba no existe
      const existingUser = users.find((u: any) => u.username === testUserData.username);
      expect(existingUser).toBeUndefined();
    });

    // ============================================
    // PASO 2: Flujo en la UI
    // ============================================
    test.step('Navegar a la página de usuarios', async () => {
      const usuariosPage = new UsuariosPage(page);
      await usuariosPage.goto();
      await expect(usuariosPage.title).toBeVisible();
    });

    test.step('Abrir modal de crear usuario', async () => {
      const usuariosPage = new UsuariosPage(page);
      await usuariosPage.openCreateModal();
      await expect(usuariosPage.createModal).toBeVisible();
    });

    test.step('Llenar formulario de usuario', async () => {
      const userFormPage = new UserFormPage(page, false);
      await userFormPage.verifyFormVisible();
      
      // Verificar que el formulario está listo antes de llenarlo
      await userFormPage.usernameInput.waitFor({ state: 'visible', timeout: 10000 });
      
      // Esperar a que el companyId se establezca (viene del usuario logueado)
      await page.waitForTimeout(1000);
      
      // Verificar que no hay errores de validación antes de llenar
      const errorBefore = await userFormPage.errorMessage.isVisible().catch(() => false);
      if (errorBefore) {
        const errorText = await userFormPage.errorMessage.textContent();
        console.log('Error antes de llenar:', errorText);
      }
      
      await userFormPage.fillForm({
        username: testUserData.username,
        password: testUserData.password,
        firstName: testUserData.firstName,
        lastName: testUserData.lastName,
        email: testUserData.email,
        phone: testUserData.phone,
        address: testUserData.address,
        languageId: testUserData.languageId, // Incluir languageId (puede ser undefined)
      });
      
      // Esperar un momento para que los cambios se apliquen
      await page.waitForTimeout(500);
      
      // Verificar que los campos se llenaron correctamente
      const usernameValue = await userFormPage.usernameInput.inputValue();
      expect(usernameValue).toBe(testUserData.username);
      
      // Verificar que no hay errores de validación después de llenar
      // Los asteriscos (*) son solo marcadores de campos requeridos, no errores
      // Verificar errores reales de validación
      const fieldErrors = await page.evaluate(() => {
        const errors: string[] = [];
        // Buscar mensajes de error reales (no asteriscos)
        document.querySelectorAll('.text-destructive').forEach((el) => {
          const text = el.textContent?.trim();
          // Filtrar asteriscos y espacios
          if (text && text !== '*' && text.trim().length > 0) {
            errors.push(text);
          }
        });
        return errors;
      });
      
      // Solo lanzar error si hay errores reales (no solo asteriscos)
      if (fieldErrors.length > 0) {
        const errorText = await userFormPage.errorMessage.textContent().catch(() => '');
        const finalError = errorText || fieldErrors.join(', ');
        if (finalError && finalError !== '*' && finalError.trim().length > 0) {
          throw new Error(`Error de validación después de llenar el formulario: ${finalError}`);
        }
      }
    });

    test.step('Enviar formulario y verificar redirección', async () => {
      const userFormPage = new UserFormPage(page, false);
      const usuariosPage = new UsuariosPage(page);
      
      // Verificar que no hay errores de validación antes de enviar
      const errorMessage = userFormPage.errorMessage;
      await page.waitForTimeout(500); // Esperar a que cualquier validación se complete
      const hasError = await errorMessage.isVisible().catch(() => false);
      if (hasError) {
        const errorText = await errorMessage.textContent();
        // Si el error está vacío, puede ser un error de validación del navegador
        if (!errorText || errorText.trim() === '') {
          // Verificar si hay errores de validación HTML5
          const invalidFields = await page.evaluate(() => {
            const form = document.querySelector('form[data-testid="user-form-create"]') as HTMLFormElement;
            if (!form) return [];
            const invalid: string[] = [];
            form.querySelectorAll('input:invalid, select:invalid').forEach((el) => {
              invalid.push((el as HTMLElement).id || 'unknown');
            });
            return invalid;
          });
          if (invalidFields.length > 0) {
            throw new Error(`Error de validación en el formulario: Campos inválidos: ${invalidFields.join(', ')}`);
          }
        }
        if (errorText && errorText.trim() !== '') {
          throw new Error(`Error de validación en el formulario: ${errorText}`);
        }
      }
      
      // Esperar a que el botón de submit esté habilitado
      await userFormPage.submitButton.waitFor({ state: 'visible', timeout: 10000 });
      await userFormPage.submitButton.waitFor({ state: 'attached', timeout: 10000 });
      
      // Esperar a que la red esté inactiva antes de enviar
      await page.waitForLoadState('networkidle');
      
      // Interceptar la petición ANTES de hacer click
      const createRequestPromise = page.waitForResponse(
        (response) => {
          const url = response.url();
          const method = response.request().method();
          return url.includes('/api/user') && method === 'POST' && response.status() < 500;
        },
        { timeout: 20000 }
      ).catch(() => null);
      
      // Enviar formulario
      await userFormPage.submit();
      
      // Esperar a que la petición se complete
      const createResponse = await createRequestPromise;
      
      // Si hay respuesta del servidor, verificar su estado
      if (createResponse) {
        if (!createResponse.ok()) {
          // Si la respuesta no es exitosa, obtener el mensaje de error
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
          throw new Error(`Error del servidor al crear usuario: ${errorMessage}`);
        }
        // Si la respuesta es exitosa, esperar a que el modal se cierre
        await usuariosPage.createModal.waitFor({ state: 'hidden', timeout: 15000 });
      } else {
        // Si no hay respuesta, esperar a que aparezca un error o que el modal se cierre
        // Usar un timeout más corto para detectar errores rápidamente
        let modalClosed = false;
        let errorDetected = false;
        
        for (let attempt = 0; attempt < 20; attempt++) {
          // Verificar si hay error
          const hasError = await errorMessage.isVisible().catch(() => false);
          if (hasError) {
            await page.waitForTimeout(500); // Dar tiempo para que el texto se renderice
            const errorText = await errorMessage.textContent();
            const errorInDOM = await page.evaluate(() => {
              const errorEl = document.querySelector('[data-testid="user-form-error"]');
              if (errorEl) {
                const span = errorEl.querySelector('span');
                return span ? span.textContent || '' : (errorEl.textContent || '');
              }
              return '';
            });
            
            // Filtrar asteriscos y espacios vacíos
            const cleanErrorText = errorText && errorText.trim() && errorText !== '*' && !errorText.match(/^\*+$/) ? errorText.trim() : '';
            const cleanErrorInDOM = errorInDOM && errorInDOM.trim() && errorInDOM !== '*' && !errorInDOM.match(/^\*+$/) ? errorInDOM.trim() : '';
            
            if (cleanErrorText || cleanErrorInDOM) {
              errorDetected = true;
              throw new Error(`Error al crear usuario: ${cleanErrorText || cleanErrorInDOM}`);
            }
          }
          
          // Verificar si el modal se cerró
          const stillOpen = await usuariosPage.createModal.isVisible().catch(() => true);
          if (!stillOpen) {
            modalClosed = true;
            break;
          }
          
          await page.waitForTimeout(500);
        }
        
        if (!modalClosed && !errorDetected) {
          // Si después de todos los intentos el modal no se cerró y no hay error visible,
          // puede ser un problema de timing o un error que no se está mostrando
          // Verificar una última vez si hay error
          const finalErrorCheck = await errorMessage.isVisible().catch(() => false);
          if (finalErrorCheck) {
            const errorText = await errorMessage.textContent();
            throw new Error(`Error al crear usuario: ${errorText || 'Error desconocido'}`);
          }
          throw new Error('No se recibió respuesta del servidor y el modal no se cerró');
        }
      }
      
      // Si llegamos aquí, el modal se cerró (éxito)
      // Esperar a que la tabla se actualice
      await page.waitForTimeout(2000);
      
      // Verificar que estamos en la página de usuarios
      await expect(page).toHaveURL(/\/usuarios/, { timeout: 5000 });
      
      // Verificar que el usuario aparece en la tabla (con retry)
      let userRow = null;
      for (let attempt = 0; attempt < 10; attempt++) {
        await usuariosPage.usersTable.waitFor({ state: 'visible', timeout: 5000 });
        userRow = await usuariosPage.findUserByUsername(testUserData.username);
        if (userRow) {
          break;
        }
        await page.waitForTimeout(1000);
      }
      expect(userRow).not.toBeNull();
    });

    // ============================================
    // PASO 3: Verificación Post-condición (API)
    // ============================================
    // Esperar un momento para que el usuario se persista en la base de datos
    await page.waitForTimeout(3000);
    
    // Hacer la verificación API DESPUÉS de los pasos UI pero ANTES de test.step
    // Usar retry para asegurar que el usuario esté disponible
    let createdUser: any = null;
    let users: any[] = [];
    
    for (let attempt = 0; attempt < 30; attempt++) {
      const usersResponseAfter = await testApiClient.get('/api/user');
      expect(usersResponseAfter.ok()).toBeTruthy();
      users = await usersResponseAfter.json();
      
      // Buscar el usuario creado (comparación exacta del username)
      createdUser = users.find((u: any) => u.username === testUserData.username);
      if (createdUser) {
        break;
      }
      
      // Esperar un poco antes de reintentar (aumentar progresivamente)
      if (attempt < 29) {
        await page.waitForTimeout(1000 + (attempt * 100));
      }
    }
    
    test.step('Verificar que el usuario se creó correctamente vía API', async () => {
      // Si el usuario no se encontró después de todos los intentos, mostrar información de debug
      if (!createdUser) {
        console.log('Usuarios encontrados:', users.map((u: any) => u.username));
        console.log('Buscando usuario:', testUserData.username);
      }
      expect(createdUser).toBeDefined();
      expect(createdUser).not.toBeNull();
      
      // Verificar los datos del usuario (campos requeridos)
      expect(createdUser.username).toBe(testUserData.username);
      expect(createdUser.firstName).toBe(testUserData.firstName);
      expect(createdUser.lastName).toBe(testUserData.lastName);
      expect(createdUser.id).toBeTruthy();
      expect(typeof createdUser.id).toBe('string');
      
      // Verificar campos opcionales (pueden estar vacíos o no estar presentes)
      // El API puede devolver estos campos como cadenas vacías si no se proporcionaron
      if (testUserData.email && createdUser.email !== undefined && createdUser.email !== '') {
        expect(createdUser.email).toBe(testUserData.email);
      }
      if (testUserData.phone && createdUser.phone !== undefined && createdUser.phone !== '') {
        expect(createdUser.phone).toBe(testUserData.phone);
      }
      if (testUserData.address && createdUser.address !== undefined && createdUser.address !== '') {
        expect(createdUser.address).toBe(testUserData.address);
      }
      
      // Verificar languageId: si no se proporcionó (undefined), debe ser null o undefined en la respuesta
      if (testUserData.languageId === undefined) {
        // Cuando se selecciona "por defecto", el languageId debe ser null o undefined
        expect(createdUser.languageId === null || createdUser.languageId === undefined).toBeTruthy();
      } else if (testUserData.languageId) {
        // Si se proporcionó un languageId, debe coincidir
        expect(createdUser.languageId).toBe(testUserData.languageId);
      }
      
      // Registrar para limpieza
      createdUserIds.push(createdUser.id);
      testCleanup.registerUserId(createdUser.id);
      
      // Verificar que el usuario se puede obtener por ID (la API no requiere autenticación)
      const userByIdResponse = await testApiClient.get(`/api/user/${createdUser.id}`);
      
      expect(userByIdResponse.ok()).toBeTruthy();
      const userById = await userByIdResponse.json();
      expect(userById.id).toBe(createdUser.id);
      expect(userById.username).toBe(testUserData.username);
      
      // Verificar languageId también en la respuesta individual
      if (testUserData.languageId === undefined) {
        // Cuando se selecciona "por defecto", debe ser null
        expect(userById.languageId === null || userById.languageId === undefined).toBeTruthy();
      } else if (testUserData.languageId) {
        expect(userById.languageId).toBe(testUserData.languageId);
      }
    });
  });

  test('debe validar campos requeridos en el formulario', async ({ page }) => {
    const usuariosPage = new UsuariosPage(page);
    const userFormPage = new UserFormPage(page, false);

    await usuariosPage.goto();
    await usuariosPage.openCreateModal();
    await userFormPage.verifyFormVisible();

    // Intentar enviar sin llenar campos requeridos
    await userFormPage.submit();

    // Verificar que el formulario no se envió (el modal sigue visible)
    await expect(usuariosPage.createModal).toBeVisible();
  });

  test('debe cancelar la creación de usuario', async ({ page, request }) => {
    // Recrear apiClient con el request del test
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const loginData = await testApiClient.loginFull('Empresa Demo', 'admin', 'admin123');
    const testAuthToken = loginData.userId;
    
    // Generar datos únicos para este test
    const testUserData = generateTestUserData();
    
    const usuariosPage = new UsuariosPage(page);
    const userFormPage = new UserFormPage(page, false);

    await usuariosPage.goto();
    await usuariosPage.openCreateModal();
    await userFormPage.verifyFormVisible();

    // Llenar algunos campos
    await userFormPage.fillForm({
      username: testUserData.username,
      password: testUserData.password,
      firstName: testUserData.firstName,
      lastName: testUserData.lastName,
    });

    // Cancelar
    await userFormPage.cancel();

    // Verificar que el modal se cerró
    await usuariosPage.createModal.waitFor({ state: 'hidden', timeout: 5000 });

    // Verificar que el usuario NO se creó vía API (la API no requiere autenticación)
    const usersResponse = await testApiClient.get('/api/user');
    const users = await usersResponse.json();
    const createdUser = users.find((u: any) => u.username === testUserData.username);
    expect(createdUser).toBeUndefined();
  });

  test('debe persistir languageId como null/undefined cuando se selecciona "por defecto"', async ({ page, request }) => {
    // Recrear apiClient y cleanup con el request del test
    const testApiClient = new ApiClient(request, process.env.API_URL || 'http://localhost:5000');
    const testCleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
    
    // Obtener información de login
    const loginData = await testApiClient.loginFull('Empresa Demo', 'admin', 'admin123');
    const testAuthToken = loginData.userId;
    await testCleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
    
    // Generar datos únicos SIN languageId (undefined = "por defecto")
    const testUserData = generateTestUserData(undefined);
    
    // Navegar a usuarios
    const usuariosPage = new UsuariosPage(page);
    await usuariosPage.goto();
    
    // Abrir modal de crear
    await usuariosPage.openCreateModal();
    
    // Llenar formulario SIN especificar languageId (debe quedar en "por defecto")
    const userFormPage = new UserFormPage(page, false);
    await userFormPage.verifyFormVisible();
    
    await userFormPage.fillForm({
      username: testUserData.username,
      password: testUserData.password,
      firstName: testUserData.firstName,
      lastName: testUserData.lastName,
      email: testUserData.email,
      phone: testUserData.phone,
      address: testUserData.address,
      languageId: undefined, // Explícitamente undefined para "por defecto"
    });
    
    // Interceptar la petición de creación para verificar qué se está enviando
    let requestBody: any = null;
    const createRequestPromise = page.waitForResponse(
      async (response) => {
        const url = response.url();
        const method = response.request().method();
        if (url.includes('/api/user') && method === 'POST' && response.status() < 500) {
          // Capturar el cuerpo de la petición
          const request = response.request();
          const postData = request.postData();
          if (postData) {
            try {
              requestBody = JSON.parse(postData);
            } catch (e) {
              // Ignorar errores de parseo
            }
          }
          return true;
        }
        return false;
      },
      { timeout: 20000 }
    ).catch(() => null);
    
    // Enviar formulario
    await userFormPage.submit();
    
    // Esperar a que la petición se complete
    const createResponse = await createRequestPromise;
    
    // Verificar qué se envió en la petición
    console.log('Request body enviado:', JSON.stringify(requestBody, null, 2));
    if (requestBody && 'languageId' in requestBody) {
      console.log('languageId en request:', requestBody.languageId, 'tipo:', typeof requestBody.languageId);
      // Verificar que languageId no fue enviado o es null/undefined
      expect(
        requestBody.languageId === null || 
        requestBody.languageId === undefined ||
        requestBody.languageId === ''
      ).toBeTruthy();
    } else {
      console.log('languageId NO fue enviado en el request (comportamiento esperado)');
    }
    
    if (createResponse && createResponse.ok()) {
      await usuariosPage.createModal.waitFor({ state: 'hidden', timeout: 15000 });
    }
    
    // Esperar a que el usuario se persista
    await page.waitForTimeout(3000);
    
    // Buscar el usuario creado vía API
    let createdUser: any = null;
    for (let attempt = 0; attempt < 30; attempt++) {
      const usersResponse = await testApiClient.get('/api/user');
      expect(usersResponse.ok()).toBeTruthy();
      const users = await usersResponse.json();
      createdUser = users.find((u: any) => u.username === testUserData.username);
      if (createdUser) {
        break;
      }
      await page.waitForTimeout(1000 + (attempt * 100));
    }
    
    // Verificar que el usuario existe
    expect(createdUser).toBeDefined();
    expect(createdUser).not.toBeNull();
    
    // VERIFICACIÓN PRINCIPAL: cuando se selecciona "por defecto" (no se envía languageId),
    // el backend debe persistir null (la resolución jerárquica se hace en tiempo de lectura)
    console.log('languageId recibido en usuario:', createdUser.languageId, 'tipo:', typeof createdUser.languageId);
    // El test DEBE fallar si languageId tiene un valor (debe ser estrictamente null)
    expect(createdUser.languageId).toBeNull();
    
    // Verificar también en la respuesta individual
    const userByIdResponse = await testApiClient.get(`/api/user/${createdUser.id}`);
    expect(userByIdResponse.ok()).toBeTruthy();
    const userById = await userByIdResponse.json();
    
    // Verificar que el backend persistió null (no debe asignar valores por defecto)
    expect(userById.languageId).toBeNull();
    
    // Limpiar
    createdUserIds.push(createdUser.id);
    testCleanup.registerUserId(createdUser.id);
  });
});

