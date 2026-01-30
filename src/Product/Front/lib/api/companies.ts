import { apiClient } from "./client";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";
import { validateId } from "@/lib/utils/id-validation";

export const companiesApi = {
  getAll: async (): Promise<Company[]> => {
    return apiClient.get<Company[]>("/api/company");
  },

  getById: async (id: string): Promise<Company> => {
    const validId = validateId(id, "empresa");
    return apiClient.get<Company>(`/api/company/${validId}`);
  },

  create: async (data: CreateCompany): Promise<Company> => {
    return apiClient.post<Company>("/api/company", data);
  },

  update: async (id: string, data: UpdateCompany): Promise<Company> => {
    const validId = validateId(id, "empresa");
    return apiClient.put<Company>(`/api/company/${validId}`, data);
  },

  delete: async (id: string): Promise<void> => {
    const validId = validateId(id, "empresa");
    return apiClient.delete<void>(`/api/company/${validId}`);
  },
};

