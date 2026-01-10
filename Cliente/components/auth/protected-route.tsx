"use client";

import React, { useEffect, useState, useRef } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuth } from "@/contexts/auth-context";
import { Loading } from "@/components/ui/loading";
import { useTranslations } from 'next-intl';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, isLoading } = useAuth();
  const t = useTranslations('common');
  const [hasCheckedAuth, setHasCheckedAuth] = useState(false);
  const hasRedirectedRef = useRef(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    // Si ya verificamos, no hacer nada más
    if (hasCheckedAuth) {
      return;
    }

    // Timeout de seguridad: si isLoading nunca se resuelve, forzar la verificación después de 2 segundos
    timeoutRef.current = setTimeout(() => {
      if (!hasCheckedAuth) {
        console.warn("ProtectedRoute: Timeout de seguridad activado, forzando verificación de autenticación");
        // Verificar localStorage directamente si isLoading tarda mucho
        const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
        if (storedUser) {
          setHasCheckedAuth(true);
        } else {
          // Si no hay usuario, marcar como verificado para permitir la redirección
          setHasCheckedAuth(true);
        }
      }
    }, 2000);

    // Esperar a que termine de cargar antes de verificar autenticación
    if (!isLoading) {
      // Limpiar timeout si isLoading se resuelve correctamente
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }

      // Verificar tanto el estado de React como localStorage
      const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
      const actuallyAuthenticated = isAuthenticated || !!storedUser;
      
      setHasCheckedAuth(true);
      
      // Solo redirigir si definitivamente no está autenticado y no estamos en login
      if (!actuallyAuthenticated && !hasRedirectedRef.current) {
        const currentPath = typeof window !== 'undefined' ? window.location.pathname : pathname;
        if (currentPath && !currentPath.includes('login')) {
          hasRedirectedRef.current = true;
          router.replace("/login");
        }
      }
    } else {
      // Si isLoading es true, intentar verificar inmediatamente desde localStorage
      // Esto evita bloqueos innecesarios si hay datos en localStorage
      const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
      if (storedUser && !hasCheckedAuth) {
        // Si hay usuario en localStorage, marcar como verificado inmediatamente
        if (timeoutRef.current) {
          clearTimeout(timeoutRef.current);
          timeoutRef.current = null;
        }
        setHasCheckedAuth(true);
      }
    }

    // Cleanup: limpiar timeout al desmontar
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }
    };
  }, [isAuthenticated, isLoading, router, pathname, hasCheckedAuth]);

  // Verificar si estamos en una ruta de login (no requiere autenticación)
  const isLoginPage = pathname?.includes('/login') || pathname === '/login';

  // Si estamos en login, permitir acceso sin verificar autenticación
  if (isLoginPage) {
    return <>{children}</>;
  }

  // Mientras carga y no hemos verificado, mostrar loading
  // El timeout de seguridad asegura que no se quede bloqueado indefinidamente
  // IMPORTANTE: Después de 3 segundos máximo, permitir interacción incluso si isLoading es true
  const [forceAllowInteraction, setForceAllowInteraction] = useState(false);
  
  useEffect(() => {
    if (isLoading && !hasCheckedAuth) {
      const forceTimeout = setTimeout(() => {
        console.warn("ProtectedRoute: Timeout forzando interacción después de 3 segundos");
        setForceAllowInteraction(true);
      }, 3000);
      
      return () => clearTimeout(forceTimeout);
    } else {
      setForceAllowInteraction(false);
    }
  }, [isLoading, hasCheckedAuth]);
  
  if (isLoading && !hasCheckedAuth && !forceAllowInteraction) {
    // Si hay usuario en localStorage pero isLoading todavía es true, permitir acceso
    const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
    if (storedUser) {
      // Si hay usuario almacenado, permitir acceso sin esperar
      // El useEffect manejará la verificación completa
      return <>{children}</>;
    }
    
    // IMPORTANTE: El contenedor NO debe bloquear la interacción indefinidamente
    // El componente Loading ya tiene pointer-events: none en su contenedor interno
    // Usar relative en lugar de fixed para que no bloquee toda la pantalla
    // El contenedor NO debe tener position fixed que bloquee la interacción
    // CRÍTICO: No usar fixed o absolute que bloquee toda la pantalla
    return (
      <div 
        className="relative min-h-screen flex items-center justify-center bg-background"
        style={{ 
          position: 'relative', // NO usar 'fixed' que bloquea toda la pantalla
          zIndex: 1 // z-index bajo para no bloquear overlays necesarios
        }}
      >
        <div style={{ pointerEvents: 'none' }}>
          <Loading size="lg" text={t('loading') || 'Cargando...'} />
        </div>
      </div>
    );
  }
  
  // Si forceAllowInteraction es true, mostrar el contenido aunque isLoading sea true
  // Esto previene bloqueos indefinidos
  if (forceAllowInteraction && isLoading && !hasCheckedAuth) {
    return <>{children}</>;
  }

  // Verificar tanto el estado de React como localStorage antes de renderizar
  const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
  const actuallyAuthenticated = isAuthenticated || !!storedUser;

  // Si no está autenticado después de verificar, no renderizar nada (se redirigirá)
  if (!actuallyAuthenticated && !isLoginPage) {
    return null;
  }

  // Si está autenticado, renderizar el contenido
  return <>{children}</>;
}

