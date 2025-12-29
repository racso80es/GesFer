"use client";

import { useSession } from "@/lib/hooks/use-session";
import { apiClient } from "./client";

/**
 * Hook para obtener un cliente API configurado con el token de la sesión actual
 * Úsalo en Client Components
 * 
 * @example
 * ```tsx
 * function MyComponent() {
 *   const api = useApiClient();
 *   
 *   useEffect(() => {
 *     api.get('/api/users').then(data => console.log(data));
 *   }, []);
 * }
 * ```
 */
export function useApiClient() {
  const { accessToken } = useSession();
  
  // Configurar el token en el cliente
  if (accessToken) {
    apiClient.setAccessToken(accessToken);
  }
  
  return apiClient;
}

