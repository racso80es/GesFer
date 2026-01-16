"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/shared/Button";
import { Input } from "@/components/shared/Input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { User } from "@/lib/types/api";
import {
  createUserSchema,
  updateUserSchema,
  type CreateUserFormData,
  type UpdateUserFormData,
} from "@/lib/validations/user";
import { useTranslations } from "next-intl";

interface UserFormProps {
  user?: User;
  companyId: string;
  onSubmit: (data: CreateUserFormData | UpdateUserFormData) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function UserForm({
  user,
  companyId,
  onSubmit,
  onCancel,
  isLoading = false,
}: UserFormProps) {
  const t = useTranslations("users.form");
  const tCommon = useTranslations("common");
  const isEditing = !!user;

  const schema = isEditing ? updateUserSchema : createUserSchema;

  const [formData, setFormData] = useState<CreateUserFormData | UpdateUserFormData>(
    isEditing
      ? {
          username: user?.username || "",
          password: "",
          firstName: user?.firstName || "",
          lastName: user?.lastName || "",
          email: user?.email || "",
          phone: user?.phone || "",
          address: user?.address || "",
          postalCodeId: user?.postalCodeId || "",
          cityId: user?.cityId || "",
          stateId: user?.stateId || "",
          countryId: user?.countryId || "",
          languageId: user?.languageId || "",
          isActive: user?.isActive ?? true,
        }
      : {
          companyId: companyId,
          username: "",
          password: "",
          firstName: "",
          lastName: "",
          email: "",
          phone: "",
          address: "",
          postalCodeId: "",
          cityId: "",
          stateId: "",
          countryId: "",
          languageId: "",
        }
  );

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Asegurar que companyId siempre sea el correcto en creación
  useEffect(() => {
    if (!isEditing) {
      setFormData((prev) => ({ ...prev, companyId: companyId } as CreateUserFormData));
    }
  }, [companyId, isEditing]);

  const handleChange = (field: string, value: string | boolean) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Limpiar error del campo cuando el usuario empieza a escribir
    if (errors[field]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const onSubmitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setIsSubmitting(true);

    try {
      // Validar con Zod
      const result = schema.safeParse(formData);
      if (!result.success) {
        const newErrors: Record<string, string> = {};
        result.error.issues.forEach((err) => {
          if (err.path[0]) {
            newErrors[err.path[0].toString()] = err.message;
          }
        });
        setErrors(newErrors);
        setIsSubmitting(false);
        return;
      }

      // Limpiar campos opcionales vacíos
      const cleanedData = Object.fromEntries(
        Object.entries(result.data).filter(([_, value]) => value !== "")
      ) as CreateUserFormData | UpdateUserFormData;

      await onSubmit(cleanedData);
    } catch (error) {
      console.error("Error submitting form:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={onSubmitForm} className="space-y-4" data-testid="shared-form-user">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="username" data-testid="shared-label-username">
            {t("username")} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="username"
            type="text"
            value={formData.username}
            onChange={(e) => handleChange("username", e.target.value)}
            data-testid="shared-input-text-username"
            aria-invalid={errors.username ? "true" : "false"}
          />
          {errors.username && (
            <ErrorMessage message={errors.username} data-testid="shared-error-username" />
          )}
        </div>

        {!isEditing && (
          <div className="space-y-2">
            <Label htmlFor="password" data-testid="shared-label-password">
              {t("password")} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="password"
              type="password"
              value={(formData as CreateUserFormData).password || ""}
              onChange={(e) => handleChange("password", e.target.value)}
              data-testid="shared-input-password-password"
              aria-invalid={errors.password ? "true" : "false"}
            />
            {errors.password && (
              <ErrorMessage message={errors.password} data-testid="shared-error-password" />
            )}
          </div>
        )}

        {isEditing && (
          <div className="space-y-2">
            <Label htmlFor="password" data-testid="shared-label-password-optional">
              {t("password")} ({tCommon("optional")})
            </Label>
            <Input
              id="password"
              type="password"
              value={(formData as UpdateUserFormData).password || ""}
              onChange={(e) => handleChange("password", e.target.value)}
              placeholder={t("passwordPlaceholder")}
              data-testid="shared-input-password-password-optional"
              aria-invalid={errors.password ? "true" : "false"}
            />
            {errors.password && (
              <ErrorMessage message={errors.password} data-testid="shared-error-password-optional" />
            )}
          </div>
        )}

        <div className="space-y-2">
          <Label htmlFor="firstName" data-testid="shared-label-firstname">
            {t("firstName")} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="firstName"
            type="text"
            value={formData.firstName}
            onChange={(e) => handleChange("firstName", e.target.value)}
            data-testid="shared-input-text-firstname"
            aria-invalid={errors.firstName ? "true" : "false"}
          />
          {errors.firstName && (
            <ErrorMessage message={errors.firstName} data-testid="shared-error-firstname" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="lastName" data-testid="shared-label-lastname">
            {t("lastName")} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="lastName"
            type="text"
            value={formData.lastName}
            onChange={(e) => handleChange("lastName", e.target.value)}
            data-testid="shared-input-text-lastname"
            aria-invalid={errors.lastName ? "true" : "false"}
          />
          {errors.lastName && (
            <ErrorMessage message={errors.lastName} data-testid="shared-error-lastname" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="email" data-testid="shared-label-email">
            {t("email")}
          </Label>
          <Input
            id="email"
            type="email"
            value={formData.email || ""}
            onChange={(e) => handleChange("email", e.target.value)}
            data-testid="shared-input-email-email"
            aria-invalid={errors.email ? "true" : "false"}
          />
          {errors.email && (
            <ErrorMessage message={errors.email} data-testid="shared-error-email" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="phone" data-testid="shared-label-phone">
            {t("phone")}
          </Label>
          <Input
            id="phone"
            type="tel"
            value={formData.phone || ""}
            onChange={(e) => handleChange("phone", e.target.value)}
            data-testid="shared-input-tel-phone"
            aria-invalid={errors.phone ? "true" : "false"}
          />
          {errors.phone && (
            <ErrorMessage message={errors.phone} data-testid="shared-error-phone" />
          )}
        </div>

        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="address" data-testid="shared-label-address">
            {t("address")}
          </Label>
          <Input
            id="address"
            type="text"
            value={formData.address || ""}
            onChange={(e) => handleChange("address", e.target.value)}
            data-testid="shared-input-text-address"
            aria-invalid={errors.address ? "true" : "false"}
          />
          {errors.address && (
            <ErrorMessage message={errors.address} data-testid="shared-error-address" />
          )}
        </div>

        {isEditing && (
          <div className="space-y-2">
            <Label htmlFor="isActive" data-testid="shared-label-isactive" className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={(formData as UpdateUserFormData).isActive}
                onChange={(e) => handleChange("isActive", e.target.checked)}
                data-testid="shared-input-checkbox-isactive"
                className="h-4 w-4"
              />
              {t("isActive")}
            </Label>
          </div>
        )}
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting || isLoading}
          data-testid="shared-button-user-form-cancel"
        >
          {tCommon("cancel")}
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting || isLoading}
          data-testid="shared-button-user-form-submit"
        >
          {isSubmitting || isLoading ? tCommon("saving") : tCommon("save")}
        </Button>
      </div>
    </form>
  );
}
