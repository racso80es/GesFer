import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { CompaniesPage } from '../page-objects/CompaniesPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';
import { DEMO_COMPANY_NAME } from '../../lib/legacy-constants';

test.describe('My Company E2E Tests (Update Only)', () => {
  const originalName = DEMO_COMPANY_NAME;
  const updatedName = `${DEMO_COMPANY_NAME} Updated`;

  test.beforeEach(async ({ page }) => {
    // Login
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(DEMO_COMPANY_NAME, 'admin', 'admin123');
    await loginPage.verifyLoginSuccess();
  });

  test.afterEach(async ({ page }) => {
    // Intentar revertir cambios
    try {
      const companiesPage = new CompaniesPage(page);
      await companiesPage.goto();
      await companiesPage.updateCompany({
          name: originalName,
          address: 'Calle Falsa 123' // Default demo data
      });
    } catch (e) {
      console.log('Cleanup failed', e);
    }
  });

  test('debe cargar y actualizar los datos de mi organización', async ({ page }) => {
    const companiesPage = new CompaniesPage(page);

    // 1. Navegar a Companies (ahora es Mi Organización)
    await companiesPage.goto();
    await expect(companiesPage.title).toBeVisible();

    // 2. Verificar que se cargan los datos (el nombre debería coincidir)
    // Puede que tarde un poco en cargar el form
    await expect(companiesPage.nameInput).toHaveValue(originalName, { timeout: 10000 });

    // 3. Actualizar datos
    await companiesPage.updateCompany({
        name: updatedName,
        address: 'Nueva Dirección 123'
    });

    // 4. Verificar feedback (opcional, si hay toast/mensaje)
    // await expect(page.getByText(/actualizada correctamente/i)).toBeVisible();

    // 5. Verificar persistencia (recargar página)
    await page.reload();
    await expect(companiesPage.nameInput).toHaveValue(updatedName, { timeout: 10000 });
    await expect(companiesPage.addressInput).toHaveValue('Nueva Dirección 123');
  });
});
