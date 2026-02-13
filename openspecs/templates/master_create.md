# Plantilla: Creación de Maestro (Master Data)

**Referencia:** [Feature/Ticket ID]
**Tipo:** Maestro CRUD

## 1. Definición de Entidad
*   **Nombre:** `[NombreEntidad]`
*   **Tabla BD:** `[NombreTabla]`
*   **Propiedades:**
    *   `Id` (Guid, PK)
    *   `CompanyId` (Guid, FK)
    *   `Code` (String, Unique)
    *   `Name` (String, Unique)
    *   `[OtrasPropiedades]`

## 2. Checklist de Implementación

### Backend
- [ ] **Domain:** Crear Entidad en `Domain/Entities/`.
- [ ] **Persistence:** Crear Configuration en `Infrastructure/Persistence/Configurations/`.
- [ ] **DbContext:** Añadir DbSet en `ApplicationDbContext`.
- [ ] **Migration:** Generar y revisar migración.
- [ ] **DTOs:** Crear DTOs (Create, Update, Read) en `Application/DTOs/`.
- [ ] **Commands:** Implementar Create/Update/Delete Commands + Validators.
- [ ] **Queries:** Implementar GetAll/GetById Queries.
- [ ] **Controller:** Crear Controller API.
- [ ] **Tests:** Unit Tests (Logic) & Integration Tests (API).
- [ ] **Seeds:** Añadir datos demo en JSON y Seeder.

### Frontend
- [ ] **Types:** Definir interfaces TypeScript.
- [ ] **API:** Crear servicio en `lib/api/`.
- [ ] **I18n:** Añadir traducciones (es, en, ca).
- [ ] **Page:** Crear página en `app/[locale]/maestros/[entidad]/`.
- [ ] **Components:** Crear Tabla y Formulario.
- [ ] **Menu:** Añadir entrada en `Sidebar.tsx`.

## 3. Verificación
- [ ] Build Backend (`dotnet build`)
- [ ] Tests Backend (`dotnet test`)
- [ ] Build Frontend (`npm run build`)
- [ ] Navegación UI correcta
- [ ] CRUD funcional (Crear, Leer, Editar, Borrar)
