"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";
import { useAuth } from "@/contexts/auth-context";
import { useTranslations } from 'next-intl';

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
  const t = useTranslations('users.form');
  const tCommon = useTranslations('common');
  const isEditing = !!user;
  
  // La empresa siempre es la del usuario logueado
  const companyId = loggedUser?.companyId || "";
  
  const [formData, setFormData] = useState<CreateUser | UpdateUser>(
    isEditing
      ? ({
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
          languageId: user?.languageId || undefined,
          isActive: user.isActive,
        } as UpdateUser)
      : ({
          companyId: companyId,
          username: "",
          password: "",
          firstName: "",
          lastName: "",
          email: "",
          phone: "",
          address: "",
          languageId: undefined,
        } as CreateUser)
  );

  // Opciones de idioma disponibles (mapeo de códigos a Guids según seed-data.sql)
  // Estos son los IDs fijos de los idiomas en la base de datos
  const languageOptions = [
    { value: "10000000-0000-0000-0000-000000000001", label: "Español", code: "es" },
    { value: "10000000-0000-0000-0000-000000000002", label: "English", code: "en" },
    { value: "10000000-0000-0000-0000-000000000003", label: "Català", code: "ca" },
  ];
  
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
  
  // Función para obtener el Guid del idioma desde el código o mantener el Guid si ya lo es
  const getLanguageId = (value: string | undefined): string | undefined => {
    if (!value) return undefined;
    // Si ya es un Guid (contiene guiones), devolverlo tal cual
    if (value.includes("-")) return value;
    // Si es un código, buscar el Guid correspondiente
    const option = languageOptions.find(opt => opt.code === value);
    return option?.value || undefined;
  };

  // Asegurar que companyId siempre sea el del usuario logueado (solo para creación)
  useEffect(() => {
    if (!isEditing && loggedUser?.companyId) {
      setFormData((prev) => ({ ...prev, companyId: loggedUser.companyId } as CreateUser));
    }
  }, [loggedUser?.companyId, isEditing]);

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    // Solo validar companyId si es creación (CreateUser)
    if (!isEditing && !(formData as CreateUser).companyId) {
      newErrors.companyId = t('companyRequired');
    }
    if (!formData.username.trim()) {
      newErrors.username = t('usernameRequired');
    }
    if (!isEditing && !formData.password) {
      newErrors.password = t('passwordRequired');
    }
    if (!formData.firstName.trim()) {
      newErrors.firstName = t('firstNameRequired');
    }
    if (!formData.lastName.trim()) {
      newErrors.lastName = t('lastNameRequired');
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
      
      // Asegurar que languageId sea undefined si está vacío o convertir código a Guid
      if (dataToSubmit.languageId && dataToSubmit.languageId !== "") {
        dataToSubmit.languageId = getLanguageId(dataToSubmit.languageId);
      } else {
        dataToSubmit.languageId = undefined;
      }
      
      // Si es edición y no hay password, no lo incluimos
      if (isEditing && !dataToSubmit.password) {
        const { password, ...updateData } = dataToSubmit as UpdateUser;
        await onSubmit(updateData);
      } else {
        await onSubmit(dataToSubmit);
      }
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
        <div className="space-y-2">
          <Label htmlFor="companyId">
            {t('company')} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="companyId"
            value={loggedUser?.companyName || user?.companyName || t('companyPlaceholder')}
            disabled
            className="bg-muted cursor-not-allowed"
            readOnly
          />
          {errors.companyId && (
            <p className="text-sm text-destructive">{errors.companyId}</p>
          )}
          <p className="text-xs text-muted-foreground">
            {t('companyDescription')}
          </p>
        </div>

        <div className="space-y-2">
          <Label htmlFor="username">
            {t('username')} <span className="text-destructive">*</span>
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
            {t('password')} {!isEditing && <span className="text-destructive">*</span>}
          </Label>
          <Input
            id="password"
            type="password"
            value={formData.password || ""}
            onChange={(e) =>
              setFormData({ ...formData, password: e.target.value })
            }
            disabled={isLoading}
            placeholder={isEditing ? t('passwordPlaceholder') : ""}
            required={!isEditing}
          />
          {errors.password && (
            <p className="text-sm text-destructive">{errors.password}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="firstName">
            {t('firstName')} <span className="text-destructive">*</span>
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
            {t('lastName')} <span className="text-destructive">*</span>
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

        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="address">{t('address')}</Label>
          <Input
            id="address"
            value={formData.address || ""}
            onChange={(e) =>
              setFormData({ ...formData, address: e.target.value || undefined })
            }
            disabled={isLoading}
          />
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

