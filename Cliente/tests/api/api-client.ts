import { APIRequestContext, APIResponse } from '@playwright/test';

/**
 * Cliente API para tests de Playwright
 * Configurado para apuntar a 127.0.0.1:5000 (usar IPv4 explícitamente para evitar problemas en Windows)
 * 
 * URL de la API: http://127.0.0.1:5000
 */
export class ApiClient {
  private request: APIRequestContext;
  private baseURL: string;

  constructor(request: APIRequestContext, baseURL: string = 'http://127.0.0.1:5000') {
    this.request = request;
    this.baseURL = baseURL;
  }

  /**
   * Realiza una petición GET
   */
  async get(endpoint: string, headers?: Record<string, string>): Promise<APIResponse> {
    return await this.request.get(`${this.baseURL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
    });
  }

  /**
   * Realiza una petición POST
   */
  async post(endpoint: string, data: any, headers?: Record<string, string>): Promise<APIResponse> {
    return await this.request.post(`${this.baseURL}${endpoint}`, {
      data,
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
    });
  }

  /**
   * Realiza una petición PUT
   */
  async put(endpoint: string, data: any, headers?: Record<string, string>): Promise<APIResponse> {
    return await this.request.put(`${this.baseURL}${endpoint}`, {
      data,
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
    });
  }

  /**
   * Realiza una petición DELETE
   */
  async delete(endpoint: string, headers?: Record<string, string>): Promise<APIResponse> {
    return await this.request.delete(`${this.baseURL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
    });
  }

  /**
   * Realiza login y obtiene la información del usuario
   * Nota: La API no devuelve un token, devuelve un LoginResponseDto
   * Para mantener compatibilidad con tests, devolvemos el userId como "token"
   */
  async login(empresa: string, usuario: string, contraseña: string): Promise<string> {
    const response = await this.post('/api/auth/login', {
      empresa,
      usuario,
      contraseña,
    });

    if (!response.ok()) {
      throw new Error(`Login failed: ${response.status()}`);
    }

    const data = await response.json();
    // La API no devuelve token, devolvemos el userId como identificador de sesión
    // Esto es solo para compatibilidad con los tests
    return data.userId || '';
  }

  /**
   * Obtiene la información completa del login
   */
  async loginFull(empresa: string, usuario: string, contraseña: string): Promise<any> {
    const response = await this.post('/api/auth/login', {
      empresa,
      usuario,
      contraseña,
    });

    if (!response.ok()) {
      throw new Error(`Login failed: ${response.status()}`);
    }

    return await response.json();
  }

  /**
   * Obtiene headers con autenticación
   */
  getAuthHeaders(token: string): Record<string, string> {
    return {
      Authorization: `Bearer ${token}`,
    };
  }

  /**
   * Elimina un usuario por ID (para limpieza de tests)
   */
  async deleteUser(userId: string, token: string): Promise<APIResponse> {
    return await this.delete(`/api/user/${userId}`, this.getAuthHeaders(token));
  }

  /**
   * Elimina una empresa por ID (para limpieza de tests)
   */
  async deleteCompany(companyId: string, token: string): Promise<APIResponse> {
    return await this.delete(`/api/company/${companyId}`, this.getAuthHeaders(token));
  }

  /**
   * Elimina un cliente por ID (para limpieza de tests)
   */
  async deleteCustomer(customerId: string, token: string): Promise<APIResponse> {
    return await this.delete(`/api/customer/${customerId}`, this.getAuthHeaders(token));
  }
}

