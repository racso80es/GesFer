import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para la página de Empresas
 * Usa getByTestId cuando está disponible, fallback a otros métodos
 */
export class EmpresasPage extends BasePage {
  readonly title: Locator;
  readonly newCompanyButton: Locator;
  readonly companiesTable: Locator;
  readonly createModal: Locator;
  readonly editModal: Locator;
  readonly companiesList: Locator;

  constructor(page: Page) {
    super(page);
    
    // Preferir getByTestId, con fallback a getByRole
    this.title = page.getByRole('heading', { name: /empresas|companies/i }).first();
    this.newCompanyButton = page.getByRole('button', { name: /nueva empresa|new company/i }).first();
    this.companiesTable = page.locator('table').first();
    this.companiesList = page.locator('tbody tr');
    // Usar un selector más simple para el modal, cualquier diálogo abierto
    this.createModal = page.locator('[role="dialog"]').first();
    this.editModal = page.locator('[role="dialog"]').nth(1);
  }

  /**
   * Navega a la página de empresas
   */
  async goto() {
    await super.goto('/empresas');
    
    // Esperar a que la página esté completamente cargada
    await this.page.waitForURL(/\/empresas/, { timeout: 15000 });
    
    // Esperar a que el estado de carga se complete
    await this.waitForLoad();
    
    // Esperar a que el título esté visible (indica que la página cargó completamente)
    await this.title.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Abre el modal de crear empresa
   */
  async openCreateModal() {
    // Esperar a que el botón esté visible y habilitado
    await this.newCompanyButton.waitFor({ state: 'visible', timeout: 10000 });
    await this.newCompanyButton.waitFor({ state: 'attached', timeout: 10000 });
    
    // Esperar a que la página esté completamente cargada
    await this.page.waitForLoadState('networkidle');
    
    // Intentar hacer scroll para asegurar que el botón esté en la vista
    await this.newCompanyButton.scrollIntoViewIfNeeded();
    
    // Esperar un momento para que cualquier animación termine
    await this.page.waitForTimeout(500);
    
    // Verificar que el botón no esté deshabilitado
    const isDisabled = await this.newCompanyButton.isDisabled().catch(() => false);
    if (isDisabled) {
      throw new Error('El botón de crear empresa está deshabilitado');
    }
    
    // Interceptar el evento de apertura del modal antes de hacer click
    const dialogPromise = this.page.waitForSelector('[role="dialog"]', { state: 'visible', timeout: 10000 }).catch(() => null);
    
    // Intentar hacer click
    try {
      await this.newCompanyButton.click({ timeout: 10000, force: false });
    } catch (error) {
      // Si falla, intentar con force
      await this.newCompanyButton.click({ force: true, timeout: 10000 });
    }
    
    // Esperar a que el modal aparezca
    await dialogPromise;
    await this.createModal.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Verifica que existe al menos una empresa en la tabla
   */
  async verifyCompaniesList() {
    await this.companiesTable.waitFor({ state: 'visible', timeout: 5000 });
    const rows = this.companiesList;
    const count = await rows.count();
    return count > 0;
  }

  /**
   * Busca una empresa por nombre
   */
  async findCompanyByName(name: string): Promise<Locator | null> {
    const rows = this.companiesList;
    const count = await rows.count();
    
    for (let i = 0; i < count; i++) {
      const row = rows.nth(i);
      const nameCell = row.locator('td').first();
      const text = await nameCell.textContent();
      if (text?.includes(name)) {
        return row;
      }
    }
    return null;
  }
}

