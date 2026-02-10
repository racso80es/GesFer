import { z } from "zod";
import type { CreateUser, UpdateUser } from "@/lib/types/api";

/**
 * Esquema de validación para crear usuario
 * Sincronizado con las validaciones del Backend (UserConfiguration.cs)
 */
export const createUserSchema = z.object({
  companyId: z.string().uuid("El ID de company debe ser un UUID válido"),
  username: z
    .string()
    .min(1, "El nombre de usuario es obligatorio")
    .max(100, "El nombre de usuario no puede exceder 100 caracteres"),
  password: z
    .string()
    .min(6, "La contraseña debe tener al menos 6 caracteres")
    .max(100, "La contraseña no puede exceder 100 caracteres"),
  firstName: z
    .string()
    .min(1, "El nombre es obligatorio")
    .max(100, "El nombre no puede exceder 100 caracteres"),
  lastName: z
    .string()
    .min(1, "El apellido es obligatorio")
    .max(100, "El apellido no puede exceder 100 caracteres"),
  email: z
    .string()
    .email("Debe ser un email válido")
    .max(200, "El email no puede exceder 200 caracteres")
    .optional()
    .or(z.literal("")),
  phone: z
    .string()
    .max(50, "El teléfono no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  address: z
    .string()
    .max(500, "La dirección no puede exceder 500 caracteres")
    .optional()
    .or(z.literal("")),
  postalCodeId: z.string().uuid().optional().or(z.literal("")),
  cityId: z.string().uuid().optional().or(z.literal("")),
  stateId: z.string().uuid().optional().or(z.literal("")),
  countryId: z.string().uuid().optional().or(z.literal("")),
  languageId: z.string().uuid().optional().or(z.literal("")),
}) satisfies z.ZodType<CreateUser>;

/**
 * Esquema de validación para actualizar usuario
 * Sincronizado con las validaciones del Backend (UserConfiguration.cs)
 */
export const updateUserSchema = z.object({
  username: z
    .string()
    .min(1, "El nombre de usuario es obligatorio")
    .max(100, "El nombre de usuario no puede exceder 100 caracteres"),
  password: z
    .string()
    .min(6, "La contraseña debe tener al menos 6 caracteres")
    .max(100, "La contraseña no puede exceder 100 caracteres")
    .optional()
    .or(z.literal("")),
  firstName: z
    .string()
    .min(1, "El nombre es obligatorio")
    .max(100, "El nombre no puede exceder 100 caracteres"),
  lastName: z
    .string()
    .min(1, "El apellido es obligatorio")
    .max(100, "El apellido no puede exceder 100 caracteres"),
  email: z
    .string()
    .email("Debe ser un email válido")
    .max(200, "El email no puede exceder 200 caracteres")
    .optional()
    .or(z.literal("")),
  phone: z
    .string()
    .max(50, "El teléfono no puede exceder 50 caracteres")
    .optional()
    .or(z.literal("")),
  address: z
    .string()
    .max(500, "La dirección no puede exceder 500 caracteres")
    .optional()
    .or(z.literal("")),
  postalCodeId: z.string().uuid().optional().or(z.literal("")),
  cityId: z.string().uuid().optional().or(z.literal("")),
  stateId: z.string().uuid().optional().or(z.literal("")),
  countryId: z.string().uuid().optional().or(z.literal("")),
  languageId: z.string().uuid().optional().or(z.literal("")),
  isActive: z.boolean(),
}) satisfies z.ZodType<UpdateUser>;

export type CreateUserFormData = z.infer<typeof createUserSchema>;
export type UpdateUserFormData = z.infer<typeof updateUserSchema>;
