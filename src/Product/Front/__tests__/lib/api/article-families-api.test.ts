import { articleFamiliesApi } from "@/lib/api/article-families-api";
import { apiClient } from "@/lib/api/client";

jest.mock("@/lib/api/client");

const mockApiClient = apiClient as jest.Mocked<typeof apiClient>;

describe("articleFamiliesApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  const validGuid = "f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f101";
  const companyId = "550e8400-e29b-41d4-a716-446655440000";
  const taxTypeId = "11111111-1111-1111-1111-111111111121";

  describe("getAll", () => {
    it("should fetch all article families", async () => {
      const mockFamilies = [
        {
          id: validGuid,
          companyId,
          code: "GEN",
          name: "General",
          description: "Familia general",
          taxTypeId,
          taxTypeName: "IVA 21%",
          taxTypeValue: 21,
          isActive: true,
          createdAt: "2024-01-01T00:00:00Z",
        },
      ];
      mockApiClient.get.mockResolvedValue(mockFamilies);

      const result = await articleFamiliesApi.getAll();

      expect(mockApiClient.get).toHaveBeenCalledWith("/api/article-families");
      expect(result).toEqual(mockFamilies);
    });
  });

  describe("getById", () => {
    it("should fetch article family by id", async () => {
      const mockFamily = {
        id: validGuid,
        companyId,
        code: "GEN",
        name: "General",
        taxTypeId,
        isActive: true,
        createdAt: "2024-01-01T00:00:00Z",
      };
      mockApiClient.get.mockResolvedValue(mockFamily);

      const result = await articleFamiliesApi.getById(validGuid);

      expect(mockApiClient.get).toHaveBeenCalledWith(
        `/api/article-families/${validGuid}`
      );
      expect(result).toEqual(mockFamily);
    });
  });

  describe("create", () => {
    it("should create article family and return id", async () => {
      const dto = {
        companyId,
        code: "MET",
        name: "Metales",
        description: "Metales y aleaciones",
        taxTypeId,
      };
      const created = { id: validGuid, ...dto, isActive: true, createdAt: "2024-01-01T00:00:00Z" };
      mockApiClient.post.mockResolvedValue(created);

      const result = await articleFamiliesApi.create(dto);

      expect(mockApiClient.post).toHaveBeenCalledWith("/api/article-families", dto);
      expect(result).toBe(validGuid);
    });
  });

  describe("update", () => {
    it("should update article family", async () => {
      const updateDto = {
        id: validGuid,
        code: "MET",
        name: "Metales",
        description: "Actualizado",
        taxTypeId,
      };
      mockApiClient.put.mockResolvedValue(undefined);

      await articleFamiliesApi.update(validGuid, updateDto);

      expect(mockApiClient.put).toHaveBeenCalledWith(
        `/api/article-families/${validGuid}`,
        updateDto
      );
    });
  });

  describe("delete", () => {
    it("should delete article family", async () => {
      mockApiClient.delete.mockResolvedValue(undefined);

      await articleFamiliesApi.delete(validGuid);

      expect(mockApiClient.delete).toHaveBeenCalledWith(
        `/api/article-families/${validGuid}`
      );
    });
  });
});
