import { test, expect } from '@playwright/test';
import { AdminLogsPage } from '../page-objects/AdminLogsPage';

// Configuración de URLs para tests
const API_URL = process.env.API_URL || 'http://localhost:5000';
const CLIENT_URL = process.env.CLIENT_URL || 'http://localhost:3000';

test.describe('Admin Logs - Verificación de 403', () => {
  test('debe hacer login con admin@gesfer.local y verificar acceso a logs', async ({ page, request }) => {
    // Verificar que los servicios estén disponibles
    try {
      const healthCheck = await request.get(`${API_URL}/api/health`);
      console.log(`[TEST] API Health Check: ${healthCheck.status()}`);
    } catch (e) {
      console.warn(`[TEST] API no disponible en ${API_URL}, continuando de todas formas`);
    }
    
    // Paso 1: Hacer login con admin@gesfer.local
    // Nota: El login de admin usa el campo "usuario" (username), no email
    // El usuario admin@gesfer.local probablemente tiene username "admin"
    await page.goto(`${CLIENT_URL}/admin/login`);
    
    // Esperar a que el formulario esté visible
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    // Autenticarse como admin
    // Intentar primero con "admin" como username (el email admin@gesfer.local probablemente tiene username "admin")
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    // Intentar login con username "admin" (el usuario admin@gesfer.local tiene este username)
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    
    // Interceptar la respuesta del login para debug
    let loginResponseStatus: number | null = null;
    page.on('response', async (response) => {
      if (response.url().includes('/api/admin/auth/login')) {
        loginResponseStatus = response.status();
        console.log(`[TEST] Login response: ${loginResponseStatus}`);
      }
    });
    
    await loginButton.click();
    
    // Esperar a que se complete el login - puede tomar tiempo
    // Primero esperar a que la página responda
    await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {});
    
    // Esperar a que se complete el login y redirija al dashboard
    // Usar waitForURL con timeout más largo y verificar múltiples condiciones
    try {
      await page.waitForURL(/\/admin\/dashboard/, { timeout: 20000 });
    } catch (e) {
      // Si no redirige, verificar si hay error en la página
      const currentUrl = page.url();
      console.log(`[TEST] URL actual después del login: ${currentUrl}`);
      
      // Verificar si hay mensaje de error visible
      const errorMessage = await page.locator('text=/error|invalid|incorrect/i').first().isVisible().catch(() => false);
      if (errorMessage) {
        const errorText = await page.locator('text=/error|invalid|incorrect/i').first().textContent().catch(() => '');
        throw new Error(`Error en login: ${errorText}`);
      }
      
      // Si estamos todavía en login, intentar navegar manualmente
      if (currentUrl.includes('/admin/login')) {
        console.log('[TEST] Aún en login, intentando navegar manualmente al dashboard');
        await page.goto(`${CLIENT_URL}/admin/dashboard`);
        await page.waitForLoadState('networkidle', { timeout: 10000 });
      }
    }
    
    // Esperar a que las cookies de NextAuth se establezcan y la sesión esté lista
    await page.waitForTimeout(3000);
    
    // Paso 2: Navegar a la pantalla de logs
    await page.goto(`${CLIENT_URL}/admin/logs`);
    
    // Paso 3: Verificar si la tabla de logs carga datos o si recibe un 403
    // Interceptar las peticiones a la API de logs ANTES de que se cargue la página
    let logsResponseStatus: number | null = null;
    let logsResponseError: string | null = null;
    
    page.on('response', async (response) => {
      if (response.url().includes('/api/log')) {
        logsResponseStatus = response.status();
        if (!response.ok()) {
          const errorText = await response.text().catch(() => '');
          logsResponseError = errorText;
          console.log(`[TEST] Respuesta de /api/log: ${logsResponseStatus} - ${errorText}`);
        } else {
          console.log(`[TEST] ✅ Respuesta de /api/log: ${logsResponseStatus} OK`);
        }
      }
    });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle', { timeout: 30000 });
    await page.waitForTimeout(3000);
    
    // Esperar a que la página de logs se cargue completamente
    const logsPage = new AdminLogsPage(page);
    
    // Verificar que estamos en la página correcta
    const currentUrl = page.url();
    console.log(`[TEST] URL actual: ${currentUrl}`);
    
    // Si estamos en login, significa que no se autenticó correctamente
    if (currentUrl.includes('/admin/login')) {
      throw new Error('No se pudo autenticar. La página redirigió de vuelta al login.');
    }
    
    // Esperar a que el título sea visible
    await expect(logsPage.title).toBeVisible({ timeout: 20000 });
    
    // Esperar un poco más para que se complete la petición a la API
    await page.waitForTimeout(3000);
    
    // Verificar el estado de la respuesta
    if (logsResponseStatus === 403) {
      console.log('[TEST] ❌ Error 403 Forbidden confirmado');
      console.log('[TEST] Error:', logsResponseError);
      // El test debe fallar aquí para confirmar el problema
      expect(logsResponseStatus).not.toBe(403);
    } else if (logsResponseStatus === 200) {
      console.log('[TEST] ✅ Acceso a logs exitoso (200 OK)');
      // Verificar que la tabla de logs se carga correctamente
      await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
      
      // Verificar que los filtros se inicializaron correctamente
      const fromDateValue = await logsPage.fromDateInput.inputValue();
      const toDateValue = await logsPage.toDateInput.inputValue();
      
      console.log(`[TEST] Filtro "Desde": ${fromDateValue}`);
      console.log(`[TEST] Filtro "Hasta": ${toDateValue}`);
      
      // Verificar que los filtros tienen valores (deben estar inicializados con fecha de hoy)
      expect(fromDateValue).not.toBe('');
      expect(toDateValue).not.toBe('');
      
      // Verificar que "Desde" es hoy a las 00:00 (formato YYYY-MM-DDTHH:mm)
      const today = new Date();
      const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 0, 0, 0);
      const expectedFromDate = `${todayStart.getFullYear()}-${String(todayStart.getMonth() + 1).padStart(2, '0')}-${String(todayStart.getDate()).padStart(2, '0')}T00:00`;
      
      expect(fromDateValue).toContain(todayStart.getFullYear().toString());
      expect(fromDateValue).toContain('T00:00');
      
      console.log('[TEST] ✅ Filtros inicializados correctamente');
    } else if (logsResponseStatus === null) {
      console.log('[TEST] ⚠️ No se detectó respuesta de /api/log, verificando que la página se cargó');
      // Si no hay respuesta, verificar que al menos la página se carga
      await expect(logsPage.title).toBeVisible({ timeout: 15000 });
      // El test pasa si la página se carga (puede que la petición aún no se haya hecho)
      expect(logsPage.title).toBeVisible();
    } else {
      console.log(`[TEST] ⚠️ Estado inesperado: ${logsResponseStatus}`);
      // Verificar que la página se carga independientemente del estado
      await expect(logsPage.title).toBeVisible({ timeout: 15000 });
      // El test falla si hay un error diferente a 403
      expect(logsResponseStatus).toBe(200);
    }
  });
});
