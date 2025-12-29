import { defineConfig, devices } from '@playwright/test';
import { TEST_CLIENT_URL } from './lib/config.test';

/**
 * Configuración de Playwright para GesFer
 * 
 * URLs configuradas desde lib/config.test.ts:
 * - Web: desde TEST_CLIENT_URL
 * - API: desde TEST_API_URL
 */
export default defineConfig({
  testDir: './tests',
  
  /* Tiempo máximo para ejecutar un test */
  timeout: 20 * 1000, // Reducido de 30s a 20s
  
  /* Tiempo de espera para expect */
  expect: {
    timeout: 3000, // Reducido de 5s a 3s
  },
  
  /* Ejecutar tests en paralelo */
  fullyParallel: true,
  
  /* Fallar el build en CI si accidentalmente dejaste test.only en el código */
  forbidOnly: !!process.env.CI,
  
  /* Reintentar en CI solo si falla */
  retries: process.env.CI ? 2 : 0,
  
  /* Número de workers: más workers = más rápido, pero más uso de recursos */
  workers: process.env.CI ? 1 : 4, // Aumentado a 4 workers en local
  
  /* Reporter a usar */
  reporter: [
    ['html'],
    ['list'],
    ['json', { outputFile: 'test-results/results.json' }],
  ],
  
  /* Configuración compartida para todos los proyectos */
  use: {
    /* URL base para usar en navegación, por ejemplo, await page.goto('/') */
    baseURL: process.env.CLIENT_URL || TEST_CLIENT_URL,
    
    /* Recopilar trace cuando se reintenta el test fallido */
    trace: 'on-first-retry',
    
    /* Screenshots solo cuando falla */
    screenshot: 'only-on-failure',
    
    /* Video solo cuando falla */
    video: 'retain-on-failure',
  },

  /* Variables de entorno para tests */
  globalSetup: undefined,
  
  /* Configuración de API */
  globalTeardown: undefined,

  /* Configurar proyectos para diferentes navegadores */
  /* En desarrollo, solo usar Chromium para velocidad. En CI, usar todos */
  projects: process.env.CI ? [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ] : [
    // Solo Chromium en desarrollo para velocidad
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /* Servidor de desarrollo local - Deshabilitado porque se ejecuta manualmente */
  // webServer: {
  //   command: 'npm run dev',
  //   url: TEST_CLIENT_URL,
  //   reuseExistingServer: true,
  //   timeout: 120 * 1000,
  // },
});

