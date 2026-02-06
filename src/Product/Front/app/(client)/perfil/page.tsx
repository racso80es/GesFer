"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@shared/components/shared/Button";
import { Input } from "@shared/components/shared/Input";
import { ModalBase } from "@shared/components/shared/ModalBase";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { UserForm } from "@/components/usuarios/user-form";
import { usersApi } from "@/lib/api/users";
import type { UpdateUser, CreateUser } from "@/lib/types/api";
import { useAuth } from "@/contexts/auth-context";
import { Edit, User as UserIcon } from "lucide-react";
import { useTranslations } from "next-intl";

export default function PerfilPage() {
  const { user: loggedUser, updateUser } = useAuth();
  const queryClient = useQueryClient();
  const t = useTranslations("profile");
  const tCommon = useTranslations("common");

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const {
    data: user,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["user", loggedUser?.userId],
    queryFn: () => usersApi.getById(loggedUser?.userId || ""),
    enabled: !!loggedUser?.userId,
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUser }) =>
      usersApi.update(id, data),
    onSuccess: (updatedUser) => {
      queryClient.invalidateQueries({ queryKey: ["user"] });
      
      // Actualizar el usuario en el contexto
      if (loggedUser && updatedUser) {
        const newLanguageId = updatedUser.languageId || loggedUser.userLanguageId;
        updateUser({
          username: updatedUser.username,
          firstName: updatedUser.firstName,
          lastName: updatedUser.lastName,
          userLanguageId: newLanguageId,
          effectiveLanguageId: newLanguageId,
        });

        // Si cambió el idioma, recargar la página
        if (newLanguageId && newLanguageId !== loggedUser.userLanguageId) {
          setTimeout(() => {
            window.location.reload();
          }, 300);
        }
      }
      
      setIsEditModalOpen(false);
    },
  });

  const handleUpdate = async (data: CreateUser | UpdateUser) => {
    if (user) {
      await updateMutation.mutateAsync({
        id: user.id,
        data: data as UpdateUser,
      });
    }
  };

  if (isLoading) {
    return (
      <ProtectedRoute>
        <MainLayout>
          <div className="flex justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
          </div>
        </MainLayout>
      </ProtectedRoute>
    );
  }

  if (error || !user) {
    return (
      <ProtectedRoute>
        <MainLayout>
          <div className="space-y-6">
            <h1 className="text-3xl font-bold">{t("title")}</h1>
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : t("error")}
            </p>
          </div>
        </MainLayout>
      </ProtectedRoute>
    );
  }

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6" data-testid="client-perfil-page">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold" data-testid="client-perfil-title">
                {t("title")}
              </h1>
              <p className="text-muted-foreground">{t("subtitle")}</p>
            </div>
            <Button
              onClick={() => setIsEditModalOpen(true)}
              data-testid="shared-button-perfil-edit"
            >
              <Edit className="h-4 w-4 mr-2" />
              {t("edit")}
            </Button>
          </div>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <UserIcon className="h-5 w-5" />
                {t("personalInfo")}
              </CardTitle>
              <CardDescription>{t("personalInfoDescription")}</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("username")}
                  </p>
                  <p className="text-base">{user.username}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("name")}
                  </p>
                  <p className="text-base">
                    {user.firstName} {user.lastName}
                  </p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("email")}
                  </p>
                  <p className="text-base">{user.email || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("phone")}
                  </p>
                  <p className="text-base">{user.phone || "-"}</p>
                </div>
                <div className="md:col-span-2">
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("address")}
                  </p>
                  <p className="text-base">{user.address || "-"}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("company")}
                  </p>
                  <p className="text-base">{user.companyName}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">
                    {t("status")}
                  </p>
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                      user.isActive
                        ? "bg-green-100 text-green-800"
                        : "bg-red-100 text-red-800"
                    }`}
                  >
                    {user.isActive ? t("active") : t("inactive")}
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Modal Editar Perfil */}
          <ModalBase
            open={isEditModalOpen}
            onOpenChange={setIsEditModalOpen}
            title={t("editProfile")}
            description={t("editProfileDescription")}
            data-testid="shared-modal-perfil-edit"
          >
            <UserForm
              user={user}
              onSubmit={handleUpdate}
              onCancel={() => setIsEditModalOpen(false)}
              isLoading={updateMutation.isPending}
            />
          </ModalBase>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}
