/**
 * Global setup de Playwright: comprueba que la API Product (o mock) esté en ejecución
 * antes de lanzar los tests.
 *
 * - Sin USE_MOCK_API: exige API real en TEST_API_URL (p. ej. 5000).
 * - Con USE_MOCK_API=1: usa API_URL (p. ej. 5002) y comprueba el mock en infrastructure/mock-apis.
 */
const useMockApi = process.env.USE_MOCK_API === '1' || process.env.USE_MOCK_API === 'true';
const apiBase = useMockApi
  ? (process.env.API_URL || 'http://127.0.0.1:5002')
  : (process.env.API_URL || 'https://127.0.0.1:5001');

const API_HEALTH_URL = `${apiBase.replace(/\/$/, '')}/health`;

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
    const msg = err instanceof Error ? err.message : String(err);
    const hint = useMockApi
      ? 'Levanta el mock: cd infrastructure/mock-apis && npm install && npm start. Ver infrastructure/mock-apis/README.md'
      : 'Levanta la API con perfil HTTPS (puerto 5001) o usa mock: USE_MOCK_API=1 API_URL=http://localhost:5002. Ver docs/operations/RUNBOOK_LOGIN_EMERGENCY.md';
    throw new Error(
      `[E2E] La API Product no está disponible en ${apiBase}. ${hint} (Error: ${msg})`
    );
  }
}

export default globalSetup;
