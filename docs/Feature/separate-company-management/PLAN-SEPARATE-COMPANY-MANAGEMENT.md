# Plan de Implementación: Separación de Gestión de Empresas

## Pasos

### 1. Migración de Código Backend (Admin)
- [ ] **Mover DTOs:** Copiar/Crear `CreateCompanyDto`, `UpdateCompanyDto`, `CompanyDto` en `src/Admin/Back/Application/DTOs`.
- [ ] **Implementar Servicios:** Crear `ICompanyService` y su implementación en `src/Admin/Back/Application/Services`.
- [ ] **Crear Controlador:** Añadir `CompanyController` a `src/Admin/Back/Api/Controllers` con acciones CRUD.
- [ ] **Configurar Autenticación:** Asegurar que el controlador valida JWT de Admin y/o `X-Internal-Secret` para `Get` y `Update`.

### 2. Implementación de Cliente Backend (Product)
- [ ] **Crear Cliente HTTP:** Implementar `AdminApiClient` (interfaz `IAdminApiClient`) en `src/Product/Back/Infrastructure`.
- [ ] **Configurar HttpClient:** Registrar `HttpClient` con la `BaseUrl` de Admin y el header de autenticación.
- [ ] **Crear Controlador Proxy:** Implementar `MyCompanyController` en `src/Product/Back/Api/Controllers` que usa `IAdminApiClient` para obtener y actualizar la propia empresa.
- [ ] **Limpieza:** Eliminar el antiguo `CompanyController` y referencias directas a `DbContext.Companies` (si es posible sin romper FKs).

### 3. Frontend Admin (Nueva Funcionalidad)
- [ ] **Añadir Rutas:** Configurar rutas para `/companies` en `src/Admin/Front/app`.
- [ ] **Vista Listado:** Crear `src/Admin/Front/app/companies/page.tsx` con tabla paginada.
- [ ] **Vista Creación/Edición:** Crear formularios reutilizables para `Company`.
- [ ] **Integración API:** Implementar llamadas a `DELETE /api/companies/{id}` y `POST/PUT`.

### 4. Frontend Product (Refactorización)
- [ ] **Eliminar Gestión:** Borrar carpeta `src/Product/Front/app/companies`.
- [ ] **Vista Mi Empresa:** Crear `src/Product/Front/app/my-company/page.tsx` para edición de perfil.
- [ ] **Integración API:** Conectar a `GET /api/my-company` y `PUT /api/my-company`.

### 5. Verificación y Pruebas
- [ ] **Build:** Asegurar que ambos backends compilan sin errores.
- [ ] **Tests:** Ejecutar tests unitarios de `Admin.Application`.
- [ ] **Manual:** Verificar flujo completo de creación en Admin y edición en Product.
