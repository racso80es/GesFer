"use client";

import { QueryClient, QueryClientProvider, useQueryClient, QueryCache } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState, useEffect, useRef, type ReactNode } from "react";
import { initializeClient, clearAuthData } from "@/lib/utils/client-init";
import { authApi } from "@/lib/api/auth";
import type { LoginResponse } from "@/lib/types/api";

// Variable de módulo para almacenar el resultado de inicialización (solo se ejecuta una vez)
let initResult: { user: LoginResponse | null; shouldClearCache: boolean } | null = null;

/**
 * Componente interno que limpia el caché si es necesario después de que el QueryClient esté disponible
 */
function CacheCleaner() {
  const queryClient = useQueryClient();
  const hasCleanedRef = useRef(false);

  useEffect(() => {
    // Solo limpiar una vez al montar
    if (hasCleanedRef.current) return;

    // Si la inicialización detectó que se necesita limpiar caché, hacerlo ahora
    if (initResult?.shouldClearCache) {
      queryClient.clear();
      hasCleanedRef.current = true;
      return;
    }

    // Verificación adicional: si hay datos pero no son válidos, limpiar el caché
    if (typeof window !== "undefined") {
      const token = localStorage.getItem("auth_token");
      const user = localStorage.getItem("auth_user");
      
      // Si hay datos pero no son válidos (fueron limpiados por initializeClient),
      // limpiar el caché también
      if ((token || user) && !authApi.getStoredUser()) {
        queryClient.clear();
        hasCleanedRef.current = true;
      }
    }
  }, [queryClient]);

  return null;
}

export function QueryProvider({ children }: { children: ReactNode }) {
  const [queryClient] = useState(() => {
    // Inicializar cliente ANTES de crear el QueryClient (solo una vez, usando variable de módulo)
    if (typeof window !== "undefined" && initResult === null) {
      initResult = initializeClient();
    }

    // Crear QueryCache con manejo global de errores
    const queryCache = new QueryCache({
      onError: (error: unknown, query) => {
        const err = error as { message?: string; status?: number };
        // Si es un error 401, limpiar caché y datos de autenticación
        if (err?.message?.includes("401") || err?.status === 401) {
          console.warn("Error 401 detectado, limpiando datos de autenticación...");
          if (typeof window !== "undefined") {
            clearAuthData();
            // El client se creará después, así que usamos el queryCache para limpiar
            queryCache.clear();
          }
        }
      },
    });

    const client = new QueryClient({
      queryCache,
      defaultOptions: {
        queries: {
          staleTime: 60 * 1000, // 1 minuto
          refetchOnWindowFocus: false,
          retry: 1,
        },
      },
    });

    // Si se detectaron datos corruptos durante la inicialización, limpiar el caché inmediatamente
    if (initResult?.shouldClearCache) {
      client.clear();
    }

    return client;
  });

  return (
    <QueryClientProvider client={queryClient}>
      <CacheCleaner />
      {children}
      {process.env.NODE_ENV === "development" && <ReactQueryDevtools />}
    </QueryClientProvider>
  );
}

