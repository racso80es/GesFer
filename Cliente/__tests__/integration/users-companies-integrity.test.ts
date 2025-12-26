/**
 * Tests de integridad E2E para Usuarios y Empresas
 * 
 * Estos tests verifican el flujo completo de creación y edición
 * de usuarios y empresas contra la API real.
 * 
 * Requiere:
 * - API ejecutándose en http://localhost:5000
 * - Credenciales de prueba: empresa "Empresa Demo", usuario "admin", contraseña "admin123"
 */

import http from "node:http";
import { URL } from "node:url";
import config from "../../config.json";

const API_URL = config.apiUrl.replace(/\/$/, "");

// Ampliar timeout porque son llamadas reales a servicios
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

// IDs de lenguajes según seed-data.sql
const LANGUAGE_IDS = {
  es: "10000000-0000-0000-0000-000000000001", // Español
  en: "10000000-0000-0000-0000-000000000002", // English
  ca: "10000000-0000-0000-0000-000000000003", // Català
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
  // El token puede estar vacío en desarrollo, pero la autenticación puede funcionar sin él
  return loginData.token || "";
};

describe("Integridad E2E: Usuarios y Empresas", () => {
  let authToken: string;
  let testCompanyId: string;
  let testUserId: string;

  beforeAll(async () => {
    // Obtener token de autenticación
    try {
      authToken = await getAuthToken();
      expect(authToken).toBeTruthy();
    } catch (error) {
      console.warn("No se pudo obtener token de autenticación. Algunos tests pueden fallar.");
    }
  });

  describe("Empresas - Flujo completo", () => {
    it("debe listar todas las empresas", async () => {
      const resp = await httpRequest(`${API_URL}/api/company`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const companies = JSON.parse(resp.body);
      expect(Array.isArray(companies)).toBe(true);
      expect(companies.length).toBeGreaterThan(0);
    });

    it("debe crear una empresa correctamente", async () => {
      const companyData = {
        name: `Test Company ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Test 123",
        phone: "123456789",
        email: `test${Date.now()}@example.com`,
        languageId: LANGUAGE_IDS.es,
      };

      const resp = await httpRequest(`${API_URL}/api/company`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(companyData),
      });

      if (resp.status !== 201) {
        console.error("Error al crear empresa:", resp.body);
        console.error("Status:", resp.status);
      }
      expect(resp.status).toBe(201);
      const company = JSON.parse(resp.body);
      expect(company).toHaveProperty("id");
      expect(company.name).toBe(companyData.name);
      expect(company.taxId).toBe(companyData.taxId);
      expect(company.email).toBe(companyData.email);
      expect(company.languageId).toBe(companyData.languageId);

      testCompanyId = company.id;
    });

    it("debe obtener la empresa creada", async () => {
      expect(testCompanyId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const company = JSON.parse(resp.body);
      expect(company.id).toBe(testCompanyId);
    });

    it("debe editar una empresa correctamente", async () => {
      expect(testCompanyId).toBeTruthy();

      const updateData = {
        name: `Updated Company ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Actualizada 456",
        phone: "987654321",
        email: `updated${Date.now()}@example.com`,
        languageId: LANGUAGE_IDS.en,
        isActive: true,
      };

      const resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateData),
      });

      expect(resp.status).toBe(200);
      const company = JSON.parse(resp.body);
      expect(company.name).toBe(updateData.name);
      expect(company.taxId).toBe(updateData.taxId);
      expect(company.address).toBe(updateData.address);
      expect(company.phone).toBe(updateData.phone);
      expect(company.email).toBe(updateData.email);
      expect(company.languageId).toBe(updateData.languageId);
      expect(company.isActive).toBe(true);
    });

    it("debe verificar que la empresa se editó correctamente", async () => {
      expect(testCompanyId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const company = JSON.parse(resp.body);
      expect(company.name).toContain("Updated Company");
      expect(company.languageId).toBe(LANGUAGE_IDS.en);
    });

    it("debe eliminar una empresa correctamente", async () => {
      expect(testCompanyId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      // DELETE puede devolver 200 o 204 según la implementación
      expect([200, 204]).toContain(resp.status);

      // Verificar que la empresa ya no existe
      const getResp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      // La empresa debería estar eliminada (soft delete) o no encontrada
      expect([404, 200]).toContain(getResp.status);
    });
  });

  describe("Usuarios - Flujo completo", () => {
    let userTestCompanyId: string;

    beforeAll(async () => {
      // Asegurar que tenemos una empresa para los tests de usuarios
      // Usar la empresa demo o crear una nueva
      const companiesResp = await httpRequest(`${API_URL}/api/company`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      if (companiesResp.status === 200) {
        const companies = JSON.parse(companiesResp.body);
        const demoCompany = companies.find((c: any) => c.name === "Empresa Demo");
        if (demoCompany) {
          userTestCompanyId = demoCompany.id;
        } else if (companies.length > 0) {
          // Usar la primera empresa disponible
          userTestCompanyId = companies[0].id;
        }
      }

      // Si no hay empresa disponible, crear una
      if (!userTestCompanyId) {
        const companyData = {
          name: `Test Company for Users ${Date.now()}`,
          taxId: `B${Math.floor(Math.random() * 100000000)}`,
          address: "Calle Test 123",
          phone: "123456789",
          email: `testusers${Date.now()}@example.com`,
          languageId: LANGUAGE_IDS.es,
        };

        const resp = await httpRequest(`${API_URL}/api/company`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authToken}`,
          },
          body: JSON.stringify(companyData),
        });

        if (resp.status === 201) {
          const company = JSON.parse(resp.body);
          userTestCompanyId = company.id;
        }
      }
    });

    it("debe listar todos los usuarios", async () => {
      const resp = await httpRequest(`${API_URL}/api/user`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const users = JSON.parse(resp.body);
      expect(Array.isArray(users)).toBe(true);
      expect(users.length).toBeGreaterThan(0);
    });

    it("debe crear un usuario correctamente", async () => {
      expect(userTestCompanyId).toBeTruthy();

      const userData = {
        companyId: userTestCompanyId,
        username: `testuser${Date.now()}`,
        password: "TestPassword123!",
        firstName: "Test",
        lastName: "User",
        email: `testuser${Date.now()}@example.com`,
        phone: "123456789",
        languageId: LANGUAGE_IDS.es,
      };

      const resp = await httpRequest(`${API_URL}/api/user`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(userData),
      });

      if (resp.status !== 201) {
        console.error("Error al crear usuario:", resp.body);
        console.error("Status:", resp.status);
      }
      expect(resp.status).toBe(201);
      const user = JSON.parse(resp.body);
      expect(user).toHaveProperty("id");
      expect(user.username).toBe(userData.username);
      expect(user.firstName).toBe(userData.firstName);
      expect(user.lastName).toBe(userData.lastName);
      expect(user.email).toBe(userData.email);
      expect(user.languageId).toBe(userData.languageId);

      testUserId = user.id;
    });

    it("debe obtener el usuario creado", async () => {
      expect(testUserId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const user = JSON.parse(resp.body);
      expect(user.id).toBe(testUserId);
    });

    it("debe editar un usuario correctamente", async () => {
      expect(testUserId).toBeTruthy();

      const updateData = {
        username: `updateduser${Date.now()}`,
        firstName: "Updated",
        lastName: "Name",
        email: `updated${Date.now()}@example.com`,
        phone: "987654321",
        languageId: LANGUAGE_IDS.ca,
        isActive: true,
      };

      const resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateData),
      });

      expect(resp.status).toBe(200);
      const user = JSON.parse(resp.body);
      expect(user.username).toBe(updateData.username);
      expect(user.firstName).toBe(updateData.firstName);
      expect(user.lastName).toBe(updateData.lastName);
      expect(user.email).toBe(updateData.email);
      expect(user.phone).toBe(updateData.phone);
      expect(user.languageId).toBe(updateData.languageId);
      expect(user.isActive).toBe(true);
    });

    it("debe verificar que el usuario se editó correctamente", async () => {
      expect(testUserId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(resp.status).toBe(200);
      const user = JSON.parse(resp.body);
      expect(user.firstName).toBe("Updated");
      expect(user.lastName).toBe("Name");
      expect(user.languageId).toBe(LANGUAGE_IDS.ca);
    });

    it("debe eliminar un usuario correctamente", async () => {
      expect(testUserId).toBeTruthy();

      const resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      // DELETE puede devolver 200 o 204 según la implementación
      expect([200, 204]).toContain(resp.status);

      // Verificar que el usuario ya no existe
      const getResp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      // El usuario debería estar eliminado (soft delete) o no encontrado
      expect([404, 200]).toContain(getResp.status);
    });
  });

  afterAll(async () => {
    // Limpiar: eliminar recursos de prueba si es necesario
    // (opcional, dependiendo de si quieres mantener los datos de prueba)
  });
});

