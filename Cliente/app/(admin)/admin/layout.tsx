"use client";

import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import { Loading } from "@/components/ui/loading";

/**
 * Layout administrativo con Client-Side Middleware
 * Verifica la sesión administrativa antes de renderizar cualquier contenido
 * Hereda los estilos globales (Tailwind, Fuentes) del layout.tsx raíz
 */
export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { data: session, status } = useSession();
  const router = useRouter();
  const pathname = usePathname();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    // Esperar a que termine de cargar la sesión
    if (status === "loading") {
      return;
    }

    // Verificar si la ruta es login (no requiere autenticación)
    if (pathname === "/admin/login") {
      setIsChecking(false);
      // Si ya está autenticado como admin, redirigir al dashboard
      if (session?.user && session.user.role === "Admin") {
        router.replace("/admin/dashboard");
      }
      return;
    }

    // Verificar autenticación para todas las demás rutas administrativas
    if (!session || !session.user) {
      router.replace("/admin/login");
      return;
    }

    // Verificar que el usuario tenga rol Admin
    if (session.user.role !== "Admin") {
      router.replace("/admin/login");
      return;
    }

    // Sesión válida, permitir acceso
    setIsChecking(false);
  }, [session, status, router, pathname]);

  // Mostrar loading mientras se verifica la sesión
  if (status === "loading" || isChecking) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loading />
      </div>
    );
  }

  // Si estamos en login y no hay sesión válida, mostrar el contenido
  if (pathname === "/admin/login") {
    return <>{children}</>;
  }

  // Verificar nuevamente antes de renderizar contenido protegido
  if (!session || session.user.role !== "Admin") {
    return null; // El useEffect redirigirá
  }

  return <>{children}</>;
}
