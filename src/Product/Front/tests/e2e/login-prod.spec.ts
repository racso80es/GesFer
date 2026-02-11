import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { DashboardPage } from '../page-objects/DashboardPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';
import { appConfig } from '../../lib/config';
import { DEMO_COMPANY_NAME } from '../../lib/legacy-constants';

/**
 * Test E2E de Login para el entorno de Producción
 *
 * Verifica que el flujo de login funciona correctamente en el entorno desplegado/containerizado.
 * Intercepta la petición de autenticación para asegurar que se envía el payload correcto.
 *
 * Requisitos:
 * - Servicios levantados (Backend API, Frontend)
 * - Base de datos con datos de prueba (Company Demo / admin / admin123)
 */
test.describe('Login E2E Tests (Production Environment)', () => {
  let cleanup: TestDataCleanup;

  test.beforeEach(async ({ request, page }) => {
    // Limpiar estado local antes de cada test
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
      document.cookie.split(";").forEach(function(c) {
        document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
      });
    });

    // Inicializar helper de limpieza (aunque para login puro no creamos datos, es buena práctica tenerlo)
    cleanup = new TestDataCleanup(request, process.env.API_URL || appConfig.api.url);
  });

  test.afterEach(async () => {
    await cleanup.cleanup();
  });

  test('debe realizar login exitoso con payload correcto y redirección', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const dashboardPage = new DashboardPage(page);

    // Credenciales de prueba
    const credentials = {
      organization: DEMO_COMPANY_NAME,
      username: 'admin',
      password: 'admin123'
    };

    // Interceptar la petición de login para verificar el payload
    // Nota: La URL puede variar según si es llamada cliente->servidor Next.js o servidor Next.js->API
    // En este caso, interceptamos la llamada que hace el cliente a NextAuth o la que hace NextAuth a la API si fuera visible (client-side fetch)
    // Dado que usamos NextAuth con CredentialsProvider, la llamada inicial es a /api/auth/callback/credentials
    const loginRequestPromise = page.waitForRequest(request =>
      request.url().includes('/api/auth/callback/credentials') && request.method() === 'POST'
    );

    // 1. Navegar a login
    await loginPage.goto();

    // 2. Realizar login
    await loginPage.login(credentials.organization, credentials.username, credentials.password);

    // 3. Verificar payload de la petición
    const request = await loginRequestPromise;
    const formData = new URLSearchParams(request.postData() || '');

    // NextAuth envía los datos como form-data en el body
    expect(formData.get('company')).toBe(credentials.organization);
    expect(formData.get('username')).toBe(credentials.username);
    expect(formData.get('password')).toBe(credentials.password);

    // 4. Verificar éxito visual y redirección
    await loginPage.verifyLoginSuccess();

    // 5. Verificar que estamos en el dashboard
    await expect(dashboardPage.title).toBeVisible();

    // Opcional: Verificar que la cookie de sesión existe
    const cookies = await page.context().cookies();
    const sessionCookie = cookies.find(c => c.name.includes('authjs.session-token') || c.name.includes('next-auth.session-token'));
    expect(sessionCookie).toBeDefined();
  });
});
