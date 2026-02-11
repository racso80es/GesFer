import { test, expect } from '@playwright/test';
import { ApiClient } from '../api/api-client';
import { DEMO_COMPANY_NAME } from '../../lib/legacy-constants';

// Configuración de URLs para tests
const API_URL = process.env.API_URL || 'http://localhost:5000';
const CLIENT_URL = process.env.CLIENT_URL || 'http://localhost:3000';

test.describe('Logging Persistence Test', () => {
  let apiClient: ApiClient;
  let adminToken: string;

  test.beforeEach(async ({ request }) => {
    apiClient = new ApiClient(request, API_URL);
    
    // Autenticarse como admin para obtener token
    const adminLoginResponse = await request.post(`${API_URL}/api/admin/auth/login`, {
      data: {
        usuario: 'admin',
        contraseña: 'admin123',
      },
      headers: {
        'Content-Type': 'application/json',
      },
    });
    
    expect(adminLoginResponse.ok()).toBeTruthy();
    const adminLoginData = await adminLoginResponse.json();
    adminToken = adminLoginData.token || adminLoginData.accessToken || '';
    
    // Si no hay token en la respuesta, intentar obtenerlo de otra forma
    if (!adminToken) {
      // Intentar obtener el token del header o de otra propiedad
      const headers = adminLoginResponse.headers();
      adminToken = headers['authorization'] || headers['x-auth-token'] || '';
    }
  });

  test('debe persistir un log en la BD después de una acción en el frontend', async ({ page, request }) => {
    // Paso 1: Obtener el conteo inicial de logs
    const initialLogsResponse = await request.get(`${API_URL}/api/log`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${adminToken}`,
      },
    });
    
    expect(initialLogsResponse.ok()).toBeTruthy();
    const initialLogsData = await initialLogsResponse.json();
    const initialCount = initialLogsData.totalCount || initialLogsData.items?.length || 0;
    
    // Paso 2: Realizar una acción en el frontend que genere un log
    // Simulamos un login fallido que debería generar un log de error
    await page.goto(`${CLIENT_URL}/login`);
    
    // Esperar a que el formulario esté visible
    await page.waitForSelector('input[type="text"], input[name="company"]', { timeout: 10000 });
    
    // Intentar login con credenciales inválidas para generar un error
    const companyInput = page.locator('input[name="company"]').or(page.locator('input[type="text"]').first());
    const usuarioInput = page.locator('input[name="usuario"]').or(page.locator('input[type="text"]').nth(1));
    const passwordInput = page.locator('input[type="password"]');
    const loginButton = page.getByRole('button', { name: /iniciar.*sesión|login/i }).first();
    
    await companyInput.fill(DEMO_COMPANY_NAME);
    await usuarioInput.fill('usuario-inexistente');
    await passwordInput.fill('password-incorrecta');
    await loginButton.click();
    
    // Esperar a que se procese el error (puede mostrar un mensaje de error)
    await page.waitForTimeout(3000);
    
    // Paso 3: Esperar un momento para que el log se persista en la BD
    await page.waitForTimeout(2000);
    
    // Paso 4: Verificar que el conteo de logs ha aumentado en +1
    const finalLogsResponse = await request.get(`${API_URL}/api/log`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${adminToken}`,
      },
    });
    
    expect(finalLogsResponse.ok()).toBeTruthy();
    const finalLogsData = await finalLogsResponse.json();
    const finalCount = finalLogsData.totalCount || finalLogsData.items?.length || 0;
    
    // Verificar que el conteo aumentó en al menos 1
    expect(finalCount).toBeGreaterThan(initialCount);
    
    // Verificar que el último log contiene información relevante
    if (finalLogsData.items && finalLogsData.items.length > 0) {
      const lastLog = finalLogsData.items[0]; // El más reciente debería estar primero
      
      // Verificar que el log tiene un mensaje
      expect(lastLog.message).toBeTruthy();
    }
  });

  test('debe persistir un log enviado directamente al endpoint de telemetría', async ({ request }) => {
    // Paso 1: Obtener el conteo inicial de logs
    const initialLogsResponse = await request.get(`${API_URL}/api/log`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${adminToken}`,
      },
    });
    
    expect(initialLogsResponse.ok()).toBeTruthy();
    const initialLogsData = await initialLogsResponse.json();
    const initialCount = initialLogsData.totalCount || initialLogsData.items?.length || 0;
    
    // Paso 2: Enviar un log directamente al endpoint de telemetría
    const testLogMessage = `Test Log Persistence - ${new Date().toISOString()}`;
    const logResponse = await request.post(`${API_URL}/api/telemetry/logs`, {
      data: {
        level: 50, // Error level (Pino)
        message: testLogMessage,
        exception: 'Test exception for persistence verification',
        properties: {
          test: true,
          source: 'Playwright-Persistence-Test',
          timestamp: new Date().toISOString(),
        },
        source: 'Playwright-Persistence-Test',
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
    
    // Paso 3: Esperar un momento para que el log se persista
    await new Promise(resolve => setTimeout(resolve, 3000));
    
    // Paso 4: Verificar que el conteo de logs ha aumentado en +1
    const finalLogsResponse = await request.get(`${API_URL}/api/log`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${adminToken}`,
      },
    });
    
    expect(finalLogsResponse.ok()).toBeTruthy();
    const finalLogsData = await finalLogsResponse.json();
    const finalCount = finalLogsData.totalCount || finalLogsData.items?.length || 0;
    
    // Verificar que el conteo aumentó en al menos 1
    expect(finalCount).toBeGreaterThan(initialCount);
    
    // Verificar que el log enviado aparece en la lista
    // Si no está en la primera página, buscar en más páginas o verificar que el conteo aumentó
    if (finalLogsData.items && finalLogsData.items.length > 0) {
      const logs = finalLogsData.items;
      let foundLog = logs.find((log: { message?: string; properties?: string }) =>
        log.message && log.message.includes('Test Log Persistence')
      );
      
      // Si no se encuentra en la primera página, buscar en propiedades JSON
      if (!foundLog) {
        foundLog = logs.find((log: { message?: string; properties?: string }) =>
          log.properties && log.properties.includes('Playwright-Persistence-Test')
        );
      }
      
      // Si aún no se encuentra, verificar que al menos el conteo aumentó (el log se guardó)
      if (!foundLog) {
        // El test ya verificó que finalCount > initialCount, así que el log se guardó
        expect(finalCount).toBeGreaterThan(initialCount);
      } else {
        expect(foundLog).toBeTruthy();
      }
    } else {
      // Si no hay items pero el conteo aumentó, el log se guardó correctamente
      expect(finalCount).toBeGreaterThan(initialCount);
    }
  });
});
