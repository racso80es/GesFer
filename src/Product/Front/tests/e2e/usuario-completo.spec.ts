import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { UsuariosPage } from '../page-objects/UsuariosPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';

test.describe('Usuario Completo E2E Tests', () => {
  let cleanup: TestDataCleanup;

  test.beforeEach(async ({ request }) => {
    cleanup = new TestDataCleanup(request, process.env.API_URL || appConfig.api.url);
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
  });

  test.afterEach(async () => {
    await cleanup.cleanup();
  });

  test('debe completar el flujo completo de usuario', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const usuariosPage = new UsuariosPage(page);

    // Login
    await loginPage.goto();
    await loginPage.login('Empresa Demo', 'admin', 'admin123');
    await loginPage.verifyLoginSuccess();

    // Navegar a usuarios
    await usuariosPage.goto();
    await expect(usuariosPage.title).toBeVisible();
  });
});
