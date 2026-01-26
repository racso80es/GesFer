import { apiClient } from "./client";
import type { Customer, CreateCustomer, UpdateCustomer } from "@/lib/types/api";

export const customersApi = {
  getAll: async (companyId?: string): Promise<Customer[]> => {
    const params = companyId ? { companyId } : undefined;
    return apiClient.get<Customer[]>("/api/customer", params);
  },

  getById: async (id: string): Promise<Customer> => {
    return apiClient.get<Customer>(`/api/customer/${id}`);
  },

  create: async (data: CreateCustomer): Promise<Customer> => {
    return apiClient.post<Customer>("/api/customer", data);
  },

  update: async (id: string, data: UpdateCustomer): Promise<Customer> => {
    return apiClient.put<Customer>(`/api/customer/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    return apiClient.delete<void>(`/api/customer/${id}`);
  },
};

