"use client";

import React, { useEffect, useState, useRef } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/auth-context";
import { Loading } from "@/components/ui/loading";
import { useTranslations } from 'next-intl';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();
  const t = useTranslations('common');
  const [hasCheckedAuth, setHasCheckedAuth] = useState(false);
  const hasRedirectedRef = useRef(false);

  useEffect(() => {
    // Esperar a que termine de cargar antes de verificar autenticación
    if (!isLoading && !hasRedirectedRef.current) {
      // Verificar tanto el estado de React como localStorage
      const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
      const actuallyAuthenticated = isAuthenticated || !!storedUser;
      
      setHasCheckedAuth(true);
      
      // Solo redirigir si definitivamente no está autenticado
      // Y no estamos ya en login (para evitar bucles)
      if (!actuallyAuthenticated) {
        const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
        if (!currentPath.includes('login')) {
          hasRedirectedRef.current = true;
          router.replace("/login");
        }
      }
    }
  }, [isAuthenticated, isLoading, router]);

  // Mientras carga, mostrar loading
  if (isLoading || !hasCheckedAuth) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loading size="lg" text={t('loading')} />
      </div>
    );
  }

  // Verificar tanto el estado de React como localStorage antes de renderizar
  const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
  const actuallyAuthenticated = isAuthenticated || !!storedUser;

  // Si no está autenticado después de verificar, no renderizar nada (se redirigirá)
  if (!actuallyAuthenticated) {
    return null;
  }

  // Si está autenticado, renderizar el contenido
  return <>{children}</>;
}

