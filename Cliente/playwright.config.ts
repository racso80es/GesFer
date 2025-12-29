import { defineConfig, devices } from '@playwright/test';

/**
 * Configuración de Playwright para GesFer
 * 
 * URLs configuradas:
 * - Web: http://localhost:3000
 * - API: http://localhost:5000
 */
export default defineConfig({
  testDir: './tests',
  
  /* Tiempo máximo para ejecutar un test */
  timeout: 30 * 1000,
  
  /* Tiempo de espera para expect */
  expect: {
    timeout: 5000,
  },
  
  /* Ejecutar tests en paralelo */
  fullyParallel: true,
  
  /* Fallar el build en CI si accidentalmente dejaste test.only en el código */
  forbidOnly: !!process.env.CI,
  
  /* Reintentar en CI solo si falla */
  retries: process.env.CI ? 2 : 0,
  
  /* Número de workers en CI, o indefinido en local */
  workers: process.env.CI ? 1 : undefined,
  
  /* Reporter a usar */
  reporter: [
    ['html'],
    ['list'],
    ['json', { outputFile: 'test-results/results.json' }],
  ],
  
  /* Configuración compartida para todos los proyectos */
  use: {
    /* URL base para usar en navegación, por ejemplo, await page.goto('/') */
    baseURL: 'http://localhost:3000',
    
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
  projects: [
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

    /* Test en dispositivos móviles */
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
  ],

  /* Servidor de desarrollo local - Deshabilitado porque se ejecuta manualmente */
  // webServer: {
  //   command: 'npm run dev',
  //   url: 'http://localhost:3000',
  //   reuseExistingServer: true,
  //   timeout: 120 * 1000,
  // },
});

