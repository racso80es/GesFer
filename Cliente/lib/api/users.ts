import { apiClient } from "./client";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";

export const usersApi = {
  getAll: async (companyId?: string): Promise<User[]> => {
    const params = companyId ? { companyId } : undefined;
    return apiClient.get<User[]>("/api/user", params);
  },

  getById: async (id: string): Promise<User> => {
    return apiClient.get<User>(`/api/user/${id}`);
  },

  create: async (data: CreateUser): Promise<User> => {
    return apiClient.post<User>("/api/user", data);
  },

  update: async (id: string, data: UpdateUser): Promise<User> => {
    return apiClient.put<User>(`/api/user/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    return apiClient.delete<void>(`/api/user/${id}`);
  },
};

