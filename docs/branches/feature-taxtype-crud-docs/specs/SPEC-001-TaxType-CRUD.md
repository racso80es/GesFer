# SPEC-001: CRUD Tipo de Tasa (TaxType)

## 1. Resumen
Implementación de un sistema CRUD (Crear, Leer, Actualizar, Eliminar) para la entidad "Tipo de Tasa" (TaxType) dentro del módulo de Producto. Este maestro permitirá gestionar los diferentes tipos de impuestos aplicables (ej. IVA 21%, IVA 10%, Exento, etc.).

## 2. Alcance
*   **Backend (Product.Back):**
    *   Nueva entidad `TaxType`.
    *   Persistencia en base de datos (EF Core).
    *   API Endpoints (Controllers) y lógica de negocio (CQRS).
    *   Tests unitarios e integración.
    *   Datos semilla (Seed data) para impuestos españoles estándar.
*   **Frontend (Product.Front):**
    *   Nuevo ítem de menú "Maestros > Tipo de Tasa".
    *   Pantalla de listado con acciones.
    *   Formulario de creación/edición.
    *   Integración con API.

## 3. Especificaciones Funcionales

### 3.1 Entidad: TaxType
| Campo       | Tipo    | Restricciones                                  | Descripción                                      |
|-------------|---------|------------------------------------------------|--------------------------------------------------|
| Id          | GUID    | PK                                             | Identificador único.                             |
| CompanyId   | GUID    | FK (Company)                                   | Empresa propietaria del dato.                    |
| Code        | String  | Obligatorio, Único por Empresa, Max 10 chars   | Código interno (ej. "IVA21").                    |
| Name        | String  | Obligatorio, Único por Empresa, Max 50 chars   | Nombre descriptivo (ej. "IVA General 21%").      |
| Description | String  | Opcional, Max 255 chars                        | Descripción detallada.                           |
| Value       | Decimal | Obligatorio, >= 0                              | Valor porcentual (ej. 21.00 para 21%).           |

### 3.2 Reglas de Negocio
1.  **Unicidad:** El par `(CompanyId, Code)` debe ser único. El par `(CompanyId, Name)` debe ser único.
2.  **Multitenancy:** Los datos deben estar aislados por `CompanyId`.
3.  **Valor:** El valor se almacena como porcentaje absoluto (21.00), no como fracción (0.21).

### 3.3 Interfaz de Usuario
*   **Menú:** Añadir sección "Maestros" si no existe, y dentro "Tipo de Tasa" (`/maestros/tipotasa`).
*   **Listado:** Tabla mostrando Código, Nombre, Valor (con símbolo %) y acciones (Editar, Eliminar).
*   **Creación/Edición:** Modal o página dedicada con validación de campos obligatorios.

## 4. Datos Iniciales (Seed)
Se deben generar automáticamente los siguientes registros para las empresas de demo:
*   IVA General (21%)
*   IVA Reducido (10%)
*   IVA Superreducido (4%)
*   Exento (0%)
