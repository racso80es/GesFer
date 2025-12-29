"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { useQuery } from "@tanstack/react-query";
import { companiesApi } from "@/lib/api/companies";
import { useRouter } from "next/navigation";
import { ArrowLeft, Edit, Building2 } from "lucide-react";
import { format } from "date-fns";
import { useState, useEffect } from "react";

export default function CompanyDetailPage({
  params,
}: {
  params: Promise<{ id: string; locale: string }> | { id: string; locale: string };
}) {
  const router = useRouter();
  const [companyId, setCompanyId] = useState<string | null>(null);

  useEffect(() => {
    if (params instanceof Promise) {
      params.then((resolvedParams) => {
        setCompanyId(resolvedParams.id);
      });
    } else {
      setCompanyId(params.id);
    }
  }, [params]);

  const {
    data: company,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["company", companyId],
    queryFn: () => companiesApi.getById(companyId!),
    enabled: !!companyId,
  });

  if (isLoading) {
    return (
      <ProtectedRoute>
        <MainLayout>
          <div className="flex justify-center py-12">
            <Loading size="lg" text="Cargando empresa..." />
          </div>
        </MainLayout>
      </ProtectedRoute>
    );
  }

  if (error || !company) {
    return (
      <ProtectedRoute>
        <MainLayout>
          <div className="space-y-6">
            <Button
              variant="ghost"
              onClick={() => router.back()}
              className="mb-4"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Volver
            </Button>
            <ErrorMessage
              message={
                error instanceof Error
                  ? error.message
                  : "Empresa no encontrada"
              }
            />
          </div>
        </MainLayout>
      </ProtectedRoute>
    );
  }

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => router.back()}
              >
                <ArrowLeft className="h-4 w-4" />
              </Button>
              <div>
                <h1 className="text-3xl font-bold">{company.name}</h1>
                <p className="text-muted-foreground">Detalle de la empresa</p>
              </div>
            </div>
            <Button
              variant="outline"
              onClick={() => router.push(`/empresas?edit=${company.id}`)}
            >
              <Edit className="h-4 w-4 mr-2" />
              Editar
            </Button>
          </div>

          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Building2 className="h-5 w-5" />
                  Información de la Empresa
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Nombre
                  </p>
                  <p className="text-base">{company.name}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    CIF/NIF
                  </p>
                  <p className="text-base">{company.taxId || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Email
                  </p>
                  <p className="text-base">{company.email || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Teléfono
                  </p>
                  <p className="text-base">{company.phone || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Dirección
                  </p>
                  <p className="text-base">{company.address || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Estado
                  </p>
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                      company.isActive
                        ? "bg-green-100 text-green-800"
                        : "bg-red-100 text-red-800"
                    }`}
                  >
                    {company.isActive ? "Activa" : "Inactiva"}
                  </span>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Información del Sistema</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    Fecha de Creación
                  </p>
                  <p className="text-base">
                    {format(new Date(company.createdAt), "PPpp")}
                  </p>
                </div>
                {company.updatedAt && (
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">
                      Última Actualización
                    </p>
                    <p className="text-base">
                      {format(new Date(company.updatedAt), "PPpp")}
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}




