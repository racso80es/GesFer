import type { ApiError, ApiResponse } from "@/lib/types/api";

// La API puede estar en HTTP (5000) o HTTPS (5001)
// Por defecto usamos HTTPS en 5001, pero puedes cambiarlo en .env.local
import { API_URL } from "@/lib/config";

/**
 * Opciones para el cliente API
 */
export interface ApiClientOptions {
  /**
   * Token de acceso JWT para autenticación
   * Si no se proporciona, intentará obtenerlo de la sesión de Auth.js
   */
  accessToken?: string;
}

class ApiClient {
  private baseUrl: string;
  private accessToken?: string;

  constructor(baseUrl: string = API_URL, options?: ApiClientOptions) {
    this.baseUrl = baseUrl;
    this.accessToken = options?.accessToken;
  }

  /**
   * Establece el token de acceso para las siguientes peticiones
   */
  setAccessToken(token: string | undefined) {
    this.accessToken = token;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    
    const config: RequestInit = {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
    };

    // Agregar token de autenticación si existe
    // Prioridad: token pasado como parámetro > token de la instancia > localStorage (legacy)
    let token = this.accessToken;
    
    if (!token && typeof window !== "undefined") {
      // Intentar obtener el token de la sesión de NextAuth desde localStorage
      // NextAuth almacena la sesión en una cookie, pero para compatibilidad
      // también intentamos localStorage (legacy)
      token = localStorage.getItem("auth_token") || undefined;
    }

    if (token) {
      config.headers = {
        ...config.headers,
        Authorization: `Bearer ${token}`,
      };
    }

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const errorData: ApiError = await response.json().catch(() => ({
          message: `Error ${response.status}: ${response.statusText}`,
        }));
        throw new Error(errorData.message || "Error en la petición");
      }

      // Si la respuesta es 204 No Content, retornar void
      if (response.status === 204) {
        return undefined as T;
      }

      return await response.json();
    } catch (error) {
      if (error instanceof Error) {
        // Mejorar mensajes de error para problemas de conexión
        if (error.message.includes("Failed to fetch") || error.message.includes("ERR_EMPTY_RESPONSE")) {
          throw new Error(
            `No se pudo conectar con la API en ${url}. ` +
            `Verifica que la API esté ejecutándose y que la URL sea correcta. ` +
            `URL configurada: ${this.baseUrl}`
          );
        }
        throw error;
      }
      throw new Error("Error desconocido en la petición");
    }
  }

  async get<T>(endpoint: string, params?: Record<string, string>): Promise<T> {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        url.searchParams.append(key, value);
      });
    }
    return this.request<T>(url.pathname + url.search);
  }

  async post<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: "POST",
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async put<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: "PUT",
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, {
      method: "DELETE",
    });
  }
}

export const apiClient = new ApiClient();

