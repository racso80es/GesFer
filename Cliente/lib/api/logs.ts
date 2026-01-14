import { apiClient } from "./client";

export interface LogDto {
  id: number;
  level: string;
  message: string;
  messageTemplate?: string;
  exception?: string;
  properties?: string;
  timeStamp: string; // DateTime desde el backend
  source?: string;
  companyId?: string;
  userId?: string;
  clientInfo?: string;
}

export interface LogsPagedResponse {
  items: LogDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface LogsFilter {
  fromDate?: string;
  toDate?: string;
  level?: string;
  companyId?: string;
  userId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export const logsApi = {
  /**
   * Obtiene logs paginados con filtros opcionales
   */
  getAll: async (filters?: LogsFilter): Promise<LogsPagedResponse> => {
    const params: Record<string, string> = {};
    
    if (filters?.fromDate) {
      params.fromDate = filters.fromDate;
    }
    if (filters?.toDate) {
      params.toDate = filters.toDate;
    }
    if (filters?.level) {
      params.level = filters.level;
    }
    if (filters?.companyId) {
      params.companyId = filters.companyId;
    }
    if (filters?.userId) {
      params.userId = filters.userId;
    }
    if (filters?.pageNumber) {
      params.pageNumber = filters.pageNumber.toString();
    }
    if (filters?.pageSize) {
      params.pageSize = filters.pageSize.toString();
    }

    return apiClient.get<LogsPagedResponse>("/api/log", params);
  },
};
