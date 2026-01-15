"use client";

import { useRouter, usePathname } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";
import {
  LayoutDashboard,
  LogOut,
  Menu,
  X,
  FileText,
} from "lucide-react";
import { useState, useEffect } from "react";
import Link from "next/link";
import { cn } from "@/lib/utils/cn";

interface AdminLayoutProps {
  children: React.ReactNode;
}

export function AdminLayout({ children }: AdminLayoutProps) {
  const router = useRouter();
  const { data: session } = useSession();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const navigation = [
    { name: "Dashboard", href: "/admin/dashboard", icon: LayoutDashboard },
    { name: "Logs", href: "/admin/logs", icon: FileText },
  ];

  const handleLogout = async () => {
    await signOut({ redirect: true, callbackUrl: "/admin/login" });
  };

  // Cerrar sidebar cuando cambia la ruta (en móvil)
  const pathname = usePathname();
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
            <AdminSidebarContent
              session={session}
              onLogout={handleLogout}
              onClose={() => setSidebarOpen(false)}
              navigation={navigation}
            />
          </div>
        </div>
      )}

      {/* Sidebar desktop */}
      <div className="hidden lg:flex lg:w-64 lg:flex-col lg:fixed lg:inset-y-0">
        <div className="flex flex-col flex-grow bg-card border-r">
          <AdminSidebarContent session={session} onLogout={handleLogout} navigation={navigation} />
        </div>
      </div>

      {/* Contenido principal */}
      <div className="flex flex-col flex-1 lg:pl-64">
        {/* Header móvil */}
        <header className="lg:hidden flex items-center justify-between p-4 bg-card border-b">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setSidebarOpen(true)}
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

function AdminSidebarContent({
  session,
  onLogout,
  onClose,
  navigation,
}: {
  session: any;
  onLogout: () => void;
  onClose?: () => void;
  navigation: Array<{ name: string; href: string; icon: any }>;
}) {
  const pathname = usePathname();

  return (
    <>
      <div className="flex items-center justify-between p-4 border-b">
        <h2 className="text-xl font-bold">GesFer Admin</h2>
        {onClose && (
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        )}
      </div>

      <nav className="flex-1 p-4 space-y-1">
        {navigation.map((item) => {
          const Icon = item.icon;
          const isActive = pathname === item.href;
          
          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onClose}
              className={cn(
                "flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors",
                "hover:bg-accent hover:text-accent-foreground",
                isActive
                  ? "bg-accent text-accent-foreground"
                  : "text-muted-foreground"
              )}
            >
              <Icon className="h-5 w-5" />
              {item.name}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t space-y-3">
        {session?.user && (
          <div className="px-3 py-2">
            <p className="text-sm font-medium">{session.user.firstName} {session.user.lastName}</p>
            <p className="text-xs text-muted-foreground">{session.user.username}</p>
          </div>
        )}
        <Button
          variant="ghost"
          className="w-full justify-start"
          onClick={onLogout}
        >
          <LogOut className="h-4 w-4 mr-2" />
          Cerrar sesión
        </Button>
      </div>
    </>
  );
}
