import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './BasePage';

export class CompaniesPage extends BasePage {
  readonly title: Locator;
  readonly newCompanyButton: Locator;
  readonly companiesList: Locator;

  // Modal Elements
  readonly createModal: Locator;
  readonly nameInput: Locator;
  readonly taxIdInput: Locator;
  readonly emailInput: Locator;
  readonly addressInput: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);

    this.title = page.getByRole('heading', { name: /companies|companies/i }).first();
    this.newCompanyButton = page.getByRole('button', { name: /nova company|new company|crear|create/i }).first();
    this.companiesList = page.locator('table'); // Más genérico si no tiene role table explícito, aunque debería

    // Modal - Ajustado selectores para ser más robustos
    this.createModal = page.locator('div[role="dialog"]');

    // Inputs dentro del modal
    this.nameInput = this.createModal.locator('input[name="name"]');
    this.taxIdInput = this.createModal.locator('input[name="taxId"]');
    this.emailInput = this.createModal.locator('input[name="email"]');
    this.addressInput = this.createModal.locator('input[name="address"]');

    // Botones
    this.saveButton = this.createModal.getByRole('button', { name: /guardar|save|crear|create/i });
    this.cancelButton = this.createModal.getByRole('button', { name: /cancel/i });
  }

  async goto() {
    await this.page.goto('/companies');
    await this.waitForLoad();
  }

  async openCreateModal() {
    await this.newCompanyButton.click();
    await expect(this.createModal).toBeVisible();
  }

  async createCompany(name: string, taxId: string, email: string, address: string) {
    await this.openCreateModal();

    await this.nameInput.fill(name);
    await this.taxIdInput.fill(taxId);
    await this.emailInput.fill(email);
    await this.addressInput.fill(address);

    await this.saveButton.click();
  }

  async verifyCompanyExists(name: string) {
    // Esperar a que la tabla se recargue o el elemento aparezca
    await expect(this.companiesList).toContainText(name, { timeout: 10000 });
  }
}
