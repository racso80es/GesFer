# Objetivo: Separación de Gestión de Empresas (Product -> Admin)

## Propósito
Transferir la autoridad y gestión de las entidades "Empresa" (Company) del dominio de Producto al dominio de Administración. El objetivo es centralizar la creación y mantenimiento de clientes en el panel de administración, permitiendo a los usuarios de Producto únicamente la edición de sus propios datos corporativos.

## Alcance

### 1. Backend: Admin (Source of Truth)
- **Migración de Lógica:** Mover controladores, comandos y DTOs relacionados con `Company` desde `Product.Back` a `Admin.Back`.
- **Nuevo Endpoint:** Crear `CompanyController` en `Admin.Back` con operaciones CRUD completas.
- **API Inter-servicio:** Exponer endpoints seguros para que `Product.Back` pueda consultar y actualizar datos de empresas.
- **Seguridad:** Implementar autenticación mediante Shared Secret (API Key) o JWT administrativo para las llamadas entre microservicios.

### 2. Backend: Product (Consumer)
- **Eliminación de Gestión:** Eliminar la capacidad de crear o borrar empresas directamente.
- **Nuevo Controlador:** Implementar `MyCompanyController` que actúe como proxy hacia `Admin.Back`.
- **Integración:** Utilizar `IAdminApiClient` para obtener y actualizar los datos de la empresa del usuario autenticado.

### 3. Frontend: Admin
- **Nueva Funcionalidad:** Implementar pantallas de gestión de empresas (Listado, Crear, Editar, Eliminar).
- **Menú:** Añadir acceso a "Empresas" en la navegación principal.

### 4. Frontend: Product
- **Refactorización:** Eliminar pantallas de gestión de empresas general.
- **Mi Empresa:** Crear una vista de "Mi Empresa" (solo edición de detalles permitidos) conectada al nuevo endpoint de `Product.Back`.

## Restricciones y Estándares
- **Nomenclatura:** El código debe utilizar estrictamente el término `Company`. La interfaz de usuario mantendrá la terminología actual ("Empresa"/"Organización").
- **Datos:** El estado de la empresa (Activo/Inactivo) será gestionado exclusivamente por Admin. Product solo podrá editar información operativa (dirección, contacto, etc.).
- **Autenticación:** Las llamadas entre `Product.Back` y `Admin.Back` deben estar aseguradas.

## Seeds (Admin SSOT)

- **Companies:** Definidos en `Admin/Back/Infrastructure/Data/Seeds/companies.json` y cargados por `AdminJsonDataSeeder.SeedCompaniesAsync()`. Product ya no inserta companies; obtiene los IDs existentes en BD.
- **Orden en BD compartida:** Ejecutar seeds de Admin (companies, admin-users) **antes** que los de Product, para que la tabla `Companies` esté poblada. Ver `Admin/Back/Infrastructure/Data/Seeds/README.md`.

## Referencias
- `openspecs/actions/feature.md`
