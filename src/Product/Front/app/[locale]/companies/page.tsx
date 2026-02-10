"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Button } from "@shared/components/ui/button";
import { Loading } from "@shared/components/ui/loading";
import { ErrorMessage } from "@shared/components/ui/error-message";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from "@shared/components/ui/dialog";
import { CompanyForm } from "@/components/companies/company-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { companiesApi } from "@/lib/api/companies";
import { Plus, Edit, Trash2, Building2, Eye } from "lucide-react";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { DestructiveActionConfirm } from "@shared/components/shared/DestructiveActionConfirm";

export default function CompaniesPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const t = useTranslations('companies');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);
  const [deletingCompanyId, setDeletingCompanyId] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [companyToDelete, setCompanyToDelete] = useState<string | null>(null);

  const {
    data: companies,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["companies"],
    queryFn: () => companiesApi.getAll(),
  });

  // Verificar si hay un parámetro de edición en la URL
  useEffect(() => {
    if (typeof window !== "undefined" && companies) {
      const params = new URLSearchParams(window.location.search);
      const editId = params.get("edit");
      if (editId) {
        const companyToEdit = companies.find((c) => c.id === editId);
        if (companyToEdit) {
          setEditingCompany(companyToEdit);
          // Limpiar la URL
          window.history.replaceState({}, "", "/companies");
        }
      }
    }
  }, [companies]);

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
      console.error("Error al eliminar company:", error);
    } finally {
      setDeletingCompanyId(null);
    }
  };

  const handleView = (id: string) => {
    router.push(`/companies/${id}`);
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
            <Button onClick={() => setIsCreateModalOpen(true)}>
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

          {companies && companies.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground mb-4">
                  {t('noCompanies')}
                </p>
                <Button onClick={() => setIsCreateModalOpen(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  {t('createFirst')}
                </Button>
              </CardContent>
            </Card>
          )}

          {companies && companies.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>{t('listTitle')}</CardTitle>
                <CardDescription>
                  {t('listDescription', { count: companies.length })}
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
                      {companies.map((company) => (
                        <tr
                          key={company.id}
                          className="border-b hover:bg-muted/50"
                        >
                          <td className="p-2 font-medium">{company.name}</td>
                          <td className="p-2">{company.taxId || "-"}</td>
                          <td className="p-2">{company.email || "-"}</td>
                          <td className="p-2">{company.phone || "-"}</td>
                          <td className="p-2">{company.address || "-"}</td>
                          <td className="p-2">
                            <span
                              className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                                company.isActive
                                  ? "bg-green-100 text-green-800"
                                  : "bg-red-100 text-red-800"
                              }`}
                            >
                              {company.isActive ? t('table.active') : t('table.inactive')}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleView(company.id)}
                                title={t('table.view')}
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingCompany(company)}
                                title={t('table.edit')}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDeleteClick(company.id)}
                                disabled={deletingCompanyId === company.id}
                                title={t('table.delete')}
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

          {/* Modal Crear Organización */}
          <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
            <DialogContent>
              <DialogClose onClose={() => setIsCreateModalOpen(false)} />
              <DialogHeader>
                <DialogTitle>{t('createCompany')}</DialogTitle>
                <DialogDescription>
                  {t('createDescription')}
                </DialogDescription>
              </DialogHeader>
              <CompanyForm
                onSubmit={handleCreate}
                onCancel={() => setIsCreateModalOpen(false)}
                isLoading={createMutation.isPending}
              />
            </DialogContent>
          </Dialog>

          {/* Modal Editar Organización */}
          <Dialog
            open={!!editingCompany}
            onOpenChange={(open: boolean) => !open && setEditingCompany(null)}
          >
            <DialogContent>
              <DialogClose onClose={() => setEditingCompany(null)} />
              <DialogHeader>
                <DialogTitle>{t('editCompany')}</DialogTitle>
                <DialogDescription>
                  {t('editDescription')}
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

          <DestructiveActionConfirm
            open={showDeleteConfirm}
            onOpenChange={setShowDeleteConfirm}
            onConfirm={handleDeleteConfirm}
            title={t('deleteConfirmTitle') || "Eliminar Organización"}
            description={t('deleteConfirmDescription') || "Esta acción eliminará permanentemente la organización. Esta acción no se puede deshacer."}
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


