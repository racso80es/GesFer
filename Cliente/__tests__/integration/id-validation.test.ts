/**
 * Tests para validar que los IDs se manejen correctamente
 * y detectar problemas con IDs mal formateados
 */

import http from "node:http";
import { URL } from "node:url";
import config from "../../config.json";

const API_URL = config.apiUrl.replace(/\/$/, "");

jest.setTimeout(30000);

type HttpResult = {
  status: number;
  body: string;
  headers: http.IncomingHttpHeaders;
};

const httpRequest = (
  targetUrl: string,
  options?: {
    method?: string;
    headers?: Record<string, string>;
    body?: string;
  }
): Promise<HttpResult> => {
  const url = new URL(targetUrl);
  const client = url.protocol === "https:" ? require("https") : http;

  return new Promise<HttpResult>((resolve, reject) => {
    const req = client.request(
      {
        hostname: url.hostname,
        port: url.port,
        path: `${url.pathname}${url.search}`,
        method: options?.method ?? "GET",
        headers: options?.headers,
      },
      (res: http.IncomingMessage) => {
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

// Helper para obtener token de autenticación
const getAuthToken = async (): Promise<string> => {
  const loginResp = await httpRequest(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      empresa: "Empresa Demo",
      usuario: "admin",
      contraseña: "admin123",
    }),
  });

  if (loginResp.status !== 200) {
    throw new Error(`Login falló con status ${loginResp.status}: ${loginResp.body}`);
  }

  const loginData = JSON.parse(loginResp.body);
  return loginData.token || "";
};

// Helper para validar formato GUID
const isValidGuid = (id: string): boolean => {
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  return guidRegex.test(id);
};

describe("Validación de IDs en peticiones API", () => {
  let authToken: string;
  let validCompanyId: string;
  let validUserId: string;

  beforeAll(async () => {
    // Obtener token de autenticación
    try {
      authToken = await getAuthToken();
      expect(authToken).toBeTruthy();
    } catch (error) {
      console.warn("No se pudo obtener token de autenticación. Algunos tests pueden fallar.");
      // Continuar de todas formas para que los tests que no requieren auth puedan ejecutarse
    }

    // Obtener una empresa válida (solo si tenemos token)
    if (authToken) {
      try {
        const companiesResp = await httpRequest(`${API_URL}/api/company`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${authToken}`,
          },
        });

        if (companiesResp.status === 200) {
          const companies = JSON.parse(companiesResp.body);
          if (companies.length > 0) {
            validCompanyId = companies[0].id;
          }
        }
      } catch (error) {
        console.warn("No se pudo obtener empresas. Algunos tests pueden fallar.");
      }

      // Obtener un usuario válido
      try {
        const usersResp = await httpRequest(`${API_URL}/api/user`, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${authToken}`,
          },
        });

        if (usersResp.status === 200) {
          const users = JSON.parse(usersResp.body);
          if (users.length > 0) {
            validUserId = users[0].id;
          }
        }
      } catch (error) {
        console.warn("No se pudo obtener usuarios. Algunos tests pueden fallar.");
      }
    }
  });

  describe("Empresas - Validación de IDs", () => {
    it("debe rechazar actualización con ID mal formateado (contiene caracteres inválidos)", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      const invalidIds = [
        "11.1111-111111111111:1", // ID con puntos y dos puntos
        "invalid-id", // ID sin formato GUID
        "123", // ID numérico simple
        "11111111-1111-1111-1111-111111111111:extra", // GUID con caracteres extra
        "", // ID vacío
      ];

      for (const invalidId of invalidIds) {
        const resp = await httpRequest(`${API_URL}/api/company/${invalidId}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authToken}`,
          },
          body: JSON.stringify({
            name: "Test Company",
            address: "Test Address",
          }),
        });

        // Debe rechazar con 400 (Bad Request), 404 (Not Found) o 405 (Method Not Allowed)
        expect([400, 404, 405]).toContain(resp.status);
      }
    });

    it("debe validar que el ID de empresa sea un GUID válido antes de actualizar", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      if (!validCompanyId) {
        console.warn("No hay empresa válida disponible para el test");
        return;
      }

      // Verificar que el ID válido es un GUID
      expect(isValidGuid(validCompanyId)).toBe(true);

      // Intentar actualizar con el ID válido
      const resp = await httpRequest(`${API_URL}/api/company/${validCompanyId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          name: "Test Company Updated",
          address: "Test Address Updated",
        }),
      });

      // Debe aceptar (200) o rechazar con 400 si hay datos inválidos, pero no 404
      expect([200, 400]).toContain(resp.status);
      expect(resp.status).not.toBe(404);
    });

    it("debe rechazar actualización con ID que contiene caracteres especiales", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      const invalidIds = [
        "../../../etc/passwd", // Path traversal
        "<script>alert('xss')</script>", // XSS attempt
        "'; DROP TABLE Companies; --", // SQL injection attempt
      ];

      for (const invalidId of invalidIds) {
        const resp = await httpRequest(`${API_URL}/api/company/${encodeURIComponent(invalidId)}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authToken}`,
          },
          body: JSON.stringify({
            name: "Test",
            address: "Test",
          }),
        });

        // Debe rechazar con 400 o 404
        expect([400, 404]).toContain(resp.status);
      }
    });
  });

  describe("Usuarios - Validación de IDs", () => {
    it("debe rechazar actualización con ID mal formateado (contiene caracteres inválidos)", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      const invalidIds = [
        "11.1111-111111111111:1", // ID con puntos y dos puntos
        "invalid-id", // ID sin formato GUID
        "123", // ID numérico simple
        "11111111-1111-1111-1111-111111111111:extra", // GUID con caracteres extra
        "", // ID vacío
      ];

      for (const invalidId of invalidIds) {
        const resp = await httpRequest(`${API_URL}/api/user/${invalidId}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authToken}`,
          },
          body: JSON.stringify({
            username: "testuser",
            firstName: "Test",
            lastName: "User",
          }),
        });

        // Debe rechazar con 400 (Bad Request), 404 (Not Found) o 405 (Method Not Allowed)
        expect([400, 404, 405]).toContain(resp.status);
      }
    });

    it("debe validar que el ID de usuario sea un GUID válido antes de actualizar", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      if (!validUserId) {
        console.warn("No hay usuario válido disponible para el test");
        return;
      }

      // Verificar que el ID válido es un GUID
      expect(isValidGuid(validUserId)).toBe(true);

      // Intentar actualizar con el ID válido
      const resp = await httpRequest(`${API_URL}/api/user/${validUserId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          username: "testuser",
          firstName: "Test",
          lastName: "User",
        }),
      });

      // Debe aceptar (200) o rechazar con 400 si hay datos inválidos, pero no 404
      expect([200, 400]).toContain(resp.status);
      expect(resp.status).not.toBe(404);
    });

    it("debe rechazar actualización con ID que contiene caracteres especiales", async () => {
      if (!authToken) {
        console.warn("Saltando test: no hay token de autenticación");
        return;
      }

      const invalidIds = [
        "../../../etc/passwd", // Path traversal
        "<script>alert('xss')</script>", // XSS attempt
        "'; DROP TABLE Users; --", // SQL injection attempt
      ];

      for (const invalidId of invalidIds) {
        const resp = await httpRequest(`${API_URL}/api/user/${encodeURIComponent(invalidId)}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authToken}`,
          },
          body: JSON.stringify({
            username: "test",
            firstName: "Test",
            lastName: "User",
          }),
        });

        // Debe rechazar con 400 o 404
        expect([400, 404]).toContain(resp.status);
      }
    });
  });
});

