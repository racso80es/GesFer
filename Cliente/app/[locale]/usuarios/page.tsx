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
import { UserForm } from "@/components/usuarios/user-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";
import { useAuth } from "@/contexts/auth-context";
import { Plus, Edit, Trash2, Users as UsersIcon, Eye } from "lucide-react";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";

export default function UsuariosPage() {
  const router = useRouter();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);

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
      setDeletingUserId(null);
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
    if (!confirm("¿Estás seguro de que deseas eliminar este usuario?")) {
      return;
    }
    setDeletingUserId(id);
    try {
      await deleteMutation.mutateAsync(id);
    } catch (error) {
      alert(
        error instanceof Error
          ? error.message
          : "Error al eliminar el usuario"
      );
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
              <h1 className="text-3xl font-bold">Usuarios</h1>
              <p className="text-muted-foreground">
                Gestiona los usuarios del sistema
              </p>
            </div>
            <Button onClick={() => setIsCreateModalOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Nuevo Usuario
            </Button>
          </div>

          {isLoading && (
            <div className="flex justify-center py-12">
              <Loading size="lg" text="Cargando usuarios..." />
            </div>
          )}

          {error && (
            <ErrorMessage
              message={
                error instanceof Error
                  ? error.message
                  : "Error al cargar los usuarios"
              }
            />
          )}

          {usuarios && usuarios.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <UsersIcon className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground mb-4">
                  No hay usuarios registrados
                </p>
                <Button onClick={() => setIsCreateModalOpen(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  Crear Primer Usuario
                </Button>
              </CardContent>
            </Card>
          )}

          {usuarios && usuarios.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Lista de Usuarios</CardTitle>
                <CardDescription>
                  {usuarios.length} usuario(s) encontrado(s)
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">Usuario</th>
                        <th className="text-left p-2">Nombre</th>
                        <th className="text-left p-2">Email</th>
                        <th className="text-left p-2">Teléfono</th>
                        <th className="text-left p-2">Empresa</th>
                        <th className="text-left p-2">Estado</th>
                        <th className="text-right p-2">Acciones</th>
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
                              {usuario.isActive ? "Activo" : "Inactivo"}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleView(usuario.id)}
                                title="Ver detalle"
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setEditingUser(usuario)}
                                title="Editar"
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDelete(usuario.id)}
                                disabled={deletingUserId === usuario.id}
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

          {/* Modal Crear Usuario */}
          <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
            <DialogContent>
              <DialogClose onClose={() => setIsCreateModalOpen(false)} />
              <DialogHeader>
                <DialogTitle>Crear Nuevo Usuario</DialogTitle>
                <DialogDescription>
                  Completa el formulario para crear un nuevo usuario
                </DialogDescription>
              </DialogHeader>
              <UserForm
                onSubmit={handleCreate}
                onCancel={() => setIsCreateModalOpen(false)}
                isLoading={createMutation.isPending}
              />
            </DialogContent>
          </Dialog>

          {/* Modal Editar Usuario */}
          <Dialog
            open={!!editingUser}
            onOpenChange={(open) => !open && setEditingUser(null)}
          >
            <DialogContent>
              <DialogClose onClose={() => setEditingUser(null)} />
              <DialogHeader>
                <DialogTitle>Editar Usuario</DialogTitle>
                <DialogDescription>
                  Modifica la información del usuario
                </DialogDescription>
              </DialogHeader>
              {editingUser && (
                <UserForm
                  user={editingUser}
                  onSubmit={handleUpdate}
                  onCancel={() => setEditingUser(null)}
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


