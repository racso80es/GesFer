/**
 * Tests unitarios para validación de IDs
 */

import { isValidGuid, sanitizeId, validateId } from "@/lib/utils/id-validation";

describe("id-validation", () => {
  describe("isValidGuid", () => {
    it("debe validar GUIDs correctos", () => {
      const validGuids = [
        "11111111-1111-1111-1111-111111111111",
        "00000000-0000-0000-0000-000000000000",
        "ffffffff-ffff-ffff-ffff-ffffffffffff",
        "12345678-1234-1234-1234-123456789abc",
        "ABCDEF12-3456-7890-ABCD-EF1234567890",
      ];

      validGuids.forEach((guid) => {
        expect(isValidGuid(guid)).toBe(true);
      });
    });

    it("debe rechazar GUIDs incorrectos", () => {
      const invalidGuids = [
        "11.1111-111111111111:1", // Con puntos y dos puntos
        "invalid-id", // Sin formato GUID
        "123", // Muy corto
        "11111111-1111-1111-1111-111111111111:extra", // Con caracteres extra
        "", // Vacío
        "11111111-1111-1111-1111", // Incompleto
        "11111111111111111111111111111111", // Sin guiones
        null,
        undefined,
      ];

      invalidGuids.forEach((guid) => {
        expect(isValidGuid(guid as string)).toBe(false);
      });
    });
  });

  describe("sanitizeId", () => {
    it("debe sanitizar IDs válidos correctamente", () => {
      expect(sanitizeId("11111111-1111-1111-1111-111111111111")).toBe(
        "11111111-1111-1111-1111-111111111111"
      );
      expect(sanitizeId("  11111111-1111-1111-1111-111111111111  ")).toBe(
        "11111111-1111-1111-1111-111111111111"
      );
    });

    it("debe retornar null para IDs inválidos", () => {
      expect(sanitizeId("11.1111-111111111111:1")).toBe(null);
      expect(sanitizeId("invalid-id")).toBe(null);
      expect(sanitizeId("")).toBe(null);
      expect(sanitizeId(null)).toBe(null);
      expect(sanitizeId(undefined)).toBe(null);
    });
  });

  describe("validateId", () => {
    it("debe validar y retornar IDs válidos", () => {
      const validId = "11111111-1111-1111-1111-111111111111";
      expect(validateId(validId, "empresa")).toBe(validId);
      expect(validateId("  " + validId + "  ", "usuario")).toBe(validId);
    });

    it("debe lanzar error para IDs inválidos", () => {
      expect(() => validateId("11.1111-111111111111:1", "empresa")).toThrow(
        "El ID de empresa no es válido"
      );
      expect(() => validateId("invalid-id", "usuario")).toThrow(
        "El ID de usuario no es válido"
      );
      expect(() => validateId("", "empresa")).toThrow("El ID de empresa es requerido");
      expect(() => validateId(null, "usuario")).toThrow("El ID de usuario es requerido");
      expect(() => validateId(undefined, "empresa")).toThrow("El ID de empresa es requerido");
    });
  });
});

