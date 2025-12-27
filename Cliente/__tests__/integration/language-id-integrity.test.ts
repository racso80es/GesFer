/**
 * Tests de integridad para el manejo de languageId en formularios
 * 
 * Estos tests verifican que:
 * 1. Cuando se selecciona un idioma específico, se envía el Guid correcto
 * 2. Cuando se selecciona "Por defecto", se envía undefined/null
 * 3. El backend acepta y persiste correctamente ambos casos
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
  return loginData.token || "";
};

describe("Integridad: Manejo de languageId en formularios", () => {
  let authToken: string;
  let testCompanyId: string;
  let testUserId: string;
  let userTestCompanyId: string;

  beforeAll(async () => {
    // Obtener token de autenticación
    try {
      authToken = await getAuthToken();
    } catch (error) {
      throw new Error(`No se pudo obtener token de autenticación: ${error}`);
    }

    // Obtener o crear una empresa para los tests
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
        userTestCompanyId = companies[0].id;
      }
    }
  });

  describe("Empresas - Manejo de languageId", () => {
    it("debe crear una empresa con languageId específico (Guid)", async () => {
      const companyData = {
        name: `Test Company Language ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Test 123",
        phone: "123456789",
        email: `testlang${Date.now()}@example.com`,
        languageId: LANGUAGE_IDS.es, // Guid específico
      };

      const resp = await httpRequest(`${API_URL}/api/company`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(companyData),
      });

      expect(resp.status).toBe(201);
      const company = JSON.parse(resp.body);
      expect(company.languageId).toBe(LANGUAGE_IDS.es);
      testCompanyId = company.id;
    });

    it("debe actualizar una empresa estableciendo languageId a null (por defecto)", async () => {
      expect(testCompanyId).toBeTruthy();

      // Obtener el valor actual de languageId antes de actualizar
      const getResp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(getResp.status).toBe(200);
      const currentCompany = JSON.parse(getResp.body);
      const previousLanguageId = currentCompany.languageId;

      const updateData = {
        name: `Test Company Language ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Test 123",
        phone: "123456789",
        email: `testlang${Date.now()}@example.com`,
        languageId: null, // null = mantener el valor actual (por defecto)
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
      // El backend mantiene el valor actual cuando se envía null
      expect(company.languageId).toBe(previousLanguageId);
    });

    it("debe actualizar una empresa sin enviar languageId (undefined)", async () => {
      expect(testCompanyId).toBeTruthy();

      // Obtener la empresa actual primero
      const getResp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(getResp.status).toBe(200);
      const currentCompany = JSON.parse(getResp.body);

      // Actualizar sin incluir languageId (debe mantener el valor actual o usar por defecto)
      const updateData = {
        name: currentCompany.name,
        taxId: currentCompany.taxId,
        address: currentCompany.address,
        phone: currentCompany.phone,
        email: currentCompany.email,
        isActive: currentCompany.isActive,
        // No incluir languageId - debe ser opcional
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
      expect(company).toHaveProperty("languageId");
    });

    it("debe rechazar languageId inválido (código de idioma en lugar de Guid)", async () => {
      expect(testCompanyId).toBeTruthy();

      const updateData = {
        name: `Test Company Language ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Test 123",
        phone: "123456789",
        email: `testlang${Date.now()}@example.com`,
        languageId: "es", // Código inválido (debe ser Guid)
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

      // Debe rechazar el código de idioma y devolver 400
      expect([400, 404]).toContain(resp.status);
    });

    it("debe cambiar languageId de un idioma a otro", async () => {
      expect(testCompanyId).toBeTruthy();

      // Primero establecer a español
      const updateDataEs = {
        name: `Test Company Language ${Date.now()}`,
        taxId: `B${Math.floor(Math.random() * 100000000)}`,
        address: "Calle Test 123",
        phone: "123456789",
        email: `testlang${Date.now()}@example.com`,
        languageId: LANGUAGE_IDS.es,
        isActive: true,
      };

      let resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateDataEs),
      });

      expect(resp.status).toBe(200);
      let company = JSON.parse(resp.body);
      expect(company.languageId).toBe(LANGUAGE_IDS.es);

      // Luego cambiar a inglés
      const updateDataEn = {
        ...updateDataEs,
        languageId: LANGUAGE_IDS.en,
      };

      resp = await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateDataEn),
      });

      expect(resp.status).toBe(200);
      company = JSON.parse(resp.body);
      expect(company.languageId).toBe(LANGUAGE_IDS.en);
    });
  });

  describe("Usuarios - Manejo de languageId", () => {
    it("debe crear un usuario con languageId específico (Guid)", async () => {
      expect(userTestCompanyId).toBeTruthy();

      const userData = {
        companyId: userTestCompanyId,
        username: `testuserlang${Date.now()}`,
        password: "TestPassword123!",
        firstName: "Test",
        lastName: "User",
        email: `testuserlang${Date.now()}@example.com`,
        phone: "123456789",
        languageId: LANGUAGE_IDS.ca, // Guid específico
      };

      const resp = await httpRequest(`${API_URL}/api/user`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(userData),
      });

      expect(resp.status).toBe(201);
      const user = JSON.parse(resp.body);
      expect(user.languageId).toBe(LANGUAGE_IDS.ca);
      testUserId = user.id;
    });

    it("debe actualizar un usuario estableciendo languageId a null (por defecto)", async () => {
      expect(testUserId).toBeTruthy();

      // Obtener el valor actual de languageId antes de actualizar
      const getResp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(getResp.status).toBe(200);
      const currentUser = JSON.parse(getResp.body);
      const previousLanguageId = currentUser.languageId;

      const updateData = {
        username: `testuserlang${Date.now()}`,
        firstName: "Test",
        lastName: "User",
        email: `testuserlang${Date.now()}@example.com`,
        phone: "123456789",
        languageId: null, // null = mantener el valor actual (por defecto)
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
      // El backend mantiene el valor actual cuando se envía null
      expect(user.languageId).toBe(previousLanguageId);
    });

    it("debe actualizar un usuario sin enviar languageId (undefined)", async () => {
      expect(testUserId).toBeTruthy();

      // Obtener el usuario actual primero
      const getResp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      expect(getResp.status).toBe(200);
      const currentUser = JSON.parse(getResp.body);

      // Actualizar sin incluir languageId (debe mantener el valor actual o usar por defecto)
      const updateData = {
        username: currentUser.username,
        firstName: currentUser.firstName,
        lastName: currentUser.lastName,
        email: currentUser.email,
        phone: currentUser.phone,
        isActive: currentUser.isActive,
        // No incluir languageId - debe ser opcional
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
      expect(user).toHaveProperty("languageId");
    });

    it("debe rechazar languageId inválido (código de idioma en lugar de Guid)", async () => {
      expect(testUserId).toBeTruthy();

      const updateData = {
        username: `testuserlang${Date.now()}`,
        firstName: "Test",
        lastName: "User",
        email: `testuserlang${Date.now()}@example.com`,
        phone: "123456789",
        languageId: "en", // Código inválido (debe ser Guid)
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

      // Debe rechazar el código de idioma y devolver 400
      expect([400, 404]).toContain(resp.status);
    });

    it("debe cambiar languageId de un idioma a otro", async () => {
      expect(testUserId).toBeTruthy();

      // Primero establecer a catalán
      const updateDataCa = {
        username: `testuserlang${Date.now()}`,
        firstName: "Test",
        lastName: "User",
        email: `testuserlang${Date.now()}@example.com`,
        phone: "123456789",
        languageId: LANGUAGE_IDS.ca,
        isActive: true,
      };

      let resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateDataCa),
      });

      expect(resp.status).toBe(200);
      let user = JSON.parse(resp.body);
      expect(user.languageId).toBe(LANGUAGE_IDS.ca);

      // Luego cambiar a inglés
      const updateDataEn = {
        ...updateDataCa,
        languageId: LANGUAGE_IDS.en,
      };

      resp = await httpRequest(`${API_URL}/api/user/${testUserId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify(updateDataEn),
      });

      expect(resp.status).toBe(200);
      user = JSON.parse(resp.body);
      expect(user.languageId).toBe(LANGUAGE_IDS.en);
    });
  });

  afterAll(async () => {
    // Limpiar recursos de prueba
    if (testCompanyId) {
      try {
        await httpRequest(`${API_URL}/api/company/${testCompanyId}`, {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${authToken}`,
          },
        });
      } catch (error) {
        // Ignorar errores de limpieza
      }
    }

    if (testUserId) {
      try {
        await httpRequest(`${API_URL}/api/user/${testUserId}`, {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${authToken}`,
          },
        });
      } catch (error) {
        // Ignorar errores de limpieza
      }
    }
  });
});


