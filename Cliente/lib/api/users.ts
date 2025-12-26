import { apiClient } from "./client";
import type { User, CreateUser, UpdateUser } from "@/lib/types/api";
import { validateId } from "@/lib/utils/id-validation";

export const usersApi = {
  getAll: async (companyId?: string): Promise<User[]> => {
    const params = companyId ? { companyId: validateId(companyId, "empresa") } : undefined;
    return apiClient.get<User[]>("/api/user", params);
  },

  getById: async (id: string): Promise<User> => {
    const validId = validateId(id, "usuario");
    return apiClient.get<User>(`/api/user/${validId}`);
  },

  create: async (data: CreateUser): Promise<User> => {
    return apiClient.post<User>("/api/user", data);
  },

  update: async (id: string, data: UpdateUser): Promise<User> => {
    const validId = validateId(id, "usuario");
    return apiClient.put<User>(`/api/user/${validId}`, data);
  },

  delete: async (id: string): Promise<void> => {
    const validId = validateId(id, "usuario");
    return apiClient.delete<void>(`/api/user/${validId}`);
  },
};

