"use client";

import { Button } from "@/components/shared/Button";
import { Menu } from "lucide-react";
import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils/cn";
import { Sidebar } from "./Sidebar";
import { useSidebar } from "@/contexts/sidebar-context";

interface AdminLayoutProps {
  children: React.ReactNode;
}

export function AdminLayout({ children }: AdminLayoutProps) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const pathname = usePathname();
  const { isCollapsed } = useSidebar();
  
  // Remover la variable no usada

  // Cerrar sidebar móvil cuando cambia la ruta
  useEffect(() => {
    setSidebarOpen(false);
  }, [pathname]);

  // Protección adicional: timeout de seguridad para cerrar sidebar automáticamente
  useEffect(() => {
    if (sidebarOpen) {
      const safetyTimeout = setTimeout(() => {
        console.warn("AdminLayout: Timeout de seguridad activado, cerrando sidebar automáticamente");
        setSidebarOpen(false);
      }, 60000);

      return () => {
        clearTimeout(safetyTimeout);
      };
    }
  }, [sidebarOpen]);

  // Calcular padding dinámico según estado del sidebar
  const sidebarWidth = isCollapsed ? 64 : 256; // 16 (4rem) cuando colapsado, 64 (16rem) cuando expandido

  return (
    <div className="flex h-screen bg-background">
      {/* Sidebar móvil */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div
            className="fixed inset-0 bg-black/50 transition-opacity"
            onClick={() => setSidebarOpen(false)}
            aria-hidden="true"
          />
          <div className="fixed inset-y-0 left-0 w-64 bg-card border-r z-50">
            <Sidebar onClose={() => setSidebarOpen(false)} isMobile />
          </div>
        </div>
      )}

      {/* Sidebar desktop */}
      <div
        className={cn(
          "hidden lg:flex lg:flex-col lg:fixed lg:inset-y-0 transition-all duration-300",
          isCollapsed ? "lg:w-16" : "lg:w-64"
        )}
      >
        <div className="flex flex-col flex-grow bg-card border-r">
          <Sidebar />
        </div>
      </div>

      {/* Contenido principal con padding dinámico */}
      <div
        className="flex flex-col flex-1 transition-all duration-300"
        style={{ paddingLeft: isCollapsed ? "64px" : "256px" }}
      >
        {/* Header móvil */}
        <header className="lg:hidden flex items-center justify-between p-4 bg-card border-b">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setSidebarOpen(true)}
            data-testid="shared-button-sidebar-toggle"
          >
            <Menu className="h-6 w-6" />
          </Button>
          <h1 className="text-lg font-semibold">GesFer Admin</h1>
          <div className="w-10" />
        </header>

        {/* Contenido */}
        <main className="flex-1 overflow-y-auto p-4 lg:p-8">{children}</main>
      </div>
    </div>
  );
}
