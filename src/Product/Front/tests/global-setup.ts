/**
 * Global setup de Playwright: comprueba que la API Product esté en ejecución
 * antes de lanzar los tests. Si no está, falla con un mensaje claro en lugar
 * de 32 timeouts en el reporte (localhost:9323).
 */
import { TEST_API_URL } from '../lib/config.test';

const API_HEALTH_URL = `${TEST_API_URL.replace(/\/$/, '')}/health`;

async function globalSetup(): Promise<void> {
  try {
    const res = await fetch(API_HEALTH_URL, {
      method: 'GET',
      signal: AbortSignal.timeout(5000),
    });
    if (!res.ok) {
      throw new Error(`API respondió con ${res.status}`);
    }
  } catch (err) {
    const msg =
      err instanceof Error ? err.message : String(err);
    throw new Error(
      `[E2E] La API Product no está disponible en ${TEST_API_URL}. ` +
        `Levanta la API (puerto 5000) antes de ejecutar los tests. ` +
        `(Error: ${msg}). ` +
        `Ver docs/operations/RUNBOOK_LOGIN_EMERGENCY.md o docker-compose up -d gesfer-product-api.`
    );
  }
}

export default globalSetup;
