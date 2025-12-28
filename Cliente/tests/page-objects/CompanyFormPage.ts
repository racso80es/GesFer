import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para el formulario de Empresa
 * Usa getByTestId cuando está disponible, fallback a otros métodos
 */
export class CompanyFormPage extends BasePage {
  readonly form: Locator;
  readonly nameInput: Locator;
  readonly taxIdInput: Locator;
  readonly addressInput: Locator;
  readonly phoneInput: Locator;
  readonly emailInput: Locator;
  readonly languageSelect: Locator;
  readonly isActiveCheckbox: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page, isEdit: boolean = false) {
    super(page);
    
    // Preferir getByTestId, con fallback
    this.form = page.locator('form').first();
    this.nameInput = page.getByLabel(/nombre|name/i).first();
    this.taxIdInput = page.getByLabel(/cif|tax.*id/i).first();
    this.addressInput = page.getByLabel(/dirección|address/i).first();
    this.phoneInput = page.getByLabel(/teléfono|phone/i).first();
    this.emailInput = page.getByLabel(/email/i).first();
    this.languageSelect = page.locator('select#languageId').first();
    this.isActiveCheckbox = page.locator('input#isActive').first();
    this.submitButton = page.getByRole('button', { name: /crear|create|actualizar|update/i }).last();
    this.cancelButton = page.getByRole('button', { name: /cancel|cancelar/i }).first();
    this.errorMessage = page.locator('[role="alert"]').first();
  }

  /**
   * Llena el formulario con los datos proporcionados
   */
  async fillForm(data: {
    name: string;
    taxId?: string;
    address: string;
    phone?: string;
    email?: string;
    languageId?: string;
    isActive?: boolean;
  }) {
    // Esperar a que el formulario esté completamente cargado
    await this.form.waitFor({ state: 'visible', timeout: 10000 });
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
    
    // Llenar campos requeridos primero
    await this.nameInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.nameInput.fill(data.name);
    
    await this.addressInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.addressInput.fill(data.address);
    
    // Llenar campos opcionales si se proporcionan
    if (data.taxId) {
      await this.taxIdInput.waitFor({ state: 'visible', timeout: 10000 });
      await this.taxIdInput.fill(data.taxId);
    }
    
    if (data.phone) {
      await this.phoneInput.waitFor({ state: 'visible', timeout: 10000 });
      await this.phoneInput.fill(data.phone);
    }
    
    if (data.email) {
      await this.emailInput.waitFor({ state: 'visible', timeout: 10000 });
      await this.emailInput.fill(data.email);
    }
    
    // Manejar languageId - siempre seleccionar algo, incluso si es undefined (seleccionar "por defecto")
    await this.languageSelect.waitFor({ state: 'visible', timeout: 10000 });
    if (data.languageId !== undefined && data.languageId !== null && data.languageId !== '') {
      // Si se proporciona un languageId, seleccionarlo
      await this.languageSelect.selectOption(data.languageId);
    } else {
      // Si languageId es undefined, null o vacío, seleccionar "por defecto" (valor vacío "")
      await this.languageSelect.selectOption('');
    }
    
    if (data.isActive !== undefined) {
      const isVisible = await this.isActiveCheckbox.isVisible().catch(() => false);
      if (isVisible) {
        const isChecked = await this.isActiveCheckbox.isChecked();
        if (data.isActive !== isChecked) {
          await this.isActiveCheckbox.click();
        }
      }
    }
    
    // Esperar un momento para que los cambios se apliquen
    await this.page.waitForTimeout(300);
  }

  /**
   * Envía el formulario
   */
  async submit() {
    await this.submitButton.waitFor({ state: 'visible', timeout: 10000 });
    await this.submitButton.waitFor({ state: 'attached', timeout: 10000 });
    
    // Intentar hacer click normalmente primero
    try {
      await this.submitButton.click({ timeout: 10000, force: false });
    } catch (error) {
      // Si falla, usar force
      await this.submitButton.click({ force: true, timeout: 10000 });
    }
  }

  /**
   * Cancela el formulario
   */
  async cancel() {
    await this.cancelButton.waitFor({ state: 'visible', timeout: 10000 });
    await this.cancelButton.waitFor({ state: 'attached', timeout: 10000 });
    
    // Intentar hacer click normalmente primero
    try {
      await this.cancelButton.click({ timeout: 10000, force: false });
    } catch (error) {
      // Si falla, usar force
      await this.cancelButton.click({ force: true, timeout: 10000 });
    }
  }

  /**
   * Verifica que el formulario está visible
   */
  async verifyFormVisible() {
    await this.form.waitFor({ state: 'visible', timeout: 15000 });
    // Esperar a que el formulario esté completamente renderizado
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Verifica que hay un mensaje de error
   */
  async verifyErrorMessage() {
    await this.errorMessage.waitFor({ state: 'visible', timeout: 5000 });
  }
}

