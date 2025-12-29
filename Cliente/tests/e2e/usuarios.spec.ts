import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { UsuariosPage } from '../page-objects/UsuariosPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';

test.describe('Usuarios E2E Tests', () => {
  let cleanup: TestDataCleanup;
  const createdUserIds: string[] = [];

  test.beforeEach(async ({ page, request }) => {
    cleanup = new TestDataCleanup(request, process.env.API_URL || appConfig.api.url);
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');

    // Login antes de cada test
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

  test('debe mostrar la lista de usuarios', async ({ page }) => {
    const usuariosPage = new UsuariosPage(page);

    // Navegar a usuarios
    await usuariosPage.goto();

    // Verificar que se muestra el título
    await expect(usuariosPage.title).toBeVisible();

    // Verificar que existe la tabla de usuarios
    const hasUsers = await usuariosPage.verifyUsersList();
    expect(hasUsers).toBeTruthy();
  });

  test('debe abrir el modal de crear usuario', async ({ page }) => {
    const usuariosPage = new UsuariosPage(page);

    // Navegar a usuarios
    await usuariosPage.goto();

    // Abrir modal de crear
    await usuariosPage.openCreateModal();

    // Verificar que el modal está visible
    await expect(usuariosPage.createModal).toBeVisible();
  });

  test('debe navegar correctamente a la página de usuarios', async ({ page }) => {
    const usuariosPage = new UsuariosPage(page);

    // Navegar directamente (el goto ya espera a que la página cargue)
    await usuariosPage.goto();

    // Verificar URL (ya verificado en goto, pero lo verificamos de nuevo)
    await expect(page).toHaveURL(/\/usuarios/, { timeout: 5000 });

    // Verificar que el título está visible (ya esperado en goto)
    await expect(usuariosPage.title).toBeVisible({ timeout: 5000 });
  });
});

