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
      const mockCompanies = [
        { id: "1", name: "Company 1", address: "Address 1", isActive: true, createdAt: "2024-01-01" },
      ];
      mockApiClient.get.mockResolvedValue(mockCompanies);

      const result = await companiesApi.getAll();

      expect(mockApiClient.get).toHaveBeenCalledWith("/api/company");
      expect(result).toEqual(mockCompanies);
    });
  });

  describe("getById", () => {
    it("should fetch company by id", async () => {
      const mockCompany = {
        id: "1",
        name: "Company 1",
        address: "Address 1",
        isActive: true,
        createdAt: "2024-01-01",
      };
      mockApiClient.get.mockResolvedValue(mockCompany);

      const result = await companiesApi.getById("1");

      expect(mockApiClient.get).toHaveBeenCalledWith("/api/company/1");
      expect(result).toEqual(mockCompany);
    });
  });

  describe("create", () => {
    it("should create a new company", async () => {
      const newCompany = {
        name: "New Company",
        address: "New Address",
      };
      const createdCompany = {
        id: "1",
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
      const updateData = {
        name: "Updated Company",
        address: "Updated Address",
        isActive: true,
      };
      const updatedCompany = {
        id: "1",
        ...updateData,
        createdAt: "2024-01-01",
      };
      mockApiClient.put.mockResolvedValue(updatedCompany);

      const result = await companiesApi.update("1", updateData);

      expect(mockApiClient.put).toHaveBeenCalledWith("/api/company/1", updateData);
      expect(result).toEqual(updatedCompany);
    });
  });

  describe("delete", () => {
    it("should delete a company", async () => {
      mockApiClient.delete.mockResolvedValue(undefined);

      await companiesApi.delete("1");

      expect(mockApiClient.delete).toHaveBeenCalledWith("/api/company/1");
    });
  });
});

