import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object para la página de Usuarios
 * Usa getByTestId cuando está disponible, fallback a otros métodos
 */
export class UsuariosPage extends BasePage {
  readonly title: Locator;
  readonly newUserButton: Locator;
  readonly usersTable: Locator;
  readonly createModal: Locator;
  readonly editModal: Locator;
  readonly usersList: Locator;

  constructor(page: Page) {
    super(page);
    
    // Preferir getByTestId, con fallback a getByRole, usando .first() para evitar múltiples elementos
    this.title = page.getByTestId('usuarios-title').or(page.getByRole('heading', { name: /usuarios/i })).first();
    this.newUserButton = page.getByTestId('usuarios-new-user-button').or(page.getByRole('button', { name: /nuevo usuario|new user/i })).first();
    this.usersTable = page.getByTestId('usuarios-table').or(page.locator('table')).first();
    this.usersList = page.getByTestId('usuarios-list');
    this.createModal = page.getByTestId('usuarios-create-modal').or(page.locator('[role="dialog"]').filter({ hasText: /crear nuevo usuario|create new user/i })).first();
    this.editModal = page.getByTestId('usuarios-edit-modal').or(page.locator('[role="dialog"]').filter({ hasText: /editar usuario|edit user/i })).first();
  }

  /**
   * Navega a la página de usuarios
   */
  async goto() {
    await super.goto('/usuarios');
    
    // Esperar a que la página esté completamente cargada
    // Verificar que no estamos siendo redirigidos a login
    await this.page.waitForURL(/\/usuarios/, { timeout: 15000 });
    
    // Esperar a que el estado de carga se complete
    await this.waitForLoad();
    
    // Esperar a que el título esté visible (indica que la página cargó completamente)
    // Esto también verifica que no estamos en una página de login
    await this.title.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Abre el modal de crear usuario
   */
  async openCreateModal() {
    // Esperar a que el botón esté visible y habilitado
    await this.newUserButton.waitFor({ state: 'visible', timeout: 10000 });
    await this.newUserButton.waitFor({ state: 'attached', timeout: 10000 });
    
    // Esperar a que la página esté completamente cargada
    await this.page.waitForLoadState('networkidle');
    
    // Intentar hacer scroll para asegurar que el botón esté en la vista
    await this.newUserButton.scrollIntoViewIfNeeded();
    
    // Esperar un momento para que cualquier animación termine
    await this.page.waitForTimeout(500);
    
    // Verificar que el botón no esté deshabilitado
    const isDisabled = await this.newUserButton.isDisabled().catch(() => false);
    if (isDisabled) {
      throw new Error('El botón de crear usuario está deshabilitado');
    }
    
    // Intentar hacer click, usando force si es necesario (para Mobile Chrome)
    try {
      await this.newUserButton.click({ timeout: 10000, force: false });
    } catch (error) {
      // Si falla, intentar con force para evitar problemas de elementos que interceptan
      await this.newUserButton.click({ force: true, timeout: 10000 });
    }
    
    await this.createModal.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Verifica que existe al menos un usuario en la tabla
   */
  async verifyUsersList() {
    await this.usersTable.waitFor({ state: 'visible', timeout: 5000 });
    const rows = this.usersTable.locator('tbody tr');
    const count = await rows.count();
    return count > 0;
  }

  /**
   * Busca un usuario por nombre de usuario
   */
  async findUserByUsername(username: string): Promise<Locator | null> {
    const rows = this.usersTable.locator('tbody tr');
    const count = await rows.count();
    
    for (let i = 0; i < count; i++) {
      const row = rows.nth(i);
      const usernameCell = row.locator('td').first();
      const text = await usernameCell.textContent();
      if (text?.includes(username)) {
        return row;
      }
    }
    return null;
  }
}

