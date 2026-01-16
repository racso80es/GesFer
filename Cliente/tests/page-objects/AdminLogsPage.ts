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
    // Usar data-testid de componentes shared
    this.fromDateInput = page.getByTestId('shared-input-datetime-from');
    this.toDateInput = page.getByTestId('shared-input-datetime-to');
    this.levelSelect = page.locator('#level'); // Select nativo, no tiene data-testid aún
    this.applyFiltersButton = page.getByTestId('shared-button-apply-filters');
    this.clearFiltersButton = page.getByTestId('shared-button-clear-filters');
    // DataTable usa data-testid="shared-datatable-logs"
    const dataTableContainer = page.getByTestId('shared-datatable-logs');
    this.logsTable = dataTableContainer.locator('table');
    this.logsTableRows = dataTableContainer.locator('table tbody tr').filter({ hasNot: page.locator('[colspan]') });
    this.noLogsMessage = page.getByTestId('shared-datatable-logs-empty');
    this.paginationInfo = page.getByTestId('shared-datatable-logs-pagination').locator('text=/página \\d+ de \\d+/i');
    this.previousPageButton = page.getByTestId('shared-datatable-logs-pagination-previous');
    this.nextPageButton = page.getByTestId('shared-datatable-logs-pagination-next');
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
