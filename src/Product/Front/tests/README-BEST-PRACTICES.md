# Mejores Prácticas para Tests de Playwright

## Reglas Establecidas

Al escribir tests de Playwright en este proyecto, siempre seguir estas reglas:

### 1. ✅ Usar TypeScript
- Todos los tests y Page Objects deben estar escritos en TypeScript
- Aprovechar el tipado fuerte para prevenir errores

### 2. ✅ Seguir el Patrón Page Object Model (POM)
- Cada página debe tener su propio Page Object
- Los Page Objects encapsulan la lógica de interacción
- Los tests solo deben llamar métodos de los Page Objects, no interactuar directamente con el DOM

### 3. ✅ Preferir `page.getByTestId()` sobre selectores CSS
- **SIEMPRE** usar `page.getByTestId('test-id')` cuando sea posible
- Usar `.or()` para proporcionar fallback a otros métodos si el test-id no está disponible:
  ```typescript
  this.button = page.getByTestId('submit-button')
    .or(page.getByRole('button', { name: /submit/i }));
  ```
- Los selectores CSS (`page.locator('.class')`) solo deben usarse como último recurso

### 4. ✅ Incluir Teardown para Limpiar Datos de Prueba
- **SIEMPRE** incluir un `test.afterEach()` o `test.afterAll()` para limpiar datos creados
- Usar la clase `TestDataCleanup` para gestionar la limpieza
- Registrar todos los IDs de recursos creados durante los tests

## Ejemplo Completo

```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/LoginPage';
import { TestDataCleanup } from '../helpers/test-data-cleanup';

test.describe('Mi Feature', () => {
  let cleanup: TestDataCleanup;
  const createdResourceIds: string[] = [];

  test.beforeEach(async ({ page, request }) => {
    cleanup = new TestDataCleanup(request);
    await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
    
    // Setup del test...
  });

  test.afterEach(async () => {
    // Teardown: Limpiar datos de prueba
    for (const id of createdResourceIds) {
      await cleanup.cleanupResource(id);
    }
    createdResourceIds.length = 0;
  });

  test('debe crear un recurso', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    
    // Test logic...
    const resourceId = '...';
    createdResourceIds.push(resourceId);
    cleanup.registerResourceId(resourceId);
  });
});
```

## Page Object Example

```typescript
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class MyPage extends BasePage {
  readonly submitButton: Locator;
  readonly inputField: Locator;

  constructor(page: Page) {
    super(page);
    
    // Preferir getByTestId con fallback
    this.submitButton = page.getByTestId('submit-button')
      .or(page.getByRole('button', { name: /submit/i }));
    
    this.inputField = page.getByTestId('input-field')
      .or(page.getByLabel(/input/i));
  }

  async submit() {
    await this.submitButton.click();
  }
}
```

## Agregar Test IDs a los Componentes

Para que los tests funcionen correctamente, los componentes deben incluir atributos `data-testid`:

```tsx
<Button 
  data-testid="login-submit-button"
  type="submit"
>
  Iniciar Sesión
</Button>

<input
  data-testid="login-empresa-input"
  id="empresa"
  type="text"
/>
```

## Clase TestDataCleanup

La clase `TestDataCleanup` proporciona métodos para:
- Registrar IDs de recursos creados
- Limpiar recursos individuales o en lote
- Gestionar autenticación para operaciones de limpieza

```typescript
const cleanup = new TestDataCleanup(request);
await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');
cleanup.registerUserId(userId);
await cleanup.cleanup(); // Limpia todos los recursos registrados
```



