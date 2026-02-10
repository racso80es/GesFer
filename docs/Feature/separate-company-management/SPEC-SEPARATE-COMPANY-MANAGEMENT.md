# Especificación Técnica: Separación de Gestión de Empresas

## 1. Contexto
Actualmente, la gestión de empresas (clientes) reside en el dominio de Producto (`Product.Back`). El objetivo es transferir esta responsabilidad al dominio de Administración (`Admin.Back`), convirtiéndolo en la Fuente de Verdad (SSOT). `Product.Back` consumirá los datos de empresa a través de una API interna segura expuesta por `Admin.Back`.

## 2. Arquitectura

### 2.1 Admin.Back (Proveedor / SSOT)
- **Responsabilidad:** Crear, Leer, Actualizar y Borrar (CRUD) entidades `Company`.
- **Base de Datos:** Mantiene la tabla `Companies` (o esquema equivalente).
- **API:**
  - `GET /api/companies`: Listar todas (Admin only).
  - `GET /api/companies/{id}`: Detalle (Admin/System).
  - `POST /api/companies`: Crear (Admin only).
  - `PUT /api/companies/{id}`: Actualizar (Admin/System).
  - `DELETE /api/companies/{id}`: Soft delete (Admin only).
- **Seguridad:** Autenticación JWT con Rol `Admin` para operaciones de gestión. Autenticación por `SharedSecret` (Header `X-Internal-Secret`) para peticiones desde `Product.Back`.

### 2.2 Product.Back (Consumidor)
- **Responsabilidad:** Permitir al usuario autenticado ver y editar *su propia* empresa.
- **Dependencia:** Elimina acceso directo a la tabla `Companies`. Usa `IAdminApiClient` para comunicarse con `Admin.Back`.
- **API:**
  - `GET /api/my-company`: Obtiene datos de la empresa del usuario actual (vía Admin API).
  - `PUT /api/my-company`: Actualiza datos permitidos (vía Admin API).
- **Lógica:** El `MyCompanyController` extrae el `CompanyId` del token del usuario y llama a `Admin.Back`.

## 3. Cambios en Base de Datos
- **Migración:** Si las tablas ya existen en una BD compartida, se debe asegurar que `Admin.DbContext` tenga los `DbSet<Company>` y `Product.DbContext` *no* los tenga (o sean solo lectura/referencia si es necesario por FKs, aunque idealmente se desacopla).
- *Nota:* Asumiremos por ahora que comparten la instancia física de BD pero se separan lógicamente los contextos. `Product` no debería escribir en `Companies` directamente.

## 4. Interfaces de Usuario
- **Admin Front:**
  - Nueva sección "Empresas" en el menú lateral.
  - Vistas: Listado (con paginación/filtro), Creación, Edición, Borrado.
- **Product Front:**
  - Eliminar sección "Empresas" del menú principal.
  - Nueva sección "Mi Organización" (o en Perfil) para editar datos propios.

## 5. Plan de Pruebas
- **Unitarias:** Tests para `CompanyService` (Admin) y `AdminApiClient` (Product).
- **Integración:** Verificar que `Product.Back` puede autenticarse con `Admin.Back` usando el secreto compartido.
- **E2E:** Flujo completo: Admin crea empresa -> Usuario se loguea -> Usuario edita empresa -> Admin ve cambios.

## 6. Consideraciones de Auditoría
- Todos los cambios en `Company` deben generar registros de auditoría en `Admin.Back`.
- `Product.Back` debe enviar el ID del usuario que realiza la acción (header `X-User-Id` o similar) para mantener la trazabilidad en `Admin`.
