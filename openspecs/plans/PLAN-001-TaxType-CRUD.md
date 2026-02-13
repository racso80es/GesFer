# PLAN-001: Implementación CRUD TaxType

## Fase 1: Backend (Dominio e Infraestructura)
1.  **Entidad:** Crear `src/Product/Back/Domain/Entities/TaxType.cs`.
2.  **Configuración EF:** Crear `src/Product/Back/Infrastructure/Persistence/Configurations/TaxTypeConfiguration.cs`.
3.  **DbContext:** Añadir `DbSet<TaxType>` en `ApplicationDbContext`.
4.  **Migración:** Generar migración EF Core.

## Fase 2: Backend (Aplicación y API)
1.  **DTOs:** Definir `TaxTypeDto`, `CreateTaxTypeDto`, `UpdateTaxTypeDto`.
2.  **CQRS - Commands:**
    *   `CreateTaxTypeCommand` + Validator + Handler
    *   `UpdateTaxTypeCommand` + Validator + Handler
    *   `DeleteTaxTypeCommand` + Handler
3.  **CQRS - Queries:**
    *   `GetTaxTypesQuery` (Listado)
    *   `GetTaxTypeByIdQuery` (Detalle)
4.  **Controller:** Implementar `TaxTypesController` con endpoints REST.

## Fase 3: Testing y Seeding
1.  **Unit Tests:** Tests para Handlers y Validadores.
2.  **Integration Tests:** Tests para Endpoints (Happy path y errores).
3.  **Seeding:** Actualizar `demo-data.json` y `JsonDataSeeder.cs`.

## Fase 4: Frontend
1.  **API Client:** Crear `tax-types-api.ts`.
2.  **Tipos:** Definir interfaces TypeScript.
3.  **Traducciones:** Añadir claves i18n.
4.  **Componentes:**
    *   `TaxTypeList.tsx`: Tabla de gestión.
    *   `TaxTypeForm.tsx`: Formulario (Hook Form + Zod).
5.  **Rutas:** Página `app/[locale]/maestros/tipotasa/page.tsx`.
6.  **Navegación:** Actualizar `Sidebar.tsx`.

## Fase 5: Documentación
1.  Generar plantilla `openspecs/templates/master_create.md`.
