import { apiClient } from "./client";
import type { LoginRequest, LoginResponse } from "@/lib/types/api";

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(
      "/api/auth/login",
      credentials
    );
    
    // Guardar token en localStorage y cookies
    if (typeof window !== "undefined" && response.token) {
      localStorage.setItem("auth_token", response.token);
      localStorage.setItem("auth_user", JSON.stringify(response));
      
      // Guardar también en cookies para que el middleware pueda acceder
      document.cookie = `auth_token=${response.token}; path=/; max-age=86400`; // 24 horas
      document.cookie = `auth_user=${encodeURIComponent(JSON.stringify(response))}; path=/; max-age=86400`;
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

