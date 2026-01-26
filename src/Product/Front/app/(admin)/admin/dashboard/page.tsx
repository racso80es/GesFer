"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { Building2, Users, Package, Truck, ShoppingCart, Activity } from "lucide-react";

interface DashboardSummary {
  totalCompanies: number;
  totalUsers: number;
  activeUsers: number;
  totalArticles: number;
  totalSuppliers: number;
  totalCustomers: number;
  generatedAt: string;
}

export default function AdminDashboardPage() {
  const { data: session } = useSession();
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSummary = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
        const token = (session as any)?.accessToken;

        if (!token) {
          setError("Token de autenticación no disponible");
          setIsLoading(false);
          return;
        }

        const response = await fetch(`${apiUrl}/api/admin/dashboard/summary`, {
          method: "GET",
          headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        });

        if (!response.ok) {
          if (response.status === 401 || response.status === 403) {
            setError("No tienes permisos para acceder al dashboard administrativo");
          } else {
            setError(`Error al cargar el resumen: ${response.statusText}`);
          }
          setIsLoading(false);
          return;
        }

        const data = await response.json();
        setSummary(data);
      } catch (err) {
        console.error("Error al cargar el resumen:", err);
        setError("Error al conectar con el servidor");
      } finally {
        setIsLoading(false);
      }
    };

    if (session?.user && session.user.role === "Admin") {
      fetchSummary();
    }
  }, [session]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loading />
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <ErrorMessage message={error} />
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Dashboard Administrativo</h1>
        <p className="text-muted-foreground">
          Bienvenido, {session?.user?.firstName} {session?.user?.lastName}
        </p>
      </div>

      {summary && (
        <>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Empresas</CardTitle>
                <Building2 className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{summary.totalCompanies}</div>
                <p className="text-xs text-muted-foreground">Empresas registradas</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Usuarios</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{summary.totalUsers}</div>
                <p className="text-xs text-muted-foreground">
                  {summary.activeUsers} usuarios activos
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Artículos</CardTitle>
                <Package className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{summary.totalArticles}</div>
                <p className="text-xs text-muted-foreground">Artículos en el catálogo</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Proveedores</CardTitle>
                <Truck className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{summary.totalSuppliers}</div>
                <p className="text-xs text-muted-foreground">Proveedores registrados</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Clientes</CardTitle>
                <ShoppingCart className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{summary.totalCustomers}</div>
                <p className="text-xs text-muted-foreground">Clientes registrados</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Última Actualización</CardTitle>
                <Activity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-sm font-medium">
                  {new Date(summary.generatedAt).toLocaleString("es-ES")}
                </div>
                <p className="text-xs text-muted-foreground">Datos actualizados</p>
              </CardContent>
            </Card>
          </div>

          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Información de Sesión</CardTitle>
              <CardDescription>Datos de tu sesión administrativa actual</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2 text-sm">
                <p>
                  <strong>Usuario:</strong> {session?.user?.username}
                </p>
                <p>
                  <strong>Nombre:</strong> {session?.user?.firstName} {session?.user?.lastName}
                </p>
                <p>
                  <strong>Rol:</strong> {session?.user?.role}
                </p>
                <p>
                  <strong>Cursor ID:</strong> {session?.user?.cursorId}
                </p>
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
