import { APIRequestContext } from '@playwright/test';
import { ApiClient } from '../api/api-client';
import { appConfig } from '../../lib/config';

/**
 * Helper para limpiar datos de prueba de la API
 */
export class TestDataCleanup {
  private apiClient: ApiClient;
  private createdUserIds: string[] = [];
  private createdCompanyIds: string[] = [];
  private createdCustomerIds: string[] = [];
  private authToken: string | null = null;

  constructor(request: APIRequestContext, baseURL: string = appConfig.api.url) {
    this.apiClient = new ApiClient(request, baseURL);
  }

  /**
   * Establece el token de autenticación para las operaciones de limpieza
   */
  async setAuthToken(empresa: string, usuario: string, contraseña: string): Promise<void> {
    try {
      this.authToken = await this.apiClient.login(empresa, usuario, contraseña);
    } catch (error) {
      console.warn('No se pudo obtener token de autenticación para limpieza:', error);
    }
  }

  /**
   * Registra un ID de usuario creado para limpieza posterior
   */
  registerUserId(userId: string): void {
    this.createdUserIds.push(userId);
  }

  /**
   * Registra un ID de empresa creado para limpieza posterior
   */
  registerCompanyId(companyId: string): void {
    this.createdCompanyIds.push(companyId);
  }

  /**
   * Registra un ID de cliente creado para limpieza posterior
   */
  registerCustomerId(customerId: string): void {
    this.createdCustomerIds.push(customerId);
  }

  /**
   * Limpia todos los datos de prueba registrados
   */
  async cleanup(): Promise<void> {
    if (!this.authToken) {
      console.warn('No hay token de autenticación, no se puede limpiar datos');
      return;
    }

    // Limpiar usuarios
    for (const userId of this.createdUserIds) {
      try {
        await this.apiClient.deleteUser(userId, this.authToken);
      } catch (error) {
        console.warn(`Error al eliminar usuario ${userId}:`, error);
      }
    }

    // Limpiar empresas
    for (const companyId of this.createdCompanyIds) {
      try {
        await this.apiClient.deleteCompany(companyId, this.authToken);
      } catch (error) {
        console.warn(`Error al eliminar empresa ${companyId}:`, error);
      }
    }

    // Limpiar clientes
    for (const customerId of this.createdCustomerIds) {
      try {
        await this.apiClient.deleteCustomer(customerId, this.authToken);
      } catch (error) {
        console.warn(`Error al eliminar cliente ${customerId}:`, error);
      }
    }

    // Limpiar arrays
    this.createdUserIds = [];
    this.createdCompanyIds = [];
    this.createdCustomerIds = [];
  }

  /**
   * Limpia un usuario específico
   */
  async cleanupUser(userId: string): Promise<void> {
    if (!this.authToken) {
      console.warn('No hay token de autenticación, no se puede limpiar usuario');
      return;
    }

    try {
      await this.apiClient.deleteUser(userId, this.authToken);
      this.createdUserIds = this.createdUserIds.filter(id => id !== userId);
    } catch (error) {
      console.warn(`Error al eliminar usuario ${userId}:`, error);
    }
  }

  /**
   * Limpia una empresa específica
   */
  async cleanupCompany(companyId: string): Promise<void> {
    if (!this.authToken) {
      console.warn('No hay token de autenticación, no se puede limpiar empresa');
      return;
    }

    try {
      await this.apiClient.deleteCompany(companyId, this.authToken);
      this.createdCompanyIds = this.createdCompanyIds.filter(id => id !== companyId);
    } catch (error) {
      console.warn(`Error al eliminar empresa ${companyId}:`, error);
    }
  }
}


