"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";
import { useAuth } from "@/contexts/auth-context";

interface UserFormProps {
  user?: User;
  onSubmit: (data: CreateUser | UpdateUser) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function UserForm({
  user,
  onSubmit,
  onCancel,
  isLoading = false,
}: UserFormProps) {
  const { user: loggedUser } = useAuth();
  const isEditing = !!user;
  
  // La empresa siempre es la del usuario logueado
  const companyId = loggedUser?.companyId || "";
  
  const [formData, setFormData] = useState<CreateUser | UpdateUser>({
    companyId: companyId,
    username: user?.username || "",
    password: "",
    firstName: user?.firstName || "",
    lastName: user?.lastName || "",
    email: user?.email || "",
    phone: user?.phone || "",
    address: user?.address || "",
    postalCodeId: user?.postalCodeId,
    cityId: user?.cityId,
    stateId: user?.stateId,
    countryId: user?.countryId,
    ...(isEditing && { isActive: user.isActive }),
  });

  // Asegurar que companyId siempre sea el del usuario logueado
  useEffect(() => {
    if (loggedUser?.companyId) {
      setFormData((prev) => ({ ...prev, companyId: loggedUser.companyId }));
    }
  }, [loggedUser?.companyId]);

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.companyId) {
      newErrors.companyId = "La empresa es requerida";
    }
    if (!formData.username.trim()) {
      newErrors.username = "El nombre de usuario es requerido";
    }
    if (!isEditing && !formData.password) {
      newErrors.password = "La contraseña es requerida";
    }
    if (!formData.firstName.trim()) {
      newErrors.firstName = "El nombre es requerido";
    }
    if (!formData.lastName.trim()) {
      newErrors.lastName = "El apellido es requerido";
    }
    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = "El email no es válido";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (!validate()) {
      return;
    }

    try {
      // Si es edición y no hay password, no lo incluimos
      if (isEditing && !formData.password) {
        const { password, ...updateData } = formData as UpdateUser;
        await onSubmit(updateData);
      } else {
        await onSubmit(formData);
      }
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : "Error al guardar el usuario"
      );
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {submitError && <ErrorMessage message={submitError} />}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="companyId">
            Empresa <span className="text-destructive">*</span>
          </Label>
          <Input
            id="companyId"
            value={loggedUser?.companyName || user?.companyName || "Empresa del usuario logueado"}
            disabled
            className="bg-muted cursor-not-allowed"
            readOnly
          />
          {errors.companyId && (
            <p className="text-sm text-destructive">{errors.companyId}</p>
          )}
          <p className="text-xs text-muted-foreground">
            La empresa corresponde a la del usuario logueado y no puede ser modificada.
          </p>
        </div>

        <div className="space-y-2">
          <Label htmlFor="username">
            Nombre de Usuario <span className="text-destructive">*</span>
          </Label>
          <Input
            id="username"
            value={formData.username}
            onChange={(e) =>
              setFormData({ ...formData, username: e.target.value })
            }
            disabled={isLoading}
            required
          />
          {errors.username && (
            <p className="text-sm text-destructive">{errors.username}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="password">
            Contraseña {!isEditing && <span className="text-destructive">*</span>}
          </Label>
          <Input
            id="password"
            type="password"
            value={formData.password || ""}
            onChange={(e) =>
              setFormData({ ...formData, password: e.target.value })
            }
            disabled={isLoading}
            placeholder={isEditing ? "Dejar vacío para no cambiar" : ""}
            required={!isEditing}
          />
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="firstName">
            Nombre <span className="text-destructive">*</span>
          </Label>
          <Input
            id="firstName"
            value={formData.firstName}
            onChange={(e) =>
              setFormData({ ...formData, firstName: e.target.value })
            }
            disabled={isLoading}
            required
          />
          {errors.firstName && (
            <p className="text-sm text-destructive">{errors.firstName}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="lastName">
            Apellido <span className="text-destructive">*</span>
          </Label>
          <Input
            id="lastName"
            value={formData.lastName}
            onChange={(e) =>
              setFormData({ ...formData, lastName: e.target.value })
            }
            disabled={isLoading}
            required
          />
          {errors.lastName && (
            <p className="text-sm text-destructive">{errors.lastName}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            value={formData.email || ""}
            onChange={(e) =>
              setFormData({ ...formData, email: e.target.value || undefined })
            }
            disabled={isLoading}
          />
          {errors.email && (
            <p className="text-sm text-destructive">{errors.email}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="phone">Teléfono</Label>
          <Input
            id="phone"
            type="tel"
            value={formData.phone || ""}
            onChange={(e) =>
              setFormData({ ...formData, phone: e.target.value || undefined })
            }
            disabled={isLoading}
          />
        </div>

        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="address">Dirección</Label>
          <Input
            id="address"
            value={formData.address || ""}
            onChange={(e) =>
              setFormData({ ...formData, address: e.target.value || undefined })
            }
            disabled={isLoading}
          />
        </div>

        {isEditing && (
          <div className="space-y-2 flex items-center gap-2">
            <input
              type="checkbox"
              id="isActive"
              checked={(formData as UpdateUser).isActive}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  isActive: e.target.checked,
                } as UpdateUser)
              }
              disabled={isLoading}
              className="h-4 w-4 rounded border-gray-300"
            />
            <Label htmlFor="isActive" className="cursor-pointer">
              Usuario activo
            </Label>
          </div>
        )}
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
          Cancelar
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? (
            "Guardando..."
          ) : (
            isEditing ? "Actualizar" : "Crear"
          )}
        </Button>
      </div>
    </form>
  );
}

