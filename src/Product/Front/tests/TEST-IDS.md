# Test IDs - Referencia Completa

Este documento lista todos los `data-testid` agregados a los componentes para facilitar los tests de Playwright.

## Página de Login (`/login`)

| Elemento | Test ID | Descripción |
|----------|---------|-------------|
| Formulario | `login-form` | Formulario de login |
| Input Company | `login-company-input` | Campo de entrada para company |
| Input Usuario | `login-usuario-input` | Campo de entrada para usuario |
| Input Contraseña | `login-password-input` | Campo de entrada para contraseña |
| Botón Submit | `login-submit-button` | Botón para enviar el formulario |
| Mensaje de Error | `login-error-message` | Mensaje de error del formulario |

## Página de Dashboard (`/dashboard`)

| Elemento | Test ID | Descripción |
|----------|---------|-------------|
| Título | `dashboard-title` | Título principal del dashboard |
| Link Dashboard | `dashboard-dashboard-link` | Link de navegación al dashboard |
| Link Companys | `dashboard-companies-link` | Link de navegación a companies |
| Link Usuarios | `dashboard-usuarios-link` | Link de navegación a usuarios |
| Link Clientes | `dashboard-clientes-link` | Link de navegación a clientes |
| Botón Logout | `dashboard-logout-button` | Botón para cerrar sesión |

## Página de Usuarios (`/usuarios`)

| Elemento | Test ID | Descripción |
|----------|---------|-------------|
| Título | `usuarios-title` | Título principal de la página |
| Botón Nuevo Usuario | `usuarios-new-user-button` | Botón para crear nuevo usuario |
| Lista de Usuarios | `usuarios-list` | Contenedor de la lista de usuarios |
| Tabla de Usuarios | `usuarios-table` | Tabla con los usuarios |
| Modal Crear | `usuarios-create-modal` | Modal para crear usuario |
| Modal Editar | `usuarios-edit-modal` | Modal para editar usuario |

## Formulario de Usuario

| Elemento | Test ID | Descripción |
|----------|---------|-------------|
| Formulario Crear | `user-form-create` | Formulario para crear usuario |
| Formulario Editar | `user-form-edit` | Formulario para editar usuario |
| Input Username | `user-form-username-input` | Campo de nombre de usuario |
| Input Password | `user-form-password-input` | Campo de contraseña |
| Input First Name | `user-form-firstname-input` | Campo de nombre |
| Input Last Name | `user-form-lastname-input` | Campo de apellido |
| Input Email | `user-form-email-input` | Campo de email |
| Botón Submit | `user-form-submit-button` | Botón para enviar formulario |
| Botón Cancel | `user-form-cancel-button` | Botón para cancelar |
| Mensaje de Error | `user-form-error` | Mensaje de error del formulario |

## Componentes UI

### ErrorMessage
- Acepta prop `data-testid` para personalizar el test ID

## Convenciones de Nomenclatura

Los test IDs siguen el patrón: `{página}-{elemento}-{tipo}`

Ejemplos:
- `login-submit-button` - Botón de submit en login
- `usuarios-new-user-button` - Botón de nuevo usuario en usuarios
- `dashboard-usuarios-link` - Link a usuarios desde dashboard

## Uso en Tests

```typescript
// En Page Objects
this.loginButton = page.getByTestId('login-submit-button')
  .or(page.getByRole('button', { name: /iniciar sesión/i }));

// En Tests
await page.getByTestId('login-submit-button').click();
```

## Agregar Nuevos Test IDs

Cuando agregues nuevos componentes o páginas:

1. Agrega el atributo `data-testid` al elemento
2. Actualiza este documento
3. Actualiza el Page Object correspondiente
4. Asegúrate de que el test ID siga la convención de nomenclatura

