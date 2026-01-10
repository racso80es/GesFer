/**
 * Utilidades para inicialización y limpieza del cliente
 * Este módulo se encarga de validar y limpiar datos corruptos
 * que pueden causar problemas cuando se abre la aplicación en la ventana principal
 * vs ventana de incógnito (donde funciona bien porque no hay datos previos)
 */

import type { LoginResponse } from "@/lib/types/api";

/**
 * Valida si un token JWT está expirado
 */
function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return true;

    const payload = JSON.parse(atob(parts[1]));
    if (!payload.exp) return true;

    // Comparar con tiempo actual en segundos
    const currentTime = Math.floor(Date.now() / 1000);
    return payload.exp < currentTime;
  } catch {
    return true;
  }
}

/**
 * Valida si los datos del usuario tienen la estructura correcta
 */
function isValidUserData(data: any): data is LoginResponse {
  if (!data || typeof data !== "object") return false;

  // Campos requeridos
  const requiredFields = ["userId", "username", "firstName", "lastName", "companyId", "companyName"];
  for (const field of requiredFields) {
    if (!(field in data) || typeof data[field] !== "string" || data[field].trim() === "") {
      return false;
    }
  }

  // Validar que permissions sea un array
  if (!Array.isArray(data.permissions)) {
    return false;
  }

  return true;
}

/**
 * Limpia todos los datos de autenticación del almacenamiento
 */
export function clearAuthData(): void {
  if (typeof window === "undefined") return;

  try {
    localStorage.removeItem("auth_token");
    localStorage.removeItem("auth_user");
    
    // Eliminar cookies también
    document.cookie = "auth_token=; path=/; max-age=0; SameSite=Lax";
    document.cookie = "auth_user=; path=/; max-age=0; SameSite=Lax";
  } catch (error) {
    console.error("Error al limpiar datos de autenticación:", error);
  }
}

/**
 * Valida y limpia datos corruptos en localStorage
 * Retorna los datos validados o null si están corruptos o expirados
 */
export function validateAndCleanStoredUser(): LoginResponse | null {
  if (typeof window === "undefined") return null;

  try {
    // Obtener token
    const token = localStorage.getItem("auth_token");
    
    // Obtener usuario
    const userStr = localStorage.getItem("auth_user");
    if (!userStr) {
      // Si no hay usuario pero hay token, limpiar token también
      if (token) {
        console.warn("Token encontrado sin datos de usuario, limpiando...");
        clearAuthData();
      }
      return null;
    }

    // Parsear usuario
    let userData: any;
    try {
      userData = JSON.parse(userStr);
    } catch (parseError) {
      console.error("Error al parsear datos de usuario (JSON inválido), limpiando...", parseError);
      clearAuthData();
      return null;
    }

    // Validar estructura del usuario
    if (!isValidUserData(userData)) {
      console.error("Datos de usuario con estructura inválida, limpiando...", userData);
      clearAuthData();
      return null;
    }

    // Validar token si existe
    if (token) {
      if (isTokenExpired(token)) {
        console.warn("Token expirado encontrado, limpiando datos de autenticación...");
        clearAuthData();
        return null;
      }
      
      // Si hay usuario pero no tiene token en los datos, y hay token en localStorage,
      // asegurar que el token esté en los datos del usuario
      if (!userData.token) {
        userData.token = token;
      }
    } else if (userData.token) {
      // Si el usuario tiene token pero no está en localStorage, guardarlo
      try {
        localStorage.setItem("auth_token", userData.token);
      } catch (e) {
        console.warn("No se pudo guardar el token en localStorage:", e);
      }
    }

    return userData as LoginResponse;
  } catch (error) {
    console.error("Error inesperado al validar datos almacenados, limpiando...", error);
    clearAuthData();
    return null;
  }
}

/**
 * Limpia la caché de TanStack Query si es necesario
 * Esta función se llama cuando detectamos problemas de caché
 */
export function clearQueryCache(): void {
  if (typeof window === "undefined") return;

  try {
    // Buscar el QueryClient en el contexto de React Query
    // Nota: Esta función debe ser llamada dentro de un componente
    // que tenga acceso al QueryClient, por lo que la limpieza manual
    // se hace mejor desde el QueryProvider o desde un hook
    console.info("Se recomienda limpiar el caché de QueryClient desde el componente que lo usa");
  } catch (error) {
    console.error("Error al limpiar caché de QueryClient:", error);
  }
}

/**
 * Función principal de inicialización del cliente
 * Debe ser llamada al montar la aplicación
 */
export function initializeClient(): {
  user: LoginResponse | null;
  shouldClearCache: boolean;
} {
  if (typeof window === "undefined") {
    return { user: null, shouldClearCache: false };
  }

  console.log("Inicializando cliente...");
  
  // Validar y limpiar datos de autenticación
  const validUser = validateAndCleanStoredUser();
  
  // Determinar si se necesita limpiar caché
  // Si había datos corruptos, es probable que el caché también esté corrupto
  const shouldClearCache = !validUser && (
    localStorage.getItem("auth_token") !== null ||
    localStorage.getItem("auth_user") !== null
  );

  if (shouldClearCache) {
    console.warn("Se detectaron datos corruptos, se recomienda limpiar el caché de QueryClient");
  }

  if (validUser) {
    console.log("Cliente inicializado correctamente con usuario:", validUser.username);
  } else {
    console.log("Cliente inicializado sin sesión de usuario");
  }

  return {
    user: validUser,
    shouldClearCache,
  };
}
