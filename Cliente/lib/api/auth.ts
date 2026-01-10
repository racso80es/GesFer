import { apiClient } from "./client";
import type { LoginRequest, LoginResponse } from "@/lib/types/api";
import { validateAndCleanStoredUser, clearAuthData } from "@/lib/utils/client-init";

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(
      "/api/auth/login",
      credentials
    );
    
    // Guardar usuario en localStorage y cookies (siempre, incluso sin token)
    if (typeof window !== "undefined") {
      try {
        // Guardar usuario siempre
        localStorage.setItem("auth_user", JSON.stringify(response));
        document.cookie = `auth_user=${encodeURIComponent(JSON.stringify(response))}; path=/; max-age=86400; SameSite=Lax`;
        
        // Guardar token solo si existe
        if (response.token) {
          localStorage.setItem("auth_token", response.token);
          document.cookie = `auth_token=${response.token}; path=/; max-age=86400; SameSite=Lax`; // 24 horas
        }
      } catch (error) {
        console.error("Error al guardar datos de autenticación:", error);
        // Si hay error, limpiar datos previos antes de continuar
        clearAuthData();
        throw new Error("No se pudo guardar la sesión. Intenta nuevamente.");
      }
    }
    
    return response;
  },

  logout: () => {
    clearAuthData();
  },

  getStoredUser: (): LoginResponse | null => {
    // Usar la función de validación que limpia datos corruptos automáticamente
    return validateAndCleanStoredUser();
  },

  getPermissions: async (userId: string): Promise<string[]> => {
    return apiClient.get<string[]>(`/api/auth/permissions/${userId}`);
  },
};

