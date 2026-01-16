"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/shared/Button";
import { Input } from "@/components/shared/Input";
import { ModalBase } from "@/components/shared/ModalBase";
import { DataTable } from "@/components/shared/DataTable";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { CompanyForm } from "@/components/admin/CompanyForm";
import { companiesApi } from "@/lib/api/companies";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { Plus, Edit, Trash2, Search, Building2 } from "lucide-react";
import { useTranslations } from "next-intl";

export default function AdminEmpresasPage() {
  const queryClient = useQueryClient();
  const t = useTranslations("companies");
  const tCommon = useTranslations("common");

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);
  const [searchTerm, setSearchTerm] = useState("");

  const {
    data: empresas,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["companies"],
    queryFn: () => companiesApi.getAll(),
  });

  // Filtrar empresas por término de búsqueda
  const filteredEmpresas = empresas?.filter((empresa) => {
    const search = searchTerm.toLowerCase();
    return (
      empresa.name.toLowerCase().includes(search) ||
      empresa.taxId?.toLowerCase().includes(search) ||
      empresa.email?.toLowerCase().includes(search) ||
      empresa.address.toLowerCase().includes(search)
    );
  }) || [];

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
    if (!confirm(t("deleteConfirm"))) {
      return;
    }
    await deleteMutation.mutateAsync(id);
  };

  const columns = [
    {
      key: "name",
      title: t("table.name"),
      render: (empresa: Company) => empresa.name,
    },
    {
      key: "taxId",
      title: t("table.taxId"),
      render: (empresa: Company) => empresa.taxId || "-",
    },
    {
      key: "email",
      title: t("table.email"),
      render: (empresa: Company) => empresa.email || "-",
    },
    {
      key: "address",
      title: t("table.address"),
      render: (empresa: Company) => empresa.address,
    },
    {
      key: "status",
      title: t("table.status"),
      render: (empresa: Company) => (
        <span
          className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
            empresa.isActive
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {empresa.isActive ? t("table.active") : t("table.inactive")}
        </span>
      ),
    },
    {
      key: "actions",
      title: t("table.actions"),
      render: (empresa: Company) => (
        <div className="flex gap-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setEditingCompany(empresa)}
            title={t("table.edit")}
            data-testid={`shared-button-empresas-edit-${empresa.id}`}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={() => handleDelete(empresa.id)}
            title={t("table.delete")}
            data-testid={`shared-button-empresas-delete-${empresa.id}`}
          >
            <Trash2 className="h-4 w-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6" data-testid="admin-empresas-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold" data-testid="admin-empresas-title">
            {t("title")}
          </h1>
          <p className="text-muted-foreground">{t("subtitle")}</p>
        </div>
        <Button
          onClick={() => setIsCreateModalOpen(true)}
          data-testid="shared-button-empresas-new-company"
        >
          <Plus className="h-4 w-4 mr-2" />
          {t("newCompany")}
        </Button>
      </div>

      {/* Filtro de búsqueda */}
      <div className="space-y-2">
        <label htmlFor="search" className="text-sm font-medium">
          {tCommon("search")}
        </label>
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
          <Input
            id="search"
            type="text"
            placeholder={tCommon("searchPlaceholder")}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
            data-testid="shared-input-text-search-companies"
          />
        </div>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Loading size="lg" text={tCommon("loading")} />
        </div>
      )}

      {error && (
        <ErrorMessage
          message={
            error instanceof Error ? error.message : tCommon("error")
          }
        />
      )}

      {!isLoading && !error && (
        <DataTable
          data={filteredEmpresas}
          columns={columns}
          getRowKey={(empresa) => empresa.id}
          loading={isLoading}
          emptyMessage={t("noCompanies")}
          data-testid="shared-datatable-admin-empresas"
        />
      )}

      {/* Modal Crear Empresa */}
      <ModalBase
        open={isCreateModalOpen}
        onOpenChange={setIsCreateModalOpen}
        title={t("createCompany")}
        description={t("createDescription")}
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
        onOpenChange={(open) => !open && setEditingCompany(null)}
        title={t("editCompany")}
        description={t("editDescription")}
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
    </div>
  );
}
