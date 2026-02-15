# Aclaración básica e impacto – Refactor Product sin Company

> **Objetivo:** Fijar el principio rector antes de volver a implementar. Los cambios del intento anterior fueron **revertidos** para alinear la implementación con esta aclaración.

---

## 1. Aclaración básica

En el dominio **Product**:

- Los datos de la **empresa** son gestionados **mediante API Admin**. Para Product, la **API Admin es la única fuente** de información de empresa.
- Product **desconoce totalmente la estructura en base de datos** de empresa; **solo conoce lo indicado por la API Admin** (DTOs, contratos, endpoints).
- Para facilitar las **seeds de datos**, hay que **cuadrar la información de IDs entre los diferentes dominios** (Admin y Product usan los mismos identificadores donde corresponda).

**Resumen:** Product no lee ni escribe la tabla `Companies` ni conoce su esquema. Toda la información de empresa se obtiene **exclusivamente vía Admin API**.

**Admin no expone todas las empresas:** como mucho una (la que corresponde al contexto). Product no necesita conocer más empresas; solo la que le pertoca (por nombre en login, por id en Mi Organización).

---

## 2. Implicaciones directas

| Principio | Implicación en Product |
|-----------|-------------------------|
| Única fuente = Admin API | No usar `DbSet<Company>`, no usar SQL directo a `Companies`, no mapear entidad Company en Product. |
| Desconoce estructura en BD | No asumir columnas, tablas ni esquema de empresa; solo lo que expone la API (por ejemplo `AdminCompanyDto`, endpoints de consulta). |
| Cuadrar IDs en seeds | Los seeds de Product (demo-data, test-data) deben usar los mismos `CompanyId` que Admin crea o expone; definir proceso/orden (p. ej. Admin seeds primero, Product seeds después con IDs conocidos). |

---

## 3. Análisis de afectaciones por área

### 3.1 Autenticación (login)

- **Hoy (antes del refactor):** Se resuelve empresa por nombre contra BD (tabla `Companies`) y luego usuario por `CompanyId` + username.
- **Con la aclaración:** Resolver empresa por nombre **solo vía Admin API** (p. ej. endpoint tipo “get company by name” o “resolve company id from name”). Product no puede hacer `SELECT Id FROM Companies WHERE Name = ...`.
- **Afectación:** `AuthService` (y opcionalmente login en API) debe usar `IAdminApiClient` (o equivalente) para obtener `CompanyId` dado el nombre de empresa. Admin debe exponer un contrato que lo permita.

### 3.2 Seeds (JsonDataSeeder, LoadDemoData, SeedMasterData / test-data)

- **Hoy:** Se obtienen “valid company IDs” leyendo de la tabla `Companies` (p. ej. `SELECT Id FROM Companies`) para filtrar usuarios/proveedores/clientes a sembrar.
- **Con la aclaración:** Product **no** puede leer la tabla `Companies`. Los IDs de empresa válidos deben venir de:
  - **Opción A:** Llamada a Admin API (p. ej. “listar company IDs” o “seed context” con IDs creados por Admin).
  - **Opción B:** Configuración/descriptor de seeds acordado entre dominios (p. ej. fichero o variable con IDs que Admin ya ha creado).
  - **Opción C:** Asumir que los `CompanyId` que aparecen en los JSON de Product ya existen (creados por Admin seeds) y no validar existencia desde Product; solo validar coherencia de IDs entre ficheros (cuadrar IDs).
- **Afectación:** Eliminar cualquier acceso a `Companies` desde Product. Definir en el plan si seeds de Product llaman a Admin API, leen configuración o solo confían en IDs previamente acordados.

### 3.3 DbInitializer (EnsureAdminUser, smoke test)

- **Hoy:** Se comprueba que la empresa del usuario admin exista (p. ej. consultando `Companies` o navegación `User.Company`).
- **Con la aclaración:** Esa comprobación debe hacerse **vía Admin API** (p. ej. “existe company con id X”) o eliminarse y confiar en que Admin es la fuente de verdad.
- **Afectación:** Sustituir cualquier lectura a BD de empresa por una llamada a Admin API o relajar la validación (solo comprobar que `CompanyId` no sea vacío, sin comprobar existencia en BD desde Product).

### 3.4 Handlers que validan “empresa existe”

- **CreateUser, CreateCustomer, CreateSupplier:** Hoy pueden validar existencia de empresa contra `_context.Companies` o equivalente.
- **Con la aclaración:** Validar existencia **solo vía Admin API** (p. ej. `GetCompanyAsync(companyId)` y comprobar que la respuesta no sea 404).
- **Afectación:** Inyectar `IAdminApiClient` donde haga falta y reemplazar comprobaciones contra BD por llamadas a la API.

### 3.5 Handlers que devuelven “nombre de empresa” (CompanyName)

- **GetUserById, GetAllUsers, UpdateUser, CreateUser:** Construyen DTOs con `CompanyName`. Hoy se puede obtener con `Include(Company)` o SQL a `Companies`.
- **Con la aclaración:** Obtener el nombre **solo vía Admin API** (p. ej. `IAdminApiClient.GetCompanyAsync(companyId)` y usar `.Name` del DTO).
- **Afectación:** Llamar a Admin API para resolver nombre por `CompanyId`; tener en cuenta caché o batch si hay muchos usuarios en una misma empresa para no hacer N llamadas.

### 3.6 Dashboard (TotalCompanies)

- **Hoy:** Puede usar `CountAsync()` sobre `Companies` o equivalente.
- **Decisión:** **Quitar la métrica** en Product. Admin es SSOT de Companies; Admin obtiene TotalCompanies de su propio contexto en su dashboard.
- **Afectación:** Eliminada propiedad `TotalCompanies` de `DashboardSummaryDto` y de la respuesta de `DashboardController` en Product. Admin no usa este campo desde Product (ya usa su propio `_context.Companies.CountAsync()`).

### 3.7 Dominio y persistencia

- **Entidad Company:** Product no tiene entidad `Company` ni navegación `User.Company`, etc. (ya previsto en la spec).
- **Tabla Companies:** Product **no** la crea, no la altera y **no** la lee. Las FKs (`CompanyId`) en tablas de Product apuntan a una tabla que es responsabilidad de Admin (creada por migraciones de Admin o por un esquema compartido donde Admin es dueño del recurso).
- **Afectación:** En Product: sin `DbSet<Company>`, sin configuraciones EF que mapeen Company, sin SQL ni uso de esa tabla. Migraciones de Product no deben crear ni modificar `Companies`.

### 3.8 Cuadre de IDs entre dominios (seeds)

- **Objetivo:** Que los IDs de empresa usados en seeds de Product coincidan con los que Admin crea o expone.
- **Opciones:**
  - Mismo fichero de IDs compartido (p. ej. constantes o JSON de “empresas de demo”) usado por Admin al sembrar y por Product en demo-data/test-data.
  - Admin seeds primero y expone los IDs creados (API o artefacto); Product seeds consumen esa información.
  - Definir en documentación los GUIDs estándar de demo/test y que ambos dominios los usen.
- **Afectación:** Incluir en el plan de implementación un apartado explícito de “seeds y cuadre de IDs” (orden de ejecución, origen de los IDs en Product, y dónde se documentan).

---

## 4. Resumen: qué evitar y qué usar en Product

| Evitar en Product | Usar en Product |
|-------------------|-----------------|
| `DbSet<Company>` | Solo `CompanyId` (valor) en entidades; FK en BD sin navegación |
| Cualquier SQL o EF sobre tabla `Companies` | `IAdminApiClient.GetCompanyAsync(id)`, `GetCompanyByNameAsync(name)` (una empresa; Admin no expone listado de todas). |
| Asumir esquema o columnas de Companies | Contratos y DTOs de Admin API (ej. `AdminCompanyDto`) |
| Seeds que lean `Companies` para validar IDs | Seeds que usen IDs obtenidos de Admin API o de configuración/ficheros acordados entre dominios |

---

## 5. Próximos pasos recomendados (antes de volver a implementar)

1. **Admin API:** Definir (o revisar) los endpoints que Product necesitará:
   - Resolver company por nombre (login).
   - Obtener empresa por id (nombre para DTOs, validar existencia).
   - Si aplica: listar IDs de empresas, contar empresas, “seed context”.
2. **Seeds:** Decidir flujo y cuadre de IDs (quién crea qué, orden, de dónde toma Product los `CompanyId` válidos) y documentarlo en el plan.
3. **Plan de implementación:** Actualizar el plan con la regla “Product no conoce la BD de empresa; solo Admin API” y las afectaciones anteriores, y luego reanudar la implementación por fases (dominio, infraestructura, aplicación, API, seeds, tests).

---

*Documento generado tras revertir los cambios del primer intento de implementación para alinear con la aclaración.*
