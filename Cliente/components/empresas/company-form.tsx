"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";

interface CompanyFormProps {
  company?: Company;
  onSubmit: (data: CreateCompany | UpdateCompany) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function CompanyForm({
  company,
  onSubmit,
  onCancel,
  isLoading = false,
}: CompanyFormProps) {
  const isEditing = !!company;
  const [formData, setFormData] = useState<CreateCompany | UpdateCompany>({
    name: company?.name || "",
    taxId: company?.taxId || "",
    address: company?.address || "",
    phone: company?.phone || "",
    email: company?.email || "",
    postalCodeId: company?.postalCodeId,
    cityId: company?.cityId,
    stateId: company?.stateId,
    countryId: company?.countryId,
    ...(isEditing && { isActive: company.isActive }),
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = "El nombre es requerido";
    }
    if (!formData.address.trim()) {
      newErrors.address = "La dirección es requerida";
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
      await onSubmit(formData);
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : "Error al guardar la empresa"
      );
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {submitError && <ErrorMessage message={submitError} />}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="name">
            Nombre <span className="text-destructive">*</span>
          </Label>
          <Input
            id="name"
            value={formData.name}
            onChange={(e) =>
              setFormData({ ...formData, name: e.target.value })
            }
            disabled={isLoading}
            required
          />
          {errors.name && (
            <p className="text-sm text-destructive">{errors.name}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="taxId">CIF/NIF</Label>
          <Input
            id="taxId"
            value={formData.taxId || ""}
            onChange={(e) =>
              setFormData({ ...formData, taxId: e.target.value || undefined })
            }
            disabled={isLoading}
          />
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

        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="address">
            Dirección <span className="text-destructive">*</span>
          </Label>
          <Input
            id="address"
            value={formData.address}
            onChange={(e) =>
              setFormData({ ...formData, address: e.target.value })
            }
            disabled={isLoading}
            required
          />
          {errors.address && (
            <p className="text-sm text-destructive">{errors.address}</p>
          )}
        </div>

        {isEditing && (
          <div className="space-y-2 flex items-center gap-2">
            <input
              type="checkbox"
              id="isActive"
              checked={(formData as UpdateCompany).isActive}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  isActive: e.target.checked,
                } as UpdateCompany)
              }
              disabled={isLoading}
              className="h-4 w-4 rounded border-gray-300"
            />
            <Label htmlFor="isActive" className="cursor-pointer">
              Empresa activa
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

