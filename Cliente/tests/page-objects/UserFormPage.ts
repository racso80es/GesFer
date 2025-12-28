import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para el formulario de Usuario
 * Usa getByTestId cuando está disponible, fallback a otros métodos
 */
export class UserFormPage extends BasePage {
  readonly form: Locator;
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly firstNameInput: Locator;
  readonly lastNameInput: Locator;
  readonly emailInput: Locator;
  readonly phoneInput: Locator;
  readonly addressInput: Locator;
  readonly languageSelect: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page, isEdit: boolean = false) {
    super(page);
    
    const formTestId = isEdit ? 'user-form-edit' : 'user-form-create';
    
    // Preferir getByTestId, con fallback
    this.form = page.getByTestId(formTestId).or(page.locator('form'));
    this.usernameInput = page.getByTestId('user-form-username-input').or(page.getByLabel(/username|nombre de usuario/i));
    this.passwordInput = page.getByTestId('user-form-password-input').or(page.getByLabel(/password|contraseña/i));
    this.firstNameInput = page.getByTestId('user-form-firstname-input').or(page.getByLabel(/first name|nombre/i));
    this.lastNameInput = page.getByTestId('user-form-lastname-input').or(page.getByLabel(/last name|apellido/i));
    this.emailInput = page.getByTestId('user-form-email-input').or(page.getByLabel(/email/i));
    this.phoneInput = page.getByLabel(/phone|teléfono/i);
    this.addressInput = page.getByLabel(/address|dirección/i);
    this.languageSelect = page.locator('select#languageId');
    this.submitButton = page.getByTestId('user-form-submit-button').or(page.getByRole('button', { name: /create|crear|update|actualizar/i }));
    this.cancelButton = page.getByTestId('user-form-cancel-button').or(page.getByRole('button', { name: /cancel|cancelar/i }));
    this.errorMessage = page.getByTestId('user-form-error').or(page.locator('[role="alert"]'));
  }

  /**
   * Llena el formulario con los datos proporcionados
   */
  async fillForm(data: {
    username: string;
    password: string;
    firstName: string;
    lastName: string;
    email?: string;
    phone?: string;
    address?: string;
    languageId?: string;
  }) {
    // Esperar a que el formulario esté completamente cargado
    await this.form.waitFor({ state: 'visible', timeout: 10000 });
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500); // Esperar a que el formulario se renderice completamente
    
    // Llenar campos requeridos primero
    await this.usernameInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.usernameInput.waitFor({ state: 'attached', timeout: 10000 });
    await this.usernameInput.fill(data.username);
    
    await this.passwordInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.passwordInput.waitFor({ state: 'attached', timeout: 10000 });
    await this.passwordInput.fill(data.password);
    
    await this.firstNameInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.firstNameInput.waitFor({ state: 'attached', timeout: 10000 });
    await this.firstNameInput.fill(data.firstName);
    
    await this.lastNameInput.waitFor({ state: 'visible', timeout: 10000 });
    await this.lastNameInput.waitFor({ state: 'attached', timeout: 10000 });
    await this.lastNameInput.fill(data.lastName);
    
    // Llenar campos opcionales si se proporcionan
    if (data.email) {
      await this.emailInput.waitFor({ state: 'visible', timeout: 10000 });
      await this.emailInput.waitFor({ state: 'attached', timeout: 10000 });
      await this.emailInput.fill(data.email);
    }
    
    if (data.phone) {
      await this.phoneInput.waitFor({ state: 'visible', timeout: 10000 });
      await this.phoneInput.waitFor({ state: 'attached', timeout: 10000 });
      await this.phoneInput.fill(data.phone);
    }
    
    // El campo address puede estar deshabilitado, intentar llenarlo solo si está habilitado
    if (data.address) {
      try {
        await this.addressInput.waitFor({ state: 'visible', timeout: 10000 });
        const isDisabled = await this.addressInput.isDisabled().catch(() => true);
        if (!isDisabled) {
          await this.addressInput.fill(data.address);
        }
        // Si está deshabilitado, simplemente continuar (es opcional)
      } catch (error) {
        // Si hay un error, continuar (el campo es opcional)
        console.log('No se pudo llenar el campo address (opcional)');
      }
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
      // Si falla, usar force para evitar problemas de elementos que interceptan
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
      // Si falla, usar force para evitar problemas de elementos que interceptan
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

