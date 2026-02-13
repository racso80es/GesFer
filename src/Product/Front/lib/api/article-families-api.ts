import { apiClient } from "./client";

export interface ArticleFamily {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description?: string;
  taxTypeId: string;
  taxTypeName?: string;
  taxTypeValue?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateArticleFamilyDto {
  companyId: string;
  code: string;
  name: string;
  description?: string;
  taxTypeId: string;
}

export interface UpdateArticleFamilyDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  taxTypeId: string;
}

const BASE_URL = "/api/article-families";

export const articleFamiliesApi = {
  getAll: async (): Promise<ArticleFamily[]> => {
    return apiClient.get<ArticleFamily[]>(BASE_URL);
  },

  getById: async (id: string): Promise<ArticleFamily> => {
    return apiClient.get<ArticleFamily>(`${BASE_URL}/${id}`);
  },

  create: async (data: CreateArticleFamilyDto): Promise<string> => {
    const created = await apiClient.post<ArticleFamily>(BASE_URL, data);
    return created.id;
  },

  update: async (id: string, data: UpdateArticleFamilyDto): Promise<void> => {
    await apiClient.put<void>(`${BASE_URL}/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete<void>(`${BASE_URL}/${id}`);
  },
};
