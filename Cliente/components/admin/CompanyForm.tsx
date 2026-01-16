"use client";

import { useState } from "react";
import { Button } from "@/components/shared/Button";
import { Input } from "@/components/shared/Input";
import { Label } from "@/components/ui/label";
import { ErrorMessage } from "@/components/ui/error-message";
import type { Company } from "@/lib/types/api";
import {
  createCompanySchema,
  updateCompanySchema,
  type CreateCompanyFormData,
  type UpdateCompanyFormData,
} from "@/lib/validations/company";
import { useTranslations } from "next-intl";

interface CompanyFormProps {
  company?: Company;
  onSubmit: (data: CreateCompanyFormData | UpdateCompanyFormData) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function CompanyForm({
  company,
  onSubmit,
  onCancel,
  isLoading = false,
}: CompanyFormProps) {
  const t = useTranslations("companies.form");
  const tCommon = useTranslations("common");
  const isEditing = !!company;

  const schema = isEditing ? updateCompanySchema : createCompanySchema;

  const [formData, setFormData] = useState<CreateCompanyFormData | UpdateCompanyFormData>(
    isEditing
      ? {
          name: company?.name || "",
          taxId: company?.taxId || "",
          address: company?.address || "",
          phone: company?.phone || "",
          email: company?.email || "",
          postalCodeId: company?.postalCodeId || "",
          cityId: company?.cityId || "",
          stateId: company?.stateId || "",
          countryId: company?.countryId || "",
          languageId: company?.languageId || "",
          isActive: company?.isActive ?? true,
        }
      : {
          name: "",
          taxId: "",
          address: "",
          phone: "",
          email: "",
          postalCodeId: "",
          cityId: "",
          stateId: "",
          countryId: "",
          languageId: "",
        }
  );

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      ) as CreateCompanyFormData | UpdateCompanyFormData;

      await onSubmit(cleanedData);
    } catch (error) {
      console.error("Error submitting form:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={onSubmitForm} className="space-y-4" data-testid="shared-form-company">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="name" data-testid="shared-label-company-name">
            {t("name")} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="name"
            type="text"
            value={formData.name}
            onChange={(e) => handleChange("name", e.target.value)}
            data-testid="shared-input-text-company-name"
            aria-invalid={errors.name ? "true" : "false"}
          />
          {errors.name && (
            <ErrorMessage message={errors.name} data-testid="shared-error-company-name" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="taxId" data-testid="shared-label-company-taxid">
            {t("taxId")}
          </Label>
          <Input
            id="taxId"
            type="text"
            value={formData.taxId || ""}
            onChange={(e) => handleChange("taxId", e.target.value)}
            data-testid="shared-input-text-company-taxid"
            aria-invalid={errors.taxId ? "true" : "false"}
          />
          {errors.taxId && (
            <ErrorMessage message={errors.taxId} data-testid="shared-error-company-taxid" />
          )}
        </div>

        <div className="space-y-2 md:col-span-2">
          <Label htmlFor="address" data-testid="shared-label-company-address">
            {t("address")} <span className="text-destructive">*</span>
          </Label>
          <Input
            id="address"
            type="text"
            value={formData.address}
            onChange={(e) => handleChange("address", e.target.value)}
            data-testid="shared-input-text-company-address"
            aria-invalid={errors.address ? "true" : "false"}
          />
          {errors.address && (
            <ErrorMessage message={errors.address} data-testid="shared-error-company-address" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="phone" data-testid="shared-label-company-phone">
            {t("phone")}
          </Label>
          <Input
            id="phone"
            type="tel"
            value={formData.phone || ""}
            onChange={(e) => handleChange("phone", e.target.value)}
            data-testid="shared-input-tel-company-phone"
            aria-invalid={errors.phone ? "true" : "false"}
          />
          {errors.phone && (
            <ErrorMessage message={errors.phone} data-testid="shared-error-company-phone" />
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="email" data-testid="shared-label-company-email">
            {t("email")}
          </Label>
          <Input
            id="email"
            type="email"
            value={formData.email || ""}
            onChange={(e) => handleChange("email", e.target.value)}
            data-testid="shared-input-email-company-email"
            aria-invalid={errors.email ? "true" : "false"}
          />
          {errors.email && (
            <ErrorMessage message={errors.email} data-testid="shared-error-company-email" />
          )}
        </div>

        {isEditing && (
          <div className="space-y-2">
            <Label htmlFor="isActive" data-testid="shared-label-company-isactive" className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={(formData as UpdateCompanyFormData).isActive}
                onChange={(e) => handleChange("isActive", e.target.checked)}
                data-testid="shared-input-checkbox-company-isactive"
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
          data-testid="shared-button-company-form-cancel"
        >
          {tCommon("cancel")}
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting || isLoading}
          data-testid="shared-button-company-form-submit"
        >
          {isSubmitting || isLoading ? tCommon("saving") : tCommon("save")}
        </Button>
      </div>
    </form>
  );
}
