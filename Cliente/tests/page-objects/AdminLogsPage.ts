import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para la página de Logs de Admin
 */
export class AdminLogsPage extends BasePage {
  readonly title: Locator;
  readonly fromDateInput: Locator;
  readonly toDateInput: Locator;
  readonly levelSelect: Locator;
  readonly applyFiltersButton: Locator;
  readonly clearFiltersButton: Locator;
  readonly logsTable: Locator;
  readonly logsTableRows: Locator;
  readonly noLogsMessage: Locator;
  readonly paginationInfo: Locator;
  readonly previousPageButton: Locator;
  readonly nextPageButton: Locator;

  constructor(page: Page) {
    super(page);
    
    this.title = page.getByRole('heading', { name: /logs del sistema/i });
    this.fromDateInput = page.locator('#fromDate');
    this.toDateInput = page.locator('#toDate');
    this.levelSelect = page.locator('#level');
    this.applyFiltersButton = page.getByRole('button', { name: /aplicar filtros/i });
    this.clearFiltersButton = page.getByRole('button', { name: /limpiar filtros/i });
    this.logsTable = page.locator('table');
    this.logsTableRows = page.locator('table tbody tr').filter({ hasNot: page.locator('[colspan]') });
    this.noLogsMessage = page.getByText(/no hay logs disponibles/i);
    this.paginationInfo = page.locator('text=/página \\d+ de \\d+/i');
    this.previousPageButton = page.getByRole('button', { name: /anterior/i });
    this.nextPageButton = page.getByRole('button', { name: /siguiente/i });
  }

  /**
   * Navega a la página de logs de admin
   */
  async goto() {
    await super.goto('/admin/logs');
    await this.waitForLoad();
    await this.title.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Aplica filtros de fecha y nivel
   */
  async applyFilters(fromDate?: string, toDate?: string, level?: string) {
    if (fromDate) {
      await this.fromDateInput.fill(fromDate);
    }
    if (toDate) {
      await this.toDateInput.fill(toDate);
    }
    if (level) {
      await this.levelSelect.selectOption(level);
    }
    await this.applyFiltersButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Limpia los filtros
   */
  async clearFilters() {
    await this.clearFiltersButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Expande los detalles de un log por su ID
   */
  async expandLogDetails(logId: string) {
    const row = this.page.locator(`tr[data-log-id="${logId}"]`).or(
      this.page.locator('table tbody tr').filter({ hasText: logId }).first()
    );
    const expandButton = row.locator('button').filter({ has: this.page.locator('svg') });
    await expandButton.click();
  }

  /**
   * Obtiene el número de logs visibles en la tabla
   */
  async getLogsCount(): Promise<number> {
    const rows = await this.logsTableRows.count();
    return rows;
  }

  /**
   * Verifica que un log con un mensaje específico esté presente
   */
  async verifyLogMessageExists(message: string): Promise<boolean> {
    const logRow = this.page.locator('table tbody tr').filter({ hasText: message }).first();
    return await logRow.isVisible({ timeout: 5000 }).catch(() => false);
  }

  /**
   * Obtiene el texto del log en una fila específica
   */
  async getLogMessage(rowIndex: number): Promise<string> {
    const row = this.logsTableRows.nth(rowIndex);
    const messageCell = row.locator('td').nth(2); // La columna de mensaje
    return await messageCell.textContent() || '';
  }
}
