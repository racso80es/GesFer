import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para la página de Login
 * Usa getByTestId cuando está disponible, fallback a getByLabel/getByRole
 */
export class LoginPage extends BasePage {
  // Selectores - preferir getByTestId, fallback a otros métodos
  readonly empresaInput: Locator;
  readonly usuarioInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly errorMessage: Locator;
  readonly loginForm: Locator;

  constructor(page: Page) {
    super(page);
    
    // Preferir getByTestId, con fallback a getByLabel, usando .first() para evitar múltiples elementos
    this.empresaInput = page.getByTestId('login-empresa-input').or(page.getByLabel(/empresa|company/i)).first();
    this.usuarioInput = page.getByTestId('login-usuario-input').or(page.getByLabel(/usuario|username/i)).first();
    this.passwordInput = page.getByTestId('login-password-input').or(page.getByLabel(/contraseña|password/i)).first();
    this.loginButton = page.getByTestId('login-submit-button').or(page.getByRole('button', { name: /iniciar sesión|login/i })).first();
    this.errorMessage = page.getByTestId('login-error-message').or(page.locator('[role="alert"], .error-message')).first();
    this.loginForm = page.getByTestId('login-form').or(page.locator('form')).first();
  }

  /**
   * Navega a la página de login
   */
  async goto() {
    await super.goto('/login');
    await this.waitForLoad();
  }

  /**
   * Realiza login con las credenciales proporcionadas
   */
  async login(empresa: string, usuario: string, password: string) {
    await this.empresaInput.fill(empresa);
    await this.usuarioInput.fill(usuario);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }

  /**
   * Verifica que el login fue exitoso (redirige al dashboard)
   */
  async verifyLoginSuccess() {
    // Esperar a que el localStorage tenga el usuario (indica que el login se completó)
    await this.page.waitForFunction(() => {
      return localStorage.getItem('auth_user') !== null;
    }, { timeout: 5000 });
    
    // Esperar a que la URL cambie a dashboard (puede tener locale o no)
    // Usar waitForURL con un timeout más largo
    await this.page.waitForURL(
      (url) => {
        const pathname = url.pathname;
        return pathname.includes('dashboard') && !pathname.includes('login');
      },
      { timeout: 10000, waitUntil: 'domcontentloaded' }
    );
    
    // Esperar a que el estado de carga se complete
    await this.page.waitForLoadState('domcontentloaded');
  }

  /**
   * Verifica que se muestra un mensaje de error
   */
  async verifyErrorMessage() {
    await this.errorMessage.waitFor({ state: 'visible', timeout: 5000 });
  }
}

