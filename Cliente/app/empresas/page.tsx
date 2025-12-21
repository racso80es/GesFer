"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from "@/components/ui/dialog";
import { CompanyForm } from "@/components/empresas/company-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { companiesApi } from "@/lib/api/companies";
import { Plus, Edit, Trash2, Building2, Eye } from "lucide-react";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";

export default function EmpresasPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);
  const [deletingCompanyId, setDeletingCompanyId] = useState<string | null>(null);

  const {
    data: empresas,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["companies"],
    queryFn: () => companiesApi.getAll(),
  });

  // Verificar si hay un parámetro de edición en la URL
  useEffect(() => {
    if (typeof window !== "undefined" && empresas) {
      const params = new URLSearchParams(window.location.search);
      const editId = params.get("edit");
      if (editId) {
        const companyToEdit = empresas.find((c) => c.id === editId);
        if (companyToEdit) {
          setEditingCompany(companyToEdit);
          // Limpiar la URL
          window.history.replaceState({}, "", "/empresas");
        }
      }
    }
  }, [empresas]);

  const createMutation = useMutation({
    mutationFn: (data: CreateCompany) => companiesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
      setIsCreateModalOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCompany }) =>
      companiesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
      setEditingCompany(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => companiesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
      setDeletingCompanyId(null);
    },
  });

  const handleCreate = async (data: CreateCompany | UpdateCompany) => {
    await createMutation.mutateAsync(data as CreateCompany);
  };

  const handleUpdate = async (data: CreateCompany | UpdateCompany) => {
    if (editingCompany) {
      await updateMutation.mutateAsync({
        id: editingCompany.id,
        data: data as UpdateCompany,
      });
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("¿Estás seguro de que deseas eliminar esta empresa?")) {
      return;
    }
    setDeletingCompanyId(id);
    try {
      await deleteMutation.mutateAsync(id);
    } catch (error) {
      alert(
        error instanceof Error
          ? error.message
          : "Error al eliminar la empresa"
      );
    } finally {
      setDeletingCompanyId(null);
    }
  };

  const handleView = (id: string) => {
    router.push(`/empresas/${id}`);
  };

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold">Empresas</h1>
              <p className="text-muted-foreground">
                Gestiona las empresas del sistema
              </p>
            </div>
            <Button onClick={() => setIsCreateModalOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Nueva Empresa
            </Button>
          </div>

          {isLoading && (
            <div className="flex justify-center py-12">
              <Loading size="lg" text="Cargando empresas..." />
            </div>
          )}

          {error && (
            <ErrorMessage
              message={
                error instanceof Error
                  ? error.message
                  : "Error al cargar las empresas"
              }
            />
          )}

          {empresas && empresas.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground mb-4">
                  No hay empresas registradas
                </p>
                <Button onClick={() => setIsCreateModalOpen(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  Crear Primera Empresa
                </Button>
              </CardContent>
            </Card>
          )}

          {empresas && empresas.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Lista de Empresas</CardTitle>
                <CardDescription>
                  {empresas.length} empresa(s) encontrada(s)
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">Nombre</th>
                        <th className="text-left p-2">CIF/NIF</th>
                        <th className="text-left p-2">Email</th>
                        <th className="text-left p-2">Teléfono</th>
                        <th className="text-left p-2">Dirección</th>
                        <th className="text-left p-2">Estado</th>
                        <th className="text-right p-2">Acciones</th>
                      </tr>
                    </thead>
                    <tbody>
                      {empresas.map((empresa) => (
                        <tr
                          key={empresa.id}
                          className="border-b hover:bg-muted/50"
                        >
                          <td className="p-2 font-medium">{empresa.name}</td>
                          <td className="p-2">{empresa.taxId || "-"}</td>
                          <td className="p-2">{empresa.email || "-"}</td>
                          <td className="p-2">{empresa.phone || "-"}</td>
                          <td className="p-2">{empresa.address || "-"}</td>
                          <td className="p-2">
                            <span
                              className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                                empresa.isActive
                                  ? "bg-green-100 text-green-800"
                                  : "bg-red-100 text-red-800"
                              }`}
                            >
                              {empresa.isActive ? "Activa" : "Inactiva"}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleView(empresa.id)}
                                title="Ver detalle"
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingCompany(empresa)}
                                title="Editar"
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDelete(empresa.id)}
                                disabled={deletingCompanyId === empresa.id}
                                title="Eliminar"
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Modal Crear Empresa */}
          <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
            <DialogContent>
              <DialogClose onClose={() => setIsCreateModalOpen(false)} />
              <DialogHeader>
                <DialogTitle>Crear Nueva Empresa</DialogTitle>
                <DialogDescription>
                  Completa el formulario para crear una nueva empresa
                </DialogDescription>
              </DialogHeader>
              <CompanyForm
                onSubmit={handleCreate}
                onCancel={() => setIsCreateModalOpen(false)}
                isLoading={createMutation.isPending}
              />
            </DialogContent>
          </Dialog>

          {/* Modal Editar Empresa */}
          <Dialog
            open={!!editingCompany}
            onOpenChange={(open) => !open && setEditingCompany(null)}
          >
            <DialogContent>
              <DialogClose onClose={() => setEditingCompany(null)} />
              <DialogHeader>
                <DialogTitle>Editar Empresa</DialogTitle>
                <DialogDescription>
                  Modifica la información de la empresa
                </DialogDescription>
              </DialogHeader>
              {editingCompany && (
                <CompanyForm
                  company={editingCompany}
                  onSubmit={handleUpdate}
                  onCancel={() => setEditingCompany(null)}
                  isLoading={updateMutation.isPending}
                />
              )}
            </DialogContent>
          </Dialog>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}

