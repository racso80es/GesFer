"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import type { LoginResponse } from "@/lib/types/api";
import { authApi } from "@/lib/api/auth";
import { signIn as nextAuthSignIn } from "next-auth/react";

interface AuthContextType {
  user: LoginResponse | null;
  isLoading: boolean;
  login: (credentials: {
    empresa: string;
    usuario: string;
    contraseña: string;
  }) => Promise<void>;
  logout: () => void;
  updateUser: (updatedUser: Partial<LoginResponse>) => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<LoginResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    
    // Cargar y validar usuario usando la función de inicialización del cliente
    const loadUser = async () => {
      try {
        // La función getStoredUser ahora usa validateAndCleanStoredUser internamente
        // que valida y limpia datos corruptos automáticamente
        const storedUser = authApi.getStoredUser();
        if (storedUser && isMounted) {
          setUser(storedUser);
        }
      } catch (error) {
        console.error("Error al cargar usuario desde localStorage:", error);
        // La función de validación ya limpia los datos corruptos automáticamente,
        // pero por si acaso, asegurar que el estado esté limpio
        if (isMounted) {
          setUser(null);
        }
      } finally {
        // Siempre establecer isLoading en false, incluso si hay error
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    // Timeout de seguridad: asegurar que isLoading se establezca en false después de máximo 2 segundos
    const timeoutId = setTimeout(() => {
      if (isMounted) {
        console.warn("AuthContext: Timeout de seguridad activado, forzando isLoading a false");
        setIsLoading(false);
      }
    }, 2000);

    // Cargar usuario inmediatamente
    loadUser();

    return () => {
      isMounted = false;
      clearTimeout(timeoutId);
    };
  }, []);

  const login = async (credentials: {
    empresa: string;
    usuario: string;
    contraseña: string;
  }) => {
    try {
      // Primero hacer login con la API para obtener los datos
      const response = await authApi.login(credentials);
      
      // Actualizar el estado del usuario primero
      setUser(response);
      
      // Luego intentar actualizar la sesión de NextAuth para que el middleware la reconozca
      // Usar signIn de next-auth/react (versión cliente)
      // Hacer esto de forma no bloqueante para no afectar el flujo principal
      try {
        const result = await nextAuthSignIn("credentials", {
          empresa: credentials.empresa,
          usuario: credentials.usuario,
          contraseña: credentials.contraseña,
          redirect: false, // No redirigir automáticamente, lo haremos manualmente
        });
        
        // Si NextAuth falla pero la API funcionó, continuar de todas formas
        // (el usuario ya está guardado en localStorage)
        if (result?.error) {
          console.warn("NextAuth signIn falló pero el login de API fue exitoso:", result.error);
        }
      } catch (nextAuthError) {
        // Si NextAuth falla completamente, no afectar el flujo principal
        // El usuario ya está autenticado en AuthContext y localStorage
        console.warn("Error al actualizar sesión NextAuth (no crítico):", nextAuthError);
      }
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    authApi.logout();
    setUser(null);
  };

  const updateUser = (updatedUser: Partial<LoginResponse>) => {
    if (user) {
      const newUser = { ...user, ...updatedUser };
      setUser(newUser);
      // Actualizar también en localStorage y cookies
      if (typeof window !== "undefined") {
        localStorage.setItem("auth_user", JSON.stringify(newUser));
        document.cookie = `auth_user=${encodeURIComponent(JSON.stringify(newUser))}; path=/; max-age=86400`;
      }
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        login,
        logout,
        updateUser,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth debe ser usado dentro de un AuthProvider");
  }
  return context;
}

