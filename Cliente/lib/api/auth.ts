import { apiClient } from "./client";
import type { LoginRequest, LoginResponse } from "@/lib/types/api";

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(
      "/api/auth/login",
      credentials
    );
    
    // Guardar usuario en localStorage y cookies (siempre, incluso sin token)
    if (typeof window !== "undefined") {
      // Guardar usuario siempre
      localStorage.setItem("auth_user", JSON.stringify(response));
      document.cookie = `auth_user=${encodeURIComponent(JSON.stringify(response))}; path=/; max-age=86400`;
      
      // Guardar token solo si existe
      if (response.token) {
        localStorage.setItem("auth_token", response.token);
        document.cookie = `auth_token=${response.token}; path=/; max-age=86400`; // 24 horas
      }
    }
    
    return response;
  },

  logout: () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("auth_token");
      localStorage.removeItem("auth_user");
      
      // Eliminar cookies también
      document.cookie = "auth_token=; path=/; max-age=0";
      document.cookie = "auth_user=; path=/; max-age=0";
    }
  },

  getStoredUser: (): LoginResponse | null => {
    if (typeof window === "undefined") return null;
    const userStr = localStorage.getItem("auth_user");
    return userStr ? JSON.parse(userStr) : null;
  },

  getPermissions: async (userId: string): Promise<string[]> => {
    return apiClient.get<string[]>(`/api/auth/permissions/${userId}`);
  },
};

