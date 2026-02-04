"use client";

import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { SessionProvider, useSession } from "next-auth/react";
import { Loading } from "@shared/components/ui/loading";
import { AdminLayout as AdminLayoutComponent } from "@/components/layout/admin-layout";
import { SidebarProvider } from "@/contexts/sidebar-context";

/**
 * Layout principal del Admin
 * Verifica la sesión administrativa antes de renderizar cualquier contenido
 */
function AdminLayoutContent({ children }: { children: React.ReactNode }) {
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
    if (pathname === "/login") {
      setIsChecking(false);
      // Si ya está autenticado como admin, redirigir al dashboard
      if (session?.user && session.user.role === "Admin") {
        router.replace("/dashboard");
      }
      return;
    }

    // Verificar autenticación para todas las demás rutas administrativas
    if (!session || !session.user) {
      router.replace("/login");
      return;
    }

    // Verificar que el usuario tenga rol Admin
    if (session.user.role !== "Admin") {
      router.replace("/login");
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

  // Si estamos en login y no hay sesión válida, mostrar el contenido sin layout
  if (pathname === "/login") {
    return <>{children}</>;
  }

  // Verificar nuevamente antes de renderizar contenido protegido
  if (!session || session.user.role !== "Admin") {
    return null; // El useEffect redirigirá
  }

  // Para rutas protegidas, usar el layout con navegación y SidebarProvider
  return (
    <SidebarProvider>
      <AdminLayoutComponent>{children}</AdminLayoutComponent>
    </SidebarProvider>
  );
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es">
      <body>
        <SessionProvider>
          <AdminLayoutContent>{children}</AdminLayoutContent>
        </SessionProvider>
      </body>
    </html>
  );
}

