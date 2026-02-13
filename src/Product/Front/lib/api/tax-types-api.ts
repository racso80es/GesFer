import { fetchWithAuth } from "../auth";

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
    return fetchWithAuth(BASE_URL);
  },

  getById: async (id: string): Promise<TaxType> => {
    return fetchWithAuth(`${BASE_URL}/${id}`);
  },

  create: async (data: CreateTaxTypeDto): Promise<string> => {
    return fetchWithAuth(BASE_URL, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: UpdateTaxTypeDto): Promise<void> => {
    return fetchWithAuth(`${BASE_URL}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return fetchWithAuth(`${BASE_URL}/${id}`, {
      method: "DELETE",
    });
  },
};
