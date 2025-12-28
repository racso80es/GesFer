"use client";

import { useRouter, usePathname } from "next/navigation";
import { useAuth } from "@/contexts/auth-context";
import { Button } from "@/components/ui/button";
import {
  LayoutDashboard,
  Users,
  Building2,
  LogOut,
  Menu,
  X,
  Briefcase,
} from "lucide-react";
import { useState } from "react";
import Link from "next/link";
import { cn } from "@/lib/utils/cn";
import { useTranslations, useLocale } from 'next-intl';

interface MainLayoutProps {
  children: React.ReactNode;
}

export function MainLayout({ children }: MainLayoutProps) {
  const router = useRouter();
  const locale = useLocale();
  const { user, logout } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const t = useTranslations('navigation');
  const tAuth = useTranslations('auth');

  const navigation = [
    { name: t('dashboard'), href: "/dashboard", icon: LayoutDashboard },
    { name: t('companies'), href: "/empresas", icon: Briefcase },
    { name: t('users'), href: "/usuarios", icon: Users },
    { name: t('customers'), href: "/clientes", icon: Building2 },
  ];

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  return (
    <div className="flex h-screen bg-background">
      {/* Sidebar móvil */}
      <div
        className={cn(
          "fixed inset-0 z-50 lg:hidden",
          sidebarOpen ? "block" : "hidden"
        )}
      >
        <div
          className="fixed inset-0 bg-black/50"
          onClick={() => setSidebarOpen(false)}
        />
        <div className="fixed inset-y-0 left-0 w-64 bg-card border-r">
          <SidebarContent
            user={user}
            onLogout={handleLogout}
            onClose={() => setSidebarOpen(false)}
            navigation={navigation}
          />
        </div>
      </div>

      {/* Sidebar desktop */}
      <div className="hidden lg:flex lg:w-64 lg:flex-col lg:fixed lg:inset-y-0">
        <div className="flex flex-col flex-grow bg-card border-r">
          <SidebarContent user={user} onLogout={handleLogout} navigation={navigation} />
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
          <h1 className="text-lg font-semibold">GesFer</h1>
          <div className="w-10" />
        </header>

        {/* Contenido */}
        <main className="flex-1 overflow-y-auto p-4 lg:p-8">{children}</main>
      </div>
    </div>
  );
}

function SidebarContent({
  user,
  onLogout,
  onClose,
  navigation,
}: {
  user: any;
  onLogout: () => void;
  onClose?: () => void;
  navigation: Array<{ name: string; href: string; icon: any }>;
}) {
  const pathname = usePathname();
  const tAuth = useTranslations('auth');

  // Normalizar pathname para comparación (remover locale si está presente)
  const normalizePathname = (path: string) => {
    const segments = path.split('/');
    // Si el primer segmento es un locale, removerlo
    if (segments[1] && ['es', 'en', 'ca'].includes(segments[1])) {
      return '/' + segments.slice(2).join('/') || '/';
    }
    return path;
  };

  const normalizedPathname = normalizePathname(pathname);

  return (
    <>
      <div className="flex items-center justify-between p-4 border-b">
        <h2 className="text-xl font-bold">GesFer</h2>
        {onClose && (
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        )}
      </div>

      <nav className="flex-1 p-4 space-y-1">
        {navigation.map((item) => {
          const Icon = item.icon;
          const isActive = normalizedPathname === item.href;
          // Mapear hrefs a test-ids
          const testIdMap: Record<string, string> = {
            '/dashboard': 'dashboard-dashboard-link',
            '/empresas': 'dashboard-empresas-link',
            '/usuarios': 'dashboard-usuarios-link',
            '/clientes': 'dashboard-clientes-link',
          };
          const testId = testIdMap[item.href] || `dashboard-${item.href.replace('/', '')}-link`;
          
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
              data-testid={testId}
            >
              <Icon className="h-5 w-5" />
              {item.name}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t space-y-3">
        {user && (
          <div className="px-3 py-2">
            <p className="text-sm font-medium">{user.firstName} {user.lastName}</p>
            <p className="text-xs text-muted-foreground">{user.companyName}</p>
          </div>
        )}
        <Button
          variant="ghost"
          className="w-full justify-start"
          onClick={onLogout}
          data-testid="dashboard-logout-button"
        >
          <LogOut className="h-4 w-4 mr-2" />
          {tAuth('logout')}
        </Button>
      </div>
    </>
  );
}

