import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './BasePage';

export class CompaniesPage extends BasePage {
  readonly title: Locator;
  readonly nameInput: Locator;
  readonly taxIdInput: Locator;
  readonly emailInput: Locator;
  readonly addressInput: Locator;
  readonly saveButton: Locator;

  constructor(page: Page) {
    super(page);

    this.title = page.getByRole('heading', { name: /mi organización|organizaciones/i }).first();

    // Form Inputs (directly on page now)
    this.nameInput = page.locator('#name').or(page.getByTestId('company-form-name'));
    this.taxIdInput = page.locator('#taxId').or(page.getByTestId('company-form-taxId'));
    this.emailInput = page.locator('#email').or(page.getByTestId('company-form-email'));
    this.addressInput = page.locator('#address');

    // Botones
    this.saveButton = page.getByRole('button', { name: /guardar|save|actualizar|update/i });
  }

  async goto() {
    await this.page.goto('/companies');
    await this.waitForLoad();
  }

  async updateCompany(data: { name?: string; taxId?: string; email?: string; address?: string }) {
    if (data.name) await this.nameInput.fill(data.name);
    if (data.taxId) await this.taxIdInput.fill(data.taxId);
    if (data.email) await this.emailInput.fill(data.email);
    if (data.address) await this.addressInput.fill(data.address);

    await this.saveButton.click();
  }

  async verifySuccessMessage() {
    await expect(this.page.locator('text=/actualizada correctamente|success/i')).toBeVisible({ timeout: 5000 });
  }

  async verifyCompanyData(data: { name?: string; taxId?: string; email?: string; address?: string }) {
      if (data.name) await expect(this.nameInput).toHaveValue(data.name);
      if (data.taxId) await expect(this.taxIdInput).toHaveValue(data.taxId);
      if (data.email) await expect(this.emailInput).toHaveValue(data.email);
      if (data.address) await expect(this.addressInput).toHaveValue(data.address);
  }
}
