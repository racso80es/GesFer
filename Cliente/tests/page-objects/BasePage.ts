import { Page, Locator } from '@playwright/test';

/**
 * Clase base para todos los Page Objects
 * Proporciona funcionalidad común y métodos auxiliares
 */
export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Navega a una ruta relativa usando la baseURL configurada
   */
  async goto(path: string = '/') {
    await this.page.goto(path);
  }

  /**
   * Espera a que la página esté completamente cargada
   */
  async waitForLoad() {
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Obtiene el título de la página
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }

  /**
   * Toma un screenshot
   */
  async takeScreenshot(name: string) {
    await this.page.screenshot({ path: `test-results/screenshots/${name}.png` });
  }
}

