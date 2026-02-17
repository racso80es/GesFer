"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@shared/components/ui/button";
import { Input } from "@shared/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@shared/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@shared/components/ui/select";
import { ErrorMessage } from "@shared/components/ui/error-message";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { useTranslations } from "next-intl";
import { useEffect, useState } from "react";

// Language options mapping (Codes to GUIDs from seed-data.sql)
const LANGUAGE_OPTIONS = [
  { value: "10000000-0000-0000-0000-000000000001", label: "Español", code: "es" },
  { value: "10000000-0000-0000-0000-000000000002", label: "English", code: "en" },
  { value: "10000000-0000-0000-0000-000000000003", label: "Català", code: "ca" },
];

const getLanguageId = (value: string | undefined): string | undefined => {
  if (!value) return undefined;
  if (value.includes("-")) return value; // Already a GUID
  const option = LANGUAGE_OPTIONS.find((opt) => opt.code === value);
  return option?.value;
};

const formSchema = z.object({
  name: z.string().min(1, "nameRequired"),
  taxId: z.string().optional(),
  address: z.string().min(1, "addressRequired"),
  phone: z.string().optional(),
  email: z.union([z.string().email("emailInvalid"), z.literal("")]).optional(),
  postalCodeId: z.string().optional(),
  cityId: z.string().optional(),
  stateId: z.string().optional(),
  countryId: z.string().optional(),
  languageId: z.string().optional(),
  isActive: z.boolean().optional(),
});

type FormValues = z.infer<typeof formSchema>;

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
  const t = useTranslations("companies.form");
  const tCommon = useTranslations("common");
  const isEditing = !!company;
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Determine current locale for displaying language names
  const locale =
    typeof window !== "undefined"
      ? window.location.pathname.split("/")[1] || "es"
      : "es";

  const languageNames: Record<string, Record<string, string>> = {
    es: { es: "Español", en: "English", ca: "Català" },
    en: { es: "Spanish", en: "English", ca: "Catalan" },
    ca: { es: "Espanyol", en: "Anglès", ca: "Català" },
  };

  const currentLanguageNames = languageNames[locale] || languageNames.es;

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
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
      isActive: isEditing ? company?.isActive : true,
    },
  });

  const handleSubmit = async (values: FormValues) => {
    setSubmitError(null);
    try {
      const dataToSubmit: any = { ...values };

      // Handle languageId logic
      if (dataToSubmit.languageId) {
        dataToSubmit.languageId = getLanguageId(dataToSubmit.languageId);
      } else {
        delete dataToSubmit.languageId;
      }
      
      // Clean empty strings for optional fields
      if (!dataToSubmit.email) delete dataToSubmit.email;
      if (!dataToSubmit.phone) delete dataToSubmit.phone;
      if (!dataToSubmit.taxId) delete dataToSubmit.taxId;

      await onSubmit(dataToSubmit);
    } catch (error) {
      setSubmitError(
        error instanceof Error ? error.message : t("saveError")
      );
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        {submitError && <ErrorMessage message={submitError} />}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Name Field */}
            <div className="md:col-span-2">
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>
                        {t("name")} <span className="text-destructive">*</span>
                      </FormLabel>
                      <FormControl>
                        <Input
                            {...field}
                            disabled={isLoading}
                            data-test-id="company-form-name"
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
            </div>

            {/* Tax ID Field */}
            <FormField
              control={form.control}
              name="taxId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("taxId")}</FormLabel>
                  <FormControl>
                    <Input
                        {...field}
                        disabled={isLoading}
                        data-test-id="company-form-taxId"
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Phone Field */}
            <FormField
              control={form.control}
              name="phone"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("phone")}</FormLabel>
                  <FormControl>
                    <Input {...field} type="tel" disabled={isLoading} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Email Field */}
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("email")}</FormLabel>
                  <FormControl>
                    <Input
                        {...field}
                        type="email"
                        disabled={isLoading}
                        data-test-id="company-form-email"
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Address Field */}
            <div className="md:col-span-2">
                <FormField
                  control={form.control}
                  name="address"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>
                        {t("address")} <span className="text-destructive">*</span>
                      </FormLabel>
                      <FormControl>
                        <Input {...field} disabled={isLoading} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
            </div>

            {/* Language Select */}
            <FormField
              control={form.control}
              name="languageId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("language")}</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    defaultValue={field.value}
                    disabled={isLoading}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={t("selectLanguage")} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {LANGUAGE_OPTIONS.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                          {currentLanguageNames[option.code] || option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Is Active Checkbox (Only for editing) */}
            {isEditing && (
                <FormField
                  control={form.control}
                  name="isActive"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                      <FormControl>
                        <input
                          type="checkbox"
                          checked={field.value}
                          onChange={field.onChange}
                          disabled={isLoading}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </FormControl>
                      <div className="space-y-1 leading-none">
                        <FormLabel className="cursor-pointer">
                          {t("isActive")}
                        </FormLabel>
                      </div>
                    </FormItem>
                  )}
                />
            )}
        </div>

        <div className="flex justify-end gap-2 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={onCancel}
            disabled={isLoading}
          >
            {tCommon("cancel")}
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading ? t("saving") : isEditing ? t("update") : t("create")}
          </Button>
        </div>
      </form>
    </Form>
  );
}
