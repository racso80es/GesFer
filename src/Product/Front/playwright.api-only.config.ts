/**
 * Configuración Playwright solo para tests de API (sin frontend).
 *
 * Permite testear la API como cliente sin levantar el cliente Next.js.
 * Útil cuando no está el frontend o cuando se valida contra el mock.
 *
 * Uso:
 *   npx playwright test -c playwright.api-only.config.ts
 *   USE_MOCK_API=1 npx playwright test -c playwright.api-only.config.ts tests/api/
 */
import { defineConfig, devices } from '@playwright/test';
import { TEST_API_URL } from './lib/config.test';

export default defineConfig({
  testDir: './tests',
  testMatch: '**/api/**/*.spec.ts',

  timeout: 20 * 1000,
  expect: { timeout: 3000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 2,

  reporter: [
    ['html'],
    ['list'],
    ['json', { outputFile: 'test-results/api-results.json' }],
  ],

  use: {
    baseURL: process.env.API_URL || TEST_API_URL,
    trace: 'on-first-retry',
  },

  globalSetup: './tests/global-setup.ts',

  projects: [
    {
      name: 'api',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Sin webServer: no se levanta el frontend; solo se testea la API como cliente.
});
