/**
 * Tests para verificar que las funciones de API validan IDs correctamente
 */

import { companiesApi } from "@/lib/api/companies";
import { usersApi } from "@/lib/api/users";

// Mock del apiClient
jest.mock("@/lib/api/client", () => ({
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

import { apiClient } from "@/lib/api/client";

describe("Validación de IDs en APIs", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("companiesApi", () => {
    it("debe validar ID antes de getById", async () => {
      await expect(companiesApi.getById("invalid-id")).rejects.toThrow(
        "El ID de empresa no es válido"
      );
      expect(apiClient.get).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de update", async () => {
      await expect(
        companiesApi.update("11.1111-111111111111:1", { name: "Test", address: "Test" })
      ).rejects.toThrow("El ID de empresa no es válido");
      expect(apiClient.put).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de delete", async () => {
      await expect(companiesApi.delete("")).rejects.toThrow("El ID de empresa es requerido");
      expect(apiClient.delete).not.toHaveBeenCalled();
    });

    it("debe permitir operaciones con IDs válidos", async () => {
      const validId = "11111111-1111-1111-1111-111111111111";
      (apiClient.get as jest.Mock).mockResolvedValue({ id: validId, name: "Test" });
      (apiClient.put as jest.Mock).mockResolvedValue({ id: validId, name: "Updated" });
      (apiClient.delete as jest.Mock).mockResolvedValue(undefined);

      await companiesApi.getById(validId);
      expect(apiClient.get).toHaveBeenCalledWith(`/api/company/${validId}`);

      await companiesApi.update(validId, { name: "Updated", address: "Test" });
      expect(apiClient.put).toHaveBeenCalledWith(`/api/company/${validId}`, {
        name: "Updated",
        address: "Test",
      });

      await companiesApi.delete(validId);
      expect(apiClient.delete).toHaveBeenCalledWith(`/api/company/${validId}`);
    });
  });

  describe("usersApi", () => {
    it("debe validar ID antes de getById", async () => {
      await expect(usersApi.getById("invalid-id")).rejects.toThrow(
        "El ID de usuario no es válido"
      );
      expect(apiClient.get).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de update", async () => {
      await expect(
        usersApi.update("11.1111-111111111111:1", {
          username: "test",
          firstName: "Test",
          lastName: "User",
        })
      ).rejects.toThrow("El ID de usuario no es válido");
      expect(apiClient.put).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de delete", async () => {
      await expect(usersApi.delete("")).rejects.toThrow("El ID de usuario es requerido");
      expect(apiClient.delete).not.toHaveBeenCalled();
    });

    it("debe validar companyId en getAll", async () => {
      await expect(usersApi.getAll("invalid-company-id")).rejects.toThrow(
        "El ID de empresa no es válido"
      );
      expect(apiClient.get).not.toHaveBeenCalled();
    });

    it("debe permitir operaciones con IDs válidos", async () => {
      const validId = "11111111-1111-1111-1111-111111111111";
      (apiClient.get as jest.Mock).mockResolvedValue([{ id: validId, username: "test" }]);
      (apiClient.put as jest.Mock).mockResolvedValue({ id: validId, username: "updated" });
      (apiClient.delete as jest.Mock).mockResolvedValue(undefined);

      await usersApi.getById(validId);
      expect(apiClient.get).toHaveBeenCalledWith(`/api/user/${validId}`);

      await usersApi.update(validId, { username: "updated", firstName: "Test", lastName: "User" });
      expect(apiClient.put).toHaveBeenCalledWith(`/api/user/${validId}`, {
        username: "updated",
        firstName: "Test",
        lastName: "User",
      });

      await usersApi.delete(validId);
      expect(apiClient.delete).toHaveBeenCalledWith(`/api/user/${validId}`);
    });
  });
});

