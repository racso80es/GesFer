import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { DashboardPage } from '../page-objects/DashboardPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';

test.describe('Login E2E Tests', () => {
  let cleanup: TestDataCleanup;

  test.beforeEach(async ({ request }) => {
    cleanup = new TestDataCleanup(request, process.env.API_URL || appConfig.api.url);
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
  });

  test.afterEach(async () => {
    // Teardown: Limpiar cualquier dato de prueba creado durante los tests
    await cleanup.cleanup();
  });
  test('debe realizar login exitoso con credenciales válidas', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const dashboardPage = new DashboardPage(page);

    // Navegar a login
    await loginPage.goto();

    // Realizar login
    await loginPage.login('Empresa Demo', 'admin', 'admin123');

    // Verificar que el login fue exitoso
    await loginPage.verifyLoginSuccess();

    // Verificar que estamos en el dashboard
    await expect(dashboardPage.title).toBeVisible();
  });

  test('debe mostrar error con credenciales inválidas', async ({ page }) => {
    const loginPage = new LoginPage(page);

    // Navegar a login
    await loginPage.goto();

    // Intentar login con credenciales inválidas
    await loginPage.login('Empresa Demo', 'admin', 'password-incorrecta');

    // Verificar que se muestra mensaje de error
    await loginPage.verifyErrorMessage();
  });

  test('debe validar campos requeridos', async ({ page }) => {
    const loginPage = new LoginPage(page);

    // Navegar a login
    await loginPage.goto();

    // Esperar a que el formulario esté completamente cargado
    await expect(loginPage.loginForm).toBeVisible();
    await expect(loginPage.empresaInput).toBeVisible();
    await expect(loginPage.usuarioInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();

    // Limpiar los campos
    await loginPage.empresaInput.clear();
    await loginPage.usuarioInput.clear();
    await loginPage.passwordInput.clear();

    // Intentar login sin completar campos
    await loginPage.loginButton.click();

    // Verificar que los campos siguen visibles (el formulario no se envió)
    await expect(loginPage.empresaInput).toBeVisible();
    await expect(loginPage.usuarioInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();
    
    // Verificar que estamos todavía en la página de login
    await expect(page).toHaveURL(/\/login/);
  });
});

