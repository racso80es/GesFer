"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Button } from "@shared/components/shared/Button";
import { Loading } from "@shared/components/ui/loading";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { ModalBase } from "@shared/components/shared/ModalBase";
import { CompanyForm } from "@/components/empresas/company-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { companiesApi } from "@/lib/api/companies";
import { Plus, Edit, Trash2, Building2, Eye } from "lucide-react";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { DestructiveActionConfirm } from "@shared/components/shared/DestructiveActionConfirm";

export default function EmpresasPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const t = useTranslations('companies');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);
  const [deletingCompanyId, setDeletingCompanyId] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [companyToDelete, setCompanyToDelete] = useState<string | null>(null);

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

  const handleDeleteClick = (id: string) => {
    setCompanyToDelete(id);
    setShowDeleteConfirm(true);
  };

  const handleDeleteConfirm = async () => {
    if (!companyToDelete) return;
    
    setDeletingCompanyId(companyToDelete);
    try {
      await deleteMutation.mutateAsync(companyToDelete);
      setShowDeleteConfirm(false);
      setCompanyToDelete(null);
    } catch (error) {
      console.error("Error al eliminar empresa:", error);
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
              <h1 className="text-3xl font-bold">{t('title')}</h1>
              <p className="text-muted-foreground">
                {t('subtitle')}
              </p>
            </div>
            <Button 
              onClick={() => setIsCreateModalOpen(true)}
              data-testid="shared-button-empresas-new-company"
            >
              <Plus className="h-4 w-4 mr-2" />
              {t('newCompany')}
            </Button>
          </div>

          {isLoading && (
            <div className="flex justify-center py-12">
              <Loading size="lg" text={t('loading')} />
            </div>
          )}

          {error && (
            <ErrorMessage
              message={
                error instanceof Error
                  ? error.message
                  : t('error')
              }
            />
          )}

          {empresas && empresas.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground mb-4">
                  {t('noCompanies')}
                </p>
                <Button 
                  onClick={() => setIsCreateModalOpen(true)}
                  data-testid="shared-button-empresas-create-first"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  {t('createFirst')}
                </Button>
              </CardContent>
            </Card>
          )}

          {empresas && empresas.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>{t('listTitle')}</CardTitle>
                <CardDescription>
                  {t('listDescription', { count: empresas.length })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">{t('table.name')}</th>
                        <th className="text-left p-2">{t('table.taxId')}</th>
                        <th className="text-left p-2">{t('table.email')}</th>
                        <th className="text-left p-2">{t('table.phone')}</th>
                        <th className="text-left p-2">{t('table.address')}</th>
                        <th className="text-left p-2">{t('table.status')}</th>
                        <th className="text-right p-2">{t('table.actions')}</th>
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
                              {empresa.isActive ? t('table.active') : t('table.inactive')}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleView(empresa.id)}
                                title={t('table.view')}
                                data-testid={`shared-button-empresas-view-${empresa.id}`}
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingCompany(empresa)}
                                title={t('table.edit')}
                                data-testid={`shared-button-empresas-edit-${empresa.id}`}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDeleteClick(empresa.id)}
                                disabled={deletingCompanyId === empresa.id}
                                title={t('table.delete')}
                                data-testid={`shared-button-empresas-delete-${empresa.id}`}
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
          <ModalBase
            open={isCreateModalOpen}
            onOpenChange={setIsCreateModalOpen}
            title={t('createCompany')}
            description={t('createDescription')}
            data-testid="shared-modal-empresas-create"
          >
            <CompanyForm
              onSubmit={handleCreate}
              onCancel={() => setIsCreateModalOpen(false)}
              isLoading={createMutation.isPending}
            />
          </ModalBase>

          {/* Modal Editar Empresa */}
          <ModalBase
            open={!!editingCompany}
            onOpenChange={(open: boolean) => !open && setEditingCompany(null)}
            title={t('editCompany')}
            description={t('editDescription')}
            data-testid="shared-modal-empresas-edit"
          >
            {editingCompany && (
              <CompanyForm
                company={editingCompany}
                onSubmit={handleUpdate}
                onCancel={() => setEditingCompany(null)}
                isLoading={updateMutation.isPending}
              />
            )}
          </ModalBase>

          <DestructiveActionConfirm
            open={showDeleteConfirm}
            onOpenChange={setShowDeleteConfirm}
            onConfirm={handleDeleteConfirm}
            title={t('deleteConfirmTitle') || "Eliminar Empresa"}
            description={t('deleteConfirmDescription') || "Esta acción eliminará permanentemente la empresa. Esta acción no se puede deshacer."}
            confirmationKeyword="ELIMINAR"
            confirmButtonText={t('deleteConfirmButton') || "Eliminar"}
            cancelButtonText={t('cancel') || "Cancelar"}
            isLoading={deletingCompanyId !== null}
          />
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}


