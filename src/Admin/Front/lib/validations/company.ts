import { z } from "zod";
import type { CreateCompany, UpdateCompany } from "@/lib/types/api";

/**
 * Esquema de validación para crear organización
 * Sincronizado con las validaciones del Backend (CompanyConfiguration.cs)
 */
export const createCompanySchema = z.object({
  name: z
    .string()
    .min(1, "El nombre de la organización es obligatorio")
    .max(200, "El nombre no puede exceder 200 caracteres"),
  taxId: z
    .string()
    .max(50, "El CIF/NIF no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  address: z
    .string()
    .min(1, "La dirección es obligatoria")
    .max(500, "La dirección no puede exceder 500 caracteres"),
  phone: z
    .string()
    .max(50, "El teléfono no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  email: z
    .string()
    .email("Debe ser un email válido")
    .max(200, "El email no puede exceder 200 caracteres")
    .optional()
    .or(z.literal("")),
  postalCodeId: z.string().uuid().optional().or(z.literal("")),
  cityId: z.string().uuid().optional().or(z.literal("")),
  stateId: z.string().uuid().optional().or(z.literal("")),
  countryId: z.string().uuid().optional().or(z.literal("")),
  languageId: z.string().uuid().optional().or(z.literal("")),
});

/**
 * Esquema de validación para actualizar organización
 * Sincronizado con las validaciones del Backend (CompanyConfiguration.cs)
 */
export const updateCompanySchema = z.object({
  name: z
    .string()
    .min(1, "El nombre de la organización es obligatorio")
    .max(200, "El nombre no puede exceder 200 caracteres"),
  taxId: z
    .string()
    .max(50, "El CIF/NIF no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  address: z
    .string()
    .min(1, "La dirección es obligatoria")
    .max(500, "La dirección no puede exceder 500 caracteres"),
  phone: z
    .string()
    .max(50, "El teléfono no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  email: z
    .string()
    .email("Debe ser un email válido")
    .max(200, "El email no puede exceder 200 caracteres")
    .optional()
    .or(z.literal("")),
  postalCodeId: z.string().uuid().optional().or(z.literal("")),
  cityId: z.string().uuid().optional().or(z.literal("")),
  stateId: z.string().uuid().optional().or(z.literal("")),
  countryId: z.string().uuid().optional().or(z.literal("")),
  languageId: z.string().uuid().optional().or(z.literal("")),
  isActive: z.boolean(),
});

export type CreateCompanyFormData = z.infer<typeof createCompanySchema>;
export type UpdateCompanyFormData = z.infer<typeof updateCompanySchema>;
