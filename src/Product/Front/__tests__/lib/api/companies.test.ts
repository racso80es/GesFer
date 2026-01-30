import { companiesApi } from "@/lib/api/companies";
import { apiClient } from "@/lib/api/client";

jest.mock("@/lib/api/client");

const mockApiClient = apiClient as jest.Mocked<typeof apiClient>;

describe("companiesApi", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("getAll", () => {
    it("should fetch all companies", async () => {
      const validGuid = "00000000-0000-0000-0000-000000000001";
      const mockCompanies = [
        { id: validGuid, name: "Company 1", address: "Address 1", isActive: true, createdAt: "2024-01-01" },
      ];
      mockApiClient.get.mockResolvedValue(mockCompanies);

      const result = await companiesApi.getAll();

      expect(mockApiClient.get).toHaveBeenCalledWith("/api/company");
      expect(result).toEqual(mockCompanies);
    });
  });

  describe("getById", () => {
    it("should fetch company by id", async () => {
      const validGuid = "00000000-0000-0000-0000-000000000001";
      const mockCompany = {
        id: validGuid,
        name: "Company 1",
        address: "Address 1",
        isActive: true,
        createdAt: "2024-01-01",
      };
      mockApiClient.get.mockResolvedValue(mockCompany);

      const result = await companiesApi.getById(validGuid);

      expect(mockApiClient.get).toHaveBeenCalledWith(`/api/company/${validGuid}`);
      expect(result).toEqual(mockCompany);
    });
  });

  describe("create", () => {
    it("should create a new company", async () => {
      const validGuid = "00000000-0000-0000-0000-000000000001";
      const newCompany = {
        name: "New Company",
        address: "New Address",
      };
      const createdCompany = {
        id: validGuid,
        ...newCompany,
        isActive: true,
        createdAt: "2024-01-01",
      };
      mockApiClient.post.mockResolvedValue(createdCompany);

      const result = await companiesApi.create(newCompany);

      expect(mockApiClient.post).toHaveBeenCalledWith("/api/company", newCompany);
      expect(result).toEqual(createdCompany);
    });
  });

  describe("update", () => {
    it("should update an existing company", async () => {
      const validGuid = "00000000-0000-0000-0000-000000000001";
      const updateData = {
        name: "Updated Company",
        address: "Updated Address",
        isActive: true,
      };
      const updatedCompany = {
        id: validGuid,
        ...updateData,
        createdAt: "2024-01-01",
      };
      mockApiClient.put.mockResolvedValue(updatedCompany);

      const result = await companiesApi.update(validGuid, updateData);

      expect(mockApiClient.put).toHaveBeenCalledWith(`/api/company/${validGuid}`, updateData);
      expect(result).toEqual(updatedCompany);
    });
  });

  describe("delete", () => {
    it("should delete a company", async () => {
      const validGuid = "00000000-0000-0000-0000-000000000001";
      mockApiClient.delete.mockResolvedValue(undefined);

      await companiesApi.delete(validGuid);

      expect(mockApiClient.delete).toHaveBeenCalledWith(`/api/company/${validGuid}`);
    });
  });
});

