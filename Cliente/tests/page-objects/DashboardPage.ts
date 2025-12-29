import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para la página del Dashboard
 * Usa getByTestId cuando está disponible, fallback a otros métodos
 */
export class DashboardPage extends BasePage {
  readonly title: Locator;
  readonly usuariosLink: Locator;
  readonly empresasLink: Locator;
  readonly clientesLink: Locator;
  readonly logoutButton: Locator;

  constructor(page: Page) {
    super(page);
    
    // Preferir getByTestId, con fallback a getByRole, usando .first() para evitar múltiples elementos
    this.title = page.getByTestId('dashboard-title').or(page.getByRole('heading', { name: /panel de control|dashboard/i })).first();
    this.usuariosLink = page.getByTestId('dashboard-usuarios-link').or(page.getByRole('link', { name: /usuarios|users/i })).first();
    this.empresasLink = page.getByTestId('dashboard-empresas-link').or(page.getByRole('link', { name: /empresas|companies/i })).first();
    this.clientesLink = page.getByTestId('dashboard-clientes-link').or(page.getByRole('link', { name: /clientes|customers/i })).first();
    this.logoutButton = page.getByTestId('dashboard-logout-button').or(page.getByRole('button', { name: /cerrar sesión|logout/i })).first();
  }

  /**
   * Navega al dashboard
   */
  async goto() {
    await super.goto('/dashboard');
    await this.waitForLoad();
  }

  /**
   * Navega a la sección de usuarios
   */
  async goToUsuarios() {
    await this.usuariosLink.click();
    await this.page.waitForURL(/\/usuarios/, { timeout: 5000 });
  }

  /**
   * Navega a la sección de empresas
   */
  async goToEmpresas() {
    await this.empresasLink.click();
    await this.page.waitForURL(/\/empresas/, { timeout: 5000 });
  }

  /**
   * Realiza logout
   */
  async logout() {
    await this.logoutButton.click();
    await this.page.waitForURL(/\/login/, { timeout: 5000 });
  }
}

