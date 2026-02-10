"use client";

import { useRouter, usePathname } from "next/navigation";
import { SessionProvider, useSession } from "next-auth/react";
import { useEffect, useState } from "react";
import { Loading } from "@shared/components/ui/loading";
import { AdminLayout as AdminLayoutComponent } from "@/components/layout/admin-layout";
import { SidebarProvider } from "@/contexts/sidebar-context";

function AdminLayoutContent({ children }: { children: React.ReactNode }) {
  const { data: session, status } = useSession();
  const router = useRouter();
  const pathname = usePathname();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    if (status === "loading") return;
    if (pathname === "/login") {
      setIsChecking(false);
      if (session?.user && session.user.role === "Admin") {
        router.replace("/dashboard");
      }
      return;
    }
    if (!session || !session.user) {
      router.replace("/login");
      return;
    }
    if (session.user.role !== "Admin") {
      router.replace("/login");
      return;
    }
    setIsChecking(false);
  }, [session, status, router, pathname]);

  if (status === "loading" || isChecking) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loading />
      </div>
    );
  }

  if (pathname === "/login") {
    return <>{children}</>;
  }

  if (!session || session.user.role !== "Admin") {
    return null;
  }

  return (
    <SidebarProvider>
      <AdminLayoutComponent>{children}</AdminLayoutComponent>
    </SidebarProvider>
  );
}

export function AdminAppWrapper({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <AdminLayoutContent>{children}</AdminLayoutContent>
    </SessionProvider>
  );
}
