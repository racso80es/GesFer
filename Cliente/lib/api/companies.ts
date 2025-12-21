import { apiClient } from "./client";
import type { Company, CreateCompany, UpdateCompany } from "@/lib/types/api";

export const companiesApi = {
  getAll: async (): Promise<Company[]> => {
    return apiClient.get<Company[]>("/api/company");
  },

  getById: async (id: string): Promise<Company> => {
    return apiClient.get<Company>(`/api/company/${id}`);
  },

  create: async (data: CreateCompany): Promise<Company> => {
    return apiClient.post<Company>("/api/company", data);
  },

  update: async (id: string, data: UpdateCompany): Promise<Company> => {
    return apiClient.put<Company>(`/api/company/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    return apiClient.delete<void>(`/api/company/${id}`);
  },
};

