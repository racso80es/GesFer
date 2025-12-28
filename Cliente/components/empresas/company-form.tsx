"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { useTranslations } from 'next-intl';

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
  const t = useTranslations('companies.form');
  const tCommon = useTranslations('common');
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
    languageId: company?.languageId || undefined,
    ...(isEditing && { isActive: company.isActive }),
  });

  // Opciones de idioma disponibles (mapeo de códigos a Guids según seed-data.sql)
  // Estos son los IDs fijos de los idiomas en la base de datos
  const languageOptions = [
    { value: "10000000-0000-0000-0000-000000000001", label: "Español", code: "es" },
    { value: "10000000-0000-0000-0000-000000000002", label: "English", code: "en" },
    { value: "10000000-0000-0000-0000-000000000003", label: "Català", code: "ca" },
  ];
  
  // Función para obtener el Guid del idioma desde el código o mantener el Guid si ya lo es
  const getLanguageId = (value: string | undefined): string | undefined => {
    if (!value) return undefined;
    // Si ya es un Guid (contiene guiones), devolverlo tal cual
    if (value.includes("-")) return value;
    // Si es un código, buscar el Guid correspondiente
    const option = languageOptions.find(opt => opt.code === value);
    return option?.value || undefined;
  };
  
  // Obtener el locale actual para mostrar los nombres de idioma en el idioma correcto
  const locale = typeof window !== 'undefined' 
    ? window.location.pathname.split('/')[1] || 'es'
    : 'es';
  
  // Mapeo de nombres de idioma según el locale
  const languageNames: Record<string, Record<string, string>> = {
    es: { es: "Español", en: "English", ca: "Català" },
    en: { es: "Spanish", en: "English", ca: "Catalan" },
    ca: { es: "Espanyol", en: "Anglès", ca: "Català" },
  };
  
  const currentLanguageNames = languageNames[locale] || languageNames.es;

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = t('nameRequired');
    }
    if (!formData.address.trim()) {
      newErrors.address = t('addressRequired');
    }
    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = t('emailInvalid');
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
      // Preparar los datos para enviar
      const dataToSubmit = { ...formData };
      
      // Asegurar que languageId sea null explícitamente si está vacío (para establecer "por defecto")
      // o convertir código a Guid si tiene un valor
      if (dataToSubmit.languageId && dataToSubmit.languageId !== "") {
        dataToSubmit.languageId = getLanguageId(dataToSubmit.languageId);
      } else {
        // Enviar null explícitamente para que el backend lo establezca como null
        // null se serializa en JSON, undefined no, así que usamos null para indicar "por defecto"
        (dataToSubmit as any).languageId = null;
      }
      
      await onSubmit(dataToSubmit);
    } catch (error) {
      setSubmitError(
        error instanceof Error
          ? error.message
          : t('saveError')
      );
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {submitError && <ErrorMessage message={submitError} />}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="name">
            {t('name')} <span className="text-destructive">*</span>
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
          <Label htmlFor="taxId">{t('taxId')}</Label>
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
          <Label htmlFor="phone">{t('phone')}</Label>
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
          <Label htmlFor="email">{t('email')}</Label>
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
            {t('address')} <span className="text-destructive">*</span>
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

        <div className="space-y-2">
          <Label htmlFor="languageId">{t('language')}</Label>
          <select
            id="languageId"
            value={formData.languageId || ""}
            onChange={(e) => {
              const selectedValue = e.target.value;
              // Si se selecciona la opción vacía, establecer undefined
              if (!selectedValue || selectedValue === "") {
                setFormData({ ...formData, languageId: undefined });
              } else {
                const languageId = getLanguageId(selectedValue);
                setFormData({ ...formData, languageId });
              }
            }}
            disabled={isLoading}
            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <option value="">{t('defaultLanguage')}</option>
            {languageOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {currentLanguageNames[option.code] || option.label}
              </option>
            ))}
          </select>
          {errors.languageId && (
            <p className="text-sm text-destructive">{errors.languageId}</p>
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
              {t('isActive')}
            </Label>
          </div>
        )}
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
          {tCommon('cancel')}
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? (
            t('saving')
          ) : (
            isEditing ? t('update') : t('create')
          )}
        </Button>
      </div>
    </form>
  );
}

