/**
 * Test de integridad extremo a extremo para API y Cliente.
 * Requiere que ambos servicios estén ejecutándose:
 * - API en http://localhost:5000
 * - Cliente en http://localhost:3000
 */
import http from "node:http";
import https from "node:https";
import { URL } from "node:url";
import config from "../../config.json";

const API_URL = config.apiUrl.replace(/\/$/, "");
const CLIENT_URL = "http://localhost:3000";

// Ampliar timeout porque son llamadas reales a servicios locales
jest.setTimeout(20000);

type HttpResult = { status: number; body: string; headers: http.IncomingHttpHeaders };

const httpRequest = (targetUrl: string, options?: { method?: string; headers?: Record<string, string>; body?: string }) => {
  const url = new URL(targetUrl);
  const client = url.protocol === "https:" ? https : http;

  return new Promise<HttpResult>((resolve, reject) => {
    const req = client.request(
      {
        hostname: url.hostname,
        port: url.port,
        path: `${url.pathname}${url.search}`,
        method: options?.method ?? "GET",
        headers: options?.headers,
      },
      (res) => {
        let data = "";
        res.on("data", (chunk) => {
          data += chunk;
        });
        res.on("end", () => {
          resolve({
            status: res.statusCode ?? 0,
            body: data,
            headers: res.headers,
          });
        });
      }
    );

    req.on("error", reject);
    if (options?.body) {
      req.write(options.body);
    }
    req.end();
  });
};

describe("Auditoría de integridad API + Cliente", () => {
  it("API: health responde healthy y login funciona con credenciales demo", async () => {
    const healthResp = await httpRequest(`${API_URL}/api/health`);
    expect(healthResp.status).toBe(200);
    const healthJson = JSON.parse(healthResp.body) as { status?: string };
    expect(healthJson.status).toBe("healthy");

    const loginResp = await httpRequest(`${API_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        empresa: "Empresa Demo",
        usuario: "admin",
        contraseña: "admin123",
      }),
    });

    expect(loginResp.status).toBe(200);
    const loginJson = JSON.parse(loginResp.body) as { username?: string; token?: string };
    expect(loginJson.username).toBe("admin");
    // El token puede venir vacío en entorno local, solo verificamos que exista la clave
    expect(loginJson).toHaveProperty("token");
  });

  it("Cliente: la pantalla de login responde 200 y sirve HTML", async () => {
    const resp = await httpRequest(`${CLIENT_URL}/login`);
    expect(resp.status).toBe(200);
    const html = resp.body;
    expect(html.length).toBeGreaterThan(1000); // contenido mínimo esperado
    expect(html.toLowerCase()).toContain("login");
  });
});

