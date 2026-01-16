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
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import Link from "next/link";
import { cn } from "@/lib/utils/cn";
import { useSidebar } from "@/contexts/sidebar-context";

interface SidebarProps {
  onClose?: () => void;
  isMobile?: boolean;
}

export function Sidebar({ onClose, isMobile = false }: SidebarProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { data: session } = useSession();
  const { isCollapsed, toggleSidebar } = useSidebar();

  const navigation = [
    { name: "Dashboard", href: "/admin/dashboard", icon: LayoutDashboard },
    { name: "Logs", href: "/admin/logs", icon: FileText },
  ];

  const handleLogout = async () => {
    await signOut({ redirect: true, callbackUrl: "/admin/login" });
  };

  return (
    <>
      <div className="flex items-center justify-between p-4 border-b">
        {!isCollapsed && <h2 className="text-xl font-bold">GesFer Admin</h2>}
        {isMobile && onClose && (
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        )}
        {!isMobile && (
          <Button variant="ghost" size="icon" onClick={toggleSidebar}>
            {isCollapsed ? (
              <ChevronRight className="h-5 w-5" />
            ) : (
              <ChevronLeft className="h-5 w-5" />
            )}
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
                  : "text-muted-foreground",
                isCollapsed && "justify-center"
              )}
              title={isCollapsed ? item.name : undefined}
            >
              <Icon className="h-5 w-5 flex-shrink-0" />
              {!isCollapsed && <span>{item.name}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t space-y-3">
        {session?.user && !isCollapsed && (
          <div className="px-3 py-2">
            <p className="text-sm font-medium">
              {session.user.firstName} {session.user.lastName}
            </p>
            <p className="text-xs text-muted-foreground">{session.user.username}</p>
          </div>
        )}
        <Button
          variant="ghost"
          className={cn(
            "w-full justify-start",
            isCollapsed && "justify-center px-2"
          )}
          onClick={handleLogout}
          title={isCollapsed ? "Cerrar sesión" : undefined}
        >
          <LogOut className="h-4 w-4 mr-2 flex-shrink-0" />
          {!isCollapsed && <span>Cerrar sesión</span>}
        </Button>
      </div>
    </>
  );
}
