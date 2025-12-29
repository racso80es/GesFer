import { test as base } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { ApiClient } from '../api/api-client';
import { appConfig } from '../../lib/config';

/**
 * Fixture para autenticación
 * Proporciona un usuario autenticado para los tests
 */
export const test = base.extend<{
  authenticatedPage: { page: any; token: string };
  apiClient: ApiClient;
}>({
  // Fixture para página autenticada
  authenticatedPage: async ({ page, request }, use) => {
    const loginPage = new LoginPage(page);
    const apiClient = new ApiClient(request, appConfig.api.url);

    // Realizar login vía API para obtener token
    const token = await apiClient.login('Empresa Demo', 'admin', 'admin123');

    // También hacer login en la UI para tener la sesión
    await loginPage.goto();
    await loginPage.login('Empresa Demo', 'admin', 'admin123');
    await loginPage.verifyLoginSuccess();

    await use({ page, token });
  },

  // Fixture para cliente API
  apiClient: async ({ request }, use) => {
    const apiClient = new ApiClient(request, process.env.API_URL || appConfig.api.url);
    await use(apiClient);
  },
});

export { expect } from '@playwright/test';

