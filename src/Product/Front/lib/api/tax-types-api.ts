import { apiClient } from "./client";

export interface TaxType {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description?: string;
  value: number;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}

export interface CreateTaxTypeDto {
  code: string;
  name: string;
  description?: string;
  value: number;
}

export interface UpdateTaxTypeDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  value: number;
}

const BASE_URL = "/api/tax-types";

export const taxTypesApi = {
  getAll: async (): Promise<TaxType[]> => {
    return apiClient.get<TaxType[]>(BASE_URL);
  },

  getById: async (id: string): Promise<TaxType> => {
    return apiClient.get<TaxType>(`${BASE_URL}/${id}`);
  },

  create: async (data: CreateTaxTypeDto): Promise<string> => {
    const created = await apiClient.post<TaxType>(BASE_URL, data);
    return created?.id ?? "";
  },

  update: async (id: string, data: UpdateTaxTypeDto): Promise<void> => {
    await apiClient.put<void>(`${BASE_URL}/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete<void>(`${BASE_URL}/${id}`);
  },
};
