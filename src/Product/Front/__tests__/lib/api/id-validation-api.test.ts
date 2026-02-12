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
      let threw = false;
      try {
        await companiesApi.getById("invalid-id");
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de organización no es válido");
      }
      expect(threw).toBe(true);
      expect(apiClient.get).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de update", async () => {
      let threw = false;
      try {
        await companiesApi.update("11.1111-111111111111:1", { name: "Test", address: "Test" });
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de organización no es válido");
      }
      expect(threw).toBe(true);
      expect(apiClient.put).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de delete", async () => {
      let threw = false;
      try {
        await companiesApi.delete("");
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de organización es requerido");
      }
      expect(threw).toBe(true);
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
      let threw = false;
      try {
        await usersApi.getById("invalid-id");
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de usuario no es válido");
      }
      expect(threw).toBe(true);
      expect(apiClient.get).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de update", async () => {
      let threw = false;
      try {
        await usersApi.update("11.1111-111111111111:1", {
          username: "test",
          firstName: "Test",
          lastName: "User",
        });
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de usuario no es válido");
      }
      expect(threw).toBe(true);
      expect(apiClient.put).not.toHaveBeenCalled();
    });

    it("debe validar ID antes de delete", async () => {
      let threw = false;
      try {
        await usersApi.delete("");
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de usuario es requerido");
      }
      expect(threw).toBe(true);
      expect(apiClient.delete).not.toHaveBeenCalled();
    });

    it("debe validar companyId en getAll", async () => {
      let threw = false;
      try {
        await usersApi.getAll("invalid-company-id");
      } catch (e: unknown) {
        threw = true;
        expect((e as Error).message).toContain("El ID de company no es válido");
      }
      expect(threw).toBe(true);
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

