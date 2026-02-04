"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Button } from "@shared/components/shared/Button";
import { Loading } from "@shared/components/ui/loading";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { ModalBase } from "@shared/components/shared/ModalBase";
import { UserForm } from "@/components/usuarios/user-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";
import { useAuth } from "@/contexts/auth-context";
import { Plus, Edit, Trash2, Users as UsersIcon, Eye } from "lucide-react";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useTranslations } from 'next-intl';
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";
import { DestructiveActionConfirm } from "@shared/components/shared/DestructiveActionConfirm";

export default function UsuariosPage() {
  const router = useRouter();
  const { user, updateUser } = useAuth();
  const queryClient = useQueryClient();
  const t = useTranslations('users');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [userToDelete, setUserToDelete] = useState<string | null>(null);

  const {
    data: usuarios,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["users", user?.companyId],
    queryFn: () => usersApi.getAll(user?.companyId),
    enabled: !!user?.companyId,
  });

  // Verificar si hay un parámetro de edición en la URL
  useEffect(() => {
    if (typeof window !== "undefined" && usuarios) {
      const params = new URLSearchParams(window.location.search);
      const editId = params.get("edit");
      if (editId) {
        const userToEdit = usuarios.find((u) => u.id === editId);
        if (userToEdit) {
          setEditingUser(userToEdit);
          // Limpiar la URL
          window.history.replaceState({}, "", "/usuarios");
        }
      }
    }
  }, [usuarios]);

  const createMutation = useMutation({
    mutationFn: (data: CreateUser) => usersApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setIsCreateModalOpen(false);
    },
    onError: (error) => {
      console.error('Error al crear usuario:', error);
      // El error se propagará al componente UserForm a través de onSubmit
      // Asegurar que el error tenga un mensaje
      if (error instanceof Error) {
        throw error;
      } else {
        throw new Error('Error al crear el usuario. Por favor, intente nuevamente.');
      }
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUser }) =>
      usersApi.update(id, data),
    onSuccess: (updatedUser, variables) => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      
      // Si el usuario actualizado es el usuario logueado, actualizar el contexto
      if (user && variables.id === user.userId) {
        const oldLanguageId = user.userLanguageId;
        const newLanguageId = variables.data.languageId || updatedUser.languageId;
        
        // Actualizar el usuario en el contexto con todos los datos actualizados
        updateUser({
          username: updatedUser.username,
          firstName: updatedUser.firstName,
          lastName: updatedUser.lastName,
          userLanguageId: newLanguageId,
          effectiveLanguageId: newLanguageId,
        });
        
        // Si cambió el idioma, recargar la página para que se carguen los nuevos mensajes
        if (newLanguageId && newLanguageId !== oldLanguageId) {
          // Esperar un poco para asegurar que las cookies se actualicen
          setTimeout(() => {
            // Recargar la página para que Next.js cargue los nuevos mensajes
            window.location.reload();
          }, 300);
        }
      }
      
      setEditingUser(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => usersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setDeletingUserId(null);
    },
  });

  const handleCreate = async (data: CreateUser | UpdateUser) => {
    try {
      await createMutation.mutateAsync(data as CreateUser);
    } catch (error) {
      // Propagar el error para que el formulario lo maneje
      // El error debe tener un mensaje claro
      if (error instanceof Error) {
        throw error;
      } else if (typeof error === 'string') {
        throw new Error(error);
      } else {
        throw new Error('Error al crear el usuario. Por favor, intente nuevamente.');
      }
    }
  };

  const handleUpdate = async (data: CreateUser | UpdateUser) => {
    if (editingUser) {
      await updateMutation.mutateAsync({
        id: editingUser.id,
        data: data as UpdateUser,
      });
    }
  };

  const handleDeleteClick = (id: string) => {
    setUserToDelete(id);
    setShowDeleteConfirm(true);
  };

  const handleDeleteConfirm = async () => {
    if (!userToDelete) return;
    
    setDeletingUserId(userToDelete);
    try {
      await deleteMutation.mutateAsync(userToDelete);
      setShowDeleteConfirm(false);
      setUserToDelete(null);
    } catch (error) {
      console.error("Error al eliminar usuario:", error);
    } finally {
      setDeletingUserId(null);
    }
  };

  const handleView = (id: string) => {
    router.push(`/usuarios/${id}`);
  };

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold" data-testid="usuarios-title">{t('title')}</h1>
              <p className="text-muted-foreground">
                {t('subtitle')}
              </p>
            </div>
            <Button 
              onClick={() => setIsCreateModalOpen(true)} 
              data-testid="shared-button-usuarios-new-user"
            >
              <Plus className="h-4 w-4 mr-2" />
              {t('newUser')}
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

          {usuarios && usuarios.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <UsersIcon className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground mb-4">
                  {t('noUsers')}
                </p>
                <Button 
                  onClick={() => setIsCreateModalOpen(true)}
                  data-testid="shared-button-usuarios-create-first"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  {t('createFirst')}
                </Button>
              </CardContent>
            </Card>
          )}

          {usuarios && usuarios.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>{t('listTitle')}</CardTitle>
                <CardDescription>
                  {t('listDescription', { count: usuarios.length })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto" data-testid="usuarios-list">
                  <table className="w-full" data-testid="usuarios-table">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">{t('table.username')}</th>
                        <th className="text-left p-2">{t('table.name')}</th>
                        <th className="text-left p-2">{t('table.email')}</th>
                        <th className="text-left p-2">{t('table.phone')}</th>
                        <th className="text-left p-2">{t('table.company')}</th>
                        <th className="text-left p-2">{t('table.status')}</th>
                        <th className="text-right p-2">{t('table.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {usuarios.map((usuario) => (
                        <tr
                          key={usuario.id}
                          className="border-b hover:bg-muted/50"
                        >
                          <td className="p-2 font-medium">{usuario.username}</td>
                          <td className="p-2">
                            {usuario.firstName} {usuario.lastName}
                          </td>
                          <td className="p-2">{usuario.email || "-"}</td>
                          <td className="p-2">{usuario.phone || "-"}</td>
                          <td className="p-2">{usuario.companyName}</td>
                          <td className="p-2">
                            <span
                              className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                                usuario.isActive
                                  ? "bg-green-100 text-green-800"
                                  : "bg-red-100 text-red-800"
                              }`}
                            >
                              {usuario.isActive ? t('table.active') : t('table.inactive')}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleView(usuario.id)}
                                title={t('table.view')}
                                data-testid={`shared-button-usuarios-view-${usuario.id}`}
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingUser(usuario)}
                                title={t('table.edit')}
                                data-testid={`shared-button-usuarios-edit-${usuario.id}`}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDeleteClick(usuario.id)}
                                disabled={deletingUserId === usuario.id}
                                title={t('table.delete')}
                                data-testid={`shared-button-usuarios-delete-${usuario.id}`}
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

          {/* Modal Crear Usuario */}
          <ModalBase
            open={isCreateModalOpen}
            onOpenChange={setIsCreateModalOpen}
            title={t('createUser')}
            description={t('createDescription')}
            data-testid="shared-modal-usuarios-create"
          >
            <UserForm
              onSubmit={handleCreate}
              onCancel={() => setIsCreateModalOpen(false)}
              isLoading={createMutation.isPending}
            />
          </ModalBase>

          {/* Modal Editar Usuario */}
          <ModalBase
            open={!!editingUser}
            onOpenChange={(open: boolean) => !open && setEditingUser(null)}
            title={t('editUser')}
            description={t('editDescription')}
            data-testid="shared-modal-usuarios-edit"
          >
            {editingUser && (
              <UserForm
                user={editingUser}
                onSubmit={handleUpdate}
                onCancel={() => setEditingUser(null)}
                isLoading={updateMutation.isPending}
              />
            )}
          </ModalBase>

          <DestructiveActionConfirm
            open={showDeleteConfirm}
            onOpenChange={setShowDeleteConfirm}
            onConfirm={handleDeleteConfirm}
            title={t('deleteConfirmTitle') || "Eliminar Usuario"}
            description={t('deleteConfirmDescription') || "Esta acción eliminará permanentemente el usuario. Esta acción no se puede deshacer."}
            confirmationKeyword="ELIMINAR"
            confirmButtonText={t('deleteConfirmButton') || "Eliminar"}
            cancelButtonText={t('cancel') || "Cancelar"}
            isLoading={deletingUserId !== null}
          />
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}


