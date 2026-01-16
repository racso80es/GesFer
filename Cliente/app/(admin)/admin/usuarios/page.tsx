"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/shared/Button";
import { Input } from "@/components/shared/Input";
import { ModalBase } from "@/components/shared/ModalBase";
import { DataTable } from "@/components/shared/DataTable";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { UserForm } from "@/components/admin/UserForm";
import { usersApi } from "@/lib/api/users";
import { companiesApi } from "@/lib/api/companies";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";
import { Plus, Edit, Trash2, Search, Users as UsersIcon } from "lucide-react";
import { useTranslations } from "next-intl";

export default function AdminUsuariosPage() {
  const queryClient = useQueryClient();
  const t = useTranslations("users");
  const tCommon = useTranslations("common");

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>("");

  // Obtener todas las empresas para el filtro
  const { data: companies } = useQuery({
    queryKey: ["companies"],
    queryFn: () => companiesApi.getAll(),
  });

  // Obtener usuarios
  const {
    data: usuarios,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["users", selectedCompanyId],
    queryFn: () => usersApi.getAll(selectedCompanyId || undefined),
  });

  // Filtrar usuarios por término de búsqueda
  const filteredUsuarios = usuarios?.filter((usuario) => {
    const search = searchTerm.toLowerCase();
    return (
      usuario.username.toLowerCase().includes(search) ||
      usuario.firstName.toLowerCase().includes(search) ||
      usuario.lastName.toLowerCase().includes(search) ||
      usuario.email?.toLowerCase().includes(search) ||
      usuario.companyName.toLowerCase().includes(search)
    );
  }) || [];

  const createMutation = useMutation({
    mutationFn: (data: CreateUser) => usersApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setIsCreateModalOpen(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUser }) =>
      usersApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setEditingUser(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => usersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });

  const handleCreate = async (data: CreateUser | UpdateUser) => {
    await createMutation.mutateAsync(data as CreateUser);
  };

  const handleUpdate = async (data: CreateUser | UpdateUser) => {
    if (editingUser) {
      await updateMutation.mutateAsync({
        id: editingUser.id,
        data: data as UpdateUser,
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
      key: "username",
      title: t("table.username"),
      render: (usuario: User) => usuario.username,
    },
    {
      key: "name",
      title: t("table.name"),
      render: (usuario: User) => `${usuario.firstName} ${usuario.lastName}`,
    },
    {
      key: "email",
      title: t("table.email"),
      render: (usuario: User) => usuario.email || "-",
    },
    {
      key: "company",
      title: t("table.company"),
      render: (usuario: User) => usuario.companyName,
    },
    {
      key: "status",
      title: t("table.status"),
      render: (usuario: User) => (
        <span
          className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
            usuario.isActive
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {usuario.isActive ? t("table.active") : t("table.inactive")}
        </span>
      ),
    },
    {
      key: "actions",
      title: t("table.actions"),
      render: (usuario: User) => (
        <div className="flex gap-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setEditingUser(usuario)}
            title={t("table.edit")}
            data-testid={`shared-button-usuarios-edit-${usuario.id}`}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={() => handleDelete(usuario.id)}
            title={t("table.delete")}
            data-testid={`shared-button-usuarios-delete-${usuario.id}`}
          >
            <Trash2 className="h-4 w-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6" data-testid="admin-usuarios-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold" data-testid="admin-usuarios-title">
            {t("title")}
          </h1>
          <p className="text-muted-foreground">{t("subtitle")}</p>
        </div>
        <Button
          onClick={() => setIsCreateModalOpen(true)}
          data-testid="shared-button-usuarios-new-user"
        >
          <Plus className="h-4 w-4 mr-2" />
          {t("newUser")}
        </Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4 items-end">
        <div className="flex-1 space-y-2">
          <label htmlFor="search" className="text-sm font-medium">
            {tCommon("search")}
          </label>
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              id="search"
              type="text"
              placeholder={tCommon("searchPlaceholder")}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
              data-testid="shared-input-text-search-users"
            />
          </div>
        </div>
        <div className="space-y-2">
          <label htmlFor="company" className="text-sm font-medium">
            {t("filterByCompany")}
          </label>
          <select
            id="company"
            value={selectedCompanyId}
            onChange={(e) => setSelectedCompanyId(e.target.value)}
            className="h-10 w-48 rounded-md border border-input bg-background px-3 py-2 text-sm"
            data-testid="shared-select-company-filter"
          >
            <option value="">{tCommon("all")}</option>
            {companies?.map((company) => (
              <option key={company.id} value={company.id}>
                {company.name}
              </option>
            ))}
          </select>
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
          data={filteredUsuarios}
          columns={columns}
          getRowKey={(usuario) => usuario.id}
          loading={isLoading}
          emptyMessage={t("noUsers")}
          data-testid="shared-datatable-admin-usuarios"
        />
      )}

      {/* Modal Crear Usuario */}
      <ModalBase
        open={isCreateModalOpen}
        onOpenChange={setIsCreateModalOpen}
        title={t("createUser")}
        description={t("createDescription")}
        data-testid="shared-modal-usuarios-create"
      >
        <UserForm
          companyId={selectedCompanyId || (companies?.[0]?.id || "")}
          onSubmit={handleCreate}
          onCancel={() => setIsCreateModalOpen(false)}
          isLoading={createMutation.isPending}
        />
      </ModalBase>

      {/* Modal Editar Usuario */}
      <ModalBase
        open={!!editingUser}
        onOpenChange={(open) => !open && setEditingUser(null)}
        title={t("editUser")}
        description={t("editDescription")}
        data-testid="shared-modal-usuarios-edit"
      >
        {editingUser && (
          <UserForm
            user={editingUser}
            companyId={editingUser.companyId}
            onSubmit={handleUpdate}
            onCancel={() => setEditingUser(null)}
            isLoading={updateMutation.isPending}
          />
        )}
      </ModalBase>
    </div>
  );
}
