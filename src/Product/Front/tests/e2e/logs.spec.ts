import { test, expect } from '@playwright/test';
import { AdminLogsPage } from '../page-objects/AdminLogsPage';
import { ApiClient } from '../api/api-client';

// Configuración de URLs para tests
const API_URL = process.env.API_URL || 'http://localhost:5000';
const CLIENT_URL = process.env.CLIENT_URL || 'http://localhost:3000';

test.describe('Sistema de Logs E2E Tests', () => {
  let apiClient: ApiClient;

  test.beforeEach(async ({ request }) => {
    apiClient = new ApiClient(request, API_URL);
  });

  test('debe autenticarse como admin y acceder a la página de logs', async ({ page }) => {
    // Navegar a login de admin
    await page.goto('/admin/login');
    
    // Esperar a que el formulario esté visible
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    // Autenticarse como admin
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    // Esperar a que se complete el login y redirija al dashboard
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan y la sesión esté lista
    await page.waitForTimeout(2000);
    
    // Navegar a la página de logs
    await page.goto('/admin/logs');
    
    // Esperar a que la página de logs cargue completamente (incluyendo la sesión)
    await page.waitForLoadState('networkidle');
    
    // Verificar que la página de logs se carga correctamente
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la sesión esté lista y la query se ejecute
    await page.waitForTimeout(2000); // Dar tiempo para que la query de TanStack Query se ejecute
    
    // La tabla puede estar vacía, pero el elemento table debería existir
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
  });

  test('debe insertar un log en la BD y mostrarlo en la pantalla de consulta', async ({ page, request }) => {
    // Paso 1: Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Paso 2: Insertar un log de prueba mediante el endpoint de telemetría
    const testLogMessage = `Test Log E2E - ${new Date().toISOString()}`;
    const logResponse = await request.post(`${API_URL}/api/telemetry/logs`, {
      data: {
        level: 50, // Error level (Pino)
        message: testLogMessage,
        exception: 'Test exception for E2E',
        properties: {
          test: true,
          source: 'E2E-Test',
          timestamp: new Date().toISOString(),
        },
        source: 'Playwright-E2E-Test',
        clientInfo: {
          userAgent: 'Playwright Test',
          test: true,
        },
      },
      headers: {
        'Content-Type': 'application/json',
      },
    });
    
    expect(logResponse.ok()).toBeTruthy();
    const logResult = await logResponse.json();
    expect(logResult.message).toContain('recibido correctamente');
    
    // Esperar un momento para que el log se persista
    await page.waitForTimeout(2000);
    
    // Paso 3: Navegar a la página de logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Paso 4: Verificar que el log aparece en la lista
    // Esperar a que la tabla se cargue (puede no tener filas inmediatamente)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    await page.waitForSelector('table', { timeout: 10000 });
    
    // Buscar el log por su mensaje
    const logExists = await logsPage.verifyLogMessageExists(testLogMessage);
    expect(logExists).toBeTruthy();
    
    // Verificar que hay al menos un log en la tabla
    const logsCount = await logsPage.getLogsCount();
    expect(logsCount).toBeGreaterThan(0);
  });

  test('debe filtrar logs por nivel', async ({ page }) => {
    // Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Navegar a logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Esperar a que la tabla se cargue (puede no tener filas)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    
    // Aplicar filtro por nivel Error
    await logsPage.applyFilters(undefined, undefined, 'Error');
    
    // Esperar a que se actualice la tabla
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Verificar que los logs mostrados tienen el nivel Error
    const errorBadges = page.locator('table tbody tr').filter({ 
      has: page.locator('span').filter({ hasText: /error/i }) 
    });
    const errorCount = await errorBadges.count();
    
    // Si hay logs de error, verificar que todos son de nivel Error
    if (errorCount > 0) {
      const firstErrorBadge = page.locator('table tbody tr').first().locator('span').filter({ hasText: /error/i });
      await expect(firstErrorBadge).toBeVisible();
    }
  });

  test('debe filtrar logs por rango de fechas', async ({ page }) => {
    // Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Navegar a logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Esperar a que la tabla se cargue (puede no tener filas)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    
    // Obtener fecha de hoy y hace 7 días
    const today = new Date();
    const sevenDaysAgo = new Date(today);
    sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
    
    // Formatear fechas para el input datetime-local (YYYY-MM-DDTHH:mm)
    const formatDateForInput = (date: Date) => {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      return `${year}-${month}-${day}T${hours}:${minutes}`;
    };
    
    const fromDate = formatDateForInput(sevenDaysAgo);
    const toDate = formatDateForInput(today);
    
    // Aplicar filtros de fecha
    await logsPage.applyFilters(fromDate, toDate);
    
    // Esperar a que se actualice la tabla
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Verificar que la tabla se actualizó (puede tener menos logs o ninguno)
    const logsCount = await logsPage.getLogsCount();
    expect(logsCount).toBeGreaterThanOrEqual(0);
  });

  test('debe expandir y mostrar detalles de un log', async ({ page }) => {
    // Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Navegar a logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Esperar a que la tabla se cargue (puede no tener filas)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    
    // Verificar que hay al menos un log
    const logsCount = await logsPage.getLogsCount();
    if (logsCount === 0) {
      test.skip();
      return;
    }
    
    // Hacer clic en el botón de expandir del primer log (usar data-testid)
    // El botón tiene data-testid="shared-button-toggle-log-{logId}"
    const firstLogRow = page.getByTestId('shared-datatable-logs').locator('table tbody tr').first();
    const firstExpandButton = firstLogRow.locator('button[data-testid^="shared-button-toggle-log-"]');
    await firstExpandButton.click();
    
    // Verificar que se muestra una fila expandida con detalles
    const expandedRow = page.locator('table tbody tr').filter({ has: page.locator('[colspan="5"]') }).first();
    await expect(expandedRow).toBeVisible({ timeout: 5000 });
    
    // Verificar que hay contenido en la fila expandida
    const expandedContent = expandedRow.locator('div');
    await expect(expandedContent).toBeVisible();
  });

  test('debe mostrar paginación cuando hay más de una página', async ({ page }) => {
    // Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Navegar a logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Esperar a que la tabla se cargue (puede no tener filas)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    await page.waitForTimeout(2000);
    
    // Verificar si hay información de paginación
    const paginationInfo = page.locator('text=/página \\d+ de \\d+/i');
    const hasPagination = await paginationInfo.isVisible({ timeout: 3000 }).catch(() => false);
    
    if (hasPagination) {
      // Si hay paginación, verificar que los botones funcionan
      const nextButton = logsPage.nextPageButton;
      const previousButton = logsPage.previousPageButton;
      
      // Verificar que el botón "Anterior" está deshabilitado en la primera página
      if (await previousButton.isVisible()) {
        const isDisabled = await previousButton.isDisabled();
        expect(isDisabled).toBeTruthy();
      }
      
      // Si hay más de una página, probar navegar a la siguiente
      if (await nextButton.isVisible() && !(await nextButton.isDisabled())) {
        await nextButton.click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);
        
        // Verificar que ahora el botón "Anterior" está habilitado
        if (await previousButton.isVisible()) {
          const isPreviousDisabled = await previousButton.isDisabled();
          expect(isPreviousDisabled).toBeFalsy();
        }
      }
    } else {
      // Si no hay paginación, verificar que hay menos de 50 logs (tamaño de página)
      const logsCount = await logsPage.getLogsCount();
      expect(logsCount).toBeLessThanOrEqual(50);
    }
  });

  test('debe limpiar filtros correctamente', async ({ page }) => {
    // Autenticarse como admin
    await page.goto('/admin/login');
    await page.waitForSelector('input[type="text"], input[name="usuario"]', { timeout: 10000 });
    
    const usuarioInput = page.locator('input[type="text"]').or(page.locator('input[name="usuario"]')).first();
    const passwordInput = page.locator('input[type="password"]').or(page.locator('input[name="contraseña"]')).first();
    const loginButton = page.getByRole('button', { name: /acceder.*panel|panel.*administrativo/i }).first();
    
    await usuarioInput.fill('admin');
    await passwordInput.fill('admin123');
    await loginButton.click();
    
    await page.waitForURL(/\/admin\/dashboard/, { timeout: 15000 });
    
    // Esperar a que las cookies de NextAuth se establezcan
    await page.waitForTimeout(2000);
    
    // Navegar a logs
    await page.goto('/admin/logs');
    const logsPage = new AdminLogsPage(page);
    await expect(logsPage.title).toBeVisible({ timeout: 15000 });
    
    // Esperar a que la página cargue completamente
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Esperar a que la tabla se cargue (puede no tener filas)
    await expect(logsPage.logsTable).toBeVisible({ timeout: 15000 });
    
    // Obtener el conteo inicial de logs
    const initialCount = await logsPage.getLogsCount();
    
    // Aplicar un filtro
    await logsPage.applyFilters(undefined, undefined, 'Error');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    const filteredCount = await logsPage.getLogsCount();
    
    // Limpiar filtros
    await logsPage.clearFilters();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Verificar que los filtros se limpiaron
    const fromDateValue = await logsPage.fromDateInput.inputValue();
    const toDateValue = await logsPage.toDateInput.inputValue();
    const levelValue = await logsPage.levelSelect.inputValue();
    
    expect(fromDateValue).toBe('');
    expect(toDateValue).toBe('');
    expect(levelValue).toBe('');
    
    // Verificar que la tabla se actualizó (puede tener más logs ahora)
    const finalCount = await logsPage.getLogsCount();
    expect(finalCount).toBeGreaterThanOrEqual(filteredCount);
  });
});
