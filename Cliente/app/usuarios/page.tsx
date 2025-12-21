"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { useQuery } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";
import { useAuth } from "@/contexts/auth-context";
import { Plus, Edit, Trash2, Users as UsersIcon } from "lucide-react";
import { useState } from "react";

export default function UsuariosPage() {
  const { user } = useAuth();
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

  const {
    data: usuarios,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["users", user?.companyId],
    queryFn: () => usersApi.getAll(user?.companyId),
    enabled: !!user?.companyId,
  });

  const handleDelete = async (id: string) => {
    if (!confirm("¿Estás seguro de que deseas eliminar este usuario?")) {
      return;
    }

    try {
      await usersApi.delete(id);
      refetch();
    } catch (error) {
      alert(
        error instanceof Error
          ? error.message
          : "Error al eliminar el usuario"
      );
    }
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
            <Button>
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
                <p className="text-muted-foreground">
                  No hay usuarios registrados
                </p>
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
                        <th className="text-left p-2">Estado</th>
                        <th className="text-right p-2">Acciones</th>
                      </tr>
                    </thead>
                    <tbody>
                      {usuarios.map((usuario) => (
                        <tr key={usuario.id} className="border-b hover:bg-muted/50">
                          <td className="p-2">{usuario.username}</td>
                          <td className="p-2">
                            {usuario.firstName} {usuario.lastName}
                          </td>
                          <td className="p-2">{usuario.email || "-"}</td>
                          <td className="p-2">{usuario.phone || "-"}</td>
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
                                onClick={() => setSelectedUserId(usuario.id)}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDelete(usuario.id)}
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
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}

