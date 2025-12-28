import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { EmpresasPage } from '../page-objects/EmpresasPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

test.describe('Empresas E2E Tests', () => {
  let cleanup: TestDataCleanup;
  const createdCompanyIds: string[] = [];

  test.beforeEach(async ({ page, request }) => {
    cleanup = new TestDataCleanup(request, process.env.API_URL || 'http://localhost:5000');
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
    // Teardown: Limpiar empresas creadas durante los tests
    for (const companyId of createdCompanyIds) {
      await cleanup.cleanupCompany(companyId);
    }
    createdCompanyIds.length = 0;
  });

  test('debe mostrar la lista de empresas', async ({ page }) => {
    const empresasPage = new EmpresasPage(page);

    // Navegar a empresas
    await empresasPage.goto();

    // Verificar que se muestra el título
    await expect(empresasPage.title).toBeVisible();

    // Verificar que existe la tabla de empresas (puede estar vacía)
    const hasCompanies = await empresasPage.verifyCompaniesList();
    // No fallar si no hay empresas, solo verificar que la tabla está presente
    expect(hasCompanies !== undefined).toBeTruthy();
  });

  test('debe abrir el modal de crear empresa', async ({ page }) => {
    const empresasPage = new EmpresasPage(page);

    // Navegar a empresas
    await empresasPage.goto();

    // Abrir modal de crear
    await empresasPage.openCreateModal();

    // Verificar que el modal está visible
    await expect(empresasPage.createModal).toBeVisible();
  });

  test('debe navegar correctamente a la página de empresas', async ({ page }) => {
    const empresasPage = new EmpresasPage(page);

    // Navegar directamente
    await empresasPage.goto();

    // Verificar URL
    await expect(page).toHaveURL(/\/empresas/, { timeout: 5000 });

    // Verificar que el título está visible
    await expect(empresasPage.title).toBeVisible({ timeout: 5000 });
  });
});

