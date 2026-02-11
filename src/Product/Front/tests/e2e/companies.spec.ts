import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { CompaniesPage } from '../page-objects/CompaniesPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';
import { DEMO_COMPANY_NAME } from '../../lib/legacy-constants';

test.describe('Companies E2E Tests', () => {
  let cleanup: TestDataCleanup;
  const createdCompanyNames: string[] = [];

  test.beforeEach(async ({ page, request }) => {
    // Usamos limpieza manual si es necesario, aunque Playwright suele limpiar contexto
    // Aquí podríamos necesitar limpiar vía API si persistimos datos reales

    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(DEMO_COMPANY_NAME, 'admin', 'admin123');
    await loginPage.verifyLoginSuccess();

    // Esperar navegación
    await page.waitForURL(/\/dashboard/, { timeout: 5000 });
  });

  test('debe crear una nueva company correctamente', async ({ page }) => {
    const companiesPage = new CompaniesPage(page);
    const uniqueId = Date.now().toString();
    const newCompanyName = `Company Test E2E ${uniqueId}`;

    // 1. Navegar a Companies
    await companiesPage.goto();

    // 2. Crear Company
    await companiesPage.createCompany(
        newCompanyName,
        `B${uniqueId.substring(0, 8)}`, // Fake CIF
        `test-${uniqueId}@example.com`,
        'Calle Falsa 123'
    );

    // 3. Verificar que aparece en la lista
    await companiesPage.verifyCompanyExists(newCompanyName);

    // Guardar para limpieza (si implementamos limpieza vía API después)
    createdCompanyNames.push(newCompanyName);
  });
});
