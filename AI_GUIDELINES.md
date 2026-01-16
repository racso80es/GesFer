# AI Guidelines - Fuente de Verdad

**Versión:** 1.0  
**Última actualización:** 2026-01-16  
**Propósito:** Manual de operación para IA y desarrolladores en el repositorio GesFer

---

## 📋 Índice

1. [Protocolo de Calidad](#protocolo-de-calidad)
2. [Arquitectura de UI](#arquitectura-de-ui)
3. [Integridad de Datos](#integridad-de-datos)
4. [Estandarización de Tests](#estandarización-de-tests)
5. [Reglas de Commit](#reglas-de-commit)
6. [Estructura de Rutas](#estructura-de-rutas)
7. [Mantenimiento de Reglas](#mantenimiento-de-reglas)

---

## 🛡️ Protocolo de Calidad

### Validación Obligatoria Pre-Commit

**REGLAS ESTRICTAS:**

1. **Antes de cualquier commit**, es OBLIGATORIO ejecutar con éxito el script `scripts/validate-commit.sh` (o `scripts/validate-commit.ps1` en Windows).

2. **Si el script falla**, DEBES corregir los errores de forma autónoma hasta que sea exitoso. **NO se permite hacer commit con validaciones fallidas**.

3. **El script valida:**
   - ✅ Compilación del Backend (`dotnet build`)
   - ✅ Lint del Frontend (`npm run lint`)
   - ✅ Tests unitarios del Backend (excluyendo tests de integración)
   - ✅ Tests unitarios del Frontend

4. **Condición de éxito:** El script DEBE retornar código de salida 0 y mostrar "Todas las validaciones pasaron".

### Ejecución Manual

```bash
# Linux/Mac
bash scripts/validate-commit.sh

# Windows (PowerShell)
powershell -ExecutionPolicy Bypass -File scripts\validate-commit.ps1
```

---

## 🎨 Arquitectura de UI

### Componentes Compartidos (`Cliente/components/shared/`)

**PRINCIPIOS FUNDAMENTALES:**

1. **Inmutabilidad:** Los componentes en `shared/` son **puros e inmutables**. No deben ser personalizados para casos específicos (como logs o usuarios).

2. **Variaciones mediante Props:** Cualquier variación de comportamiento o estilo DEBE pasarse mediante props. Si necesitas una variante específica, extiende el componente mediante composición, NO modifiques el componente base.

3. **Prohibición de Elementos Nativos:** 
   - ❌ **PROHIBIDO** usar elementos HTML nativos (`<button>`, `<input>`, `<table>`) cuando existe un componente equivalente en `shared/`.
   - ✅ **OBLIGATORIO** usar componentes de `shared/` (`Button`, `Input`, `DataTable`, `ModalBase`).

### Componentes Disponibles

- **Button.tsx** - Envuelve `@/components/ui/button` con soporte para `data-testid`
- **Input.tsx** - Envuelve `@/components/ui/input` con soporte para `data-testid`
- **ModalBase.tsx** - Modal reutilizable con soporte para `data-testid`
- **DataTable.tsx** - Tabla de datos genérica con paginación y filas expandibles

### Sincronización con Fuente

Los componentes `shared/` envuelven componentes de `@/components/ui` (shadcn/ui). **NO se modifican directamente**. Cualquier cambio en el comportamiento base debe hacerse en el componente de shadcn/ui, y luego propagarse a través del wrapper.

### Ejemplo de Uso Correcto

```tsx
// ✅ CORRECTO: Usar componente shared
import { Button } from "@/components/shared/Button";

<Button 
  variant="destructive" 
  onClick={handleDelete}
  data-testid="shared-button-delete-user"
>
  Eliminar
</Button>

// ❌ INCORRECTO: Usar elemento nativo
<button onClick={handleDelete}>Eliminar</button>

// ❌ INCORRECTO: Modificar componente shared para caso específico
// NO crear componentes como DeleteUserButton.tsx que extiendan Button
```

---

## 🔗 Integridad de Datos

### Validación de Formularios con Zod

**REGLAS OBLIGATORIAS:**

1. **No usar validaciones hardcoded:** Todas las validaciones de formularios DEBEN usar esquemas Zod (`z.object()`) que reflejen exactamente las restricciones del Backend.

2. **Sincronización Backend-Frontend:** Los esquemas Zod DEBEN estar en `Cliente/lib/validations/` y reflejar las mismas reglas que las entidades del Backend (por ejemplo, `UserConfiguration.cs`, `CompanyConfiguration.cs`).

3. **Ejemplo de esquema Zod:**
   ```typescript
   // Cliente/lib/validations/user.ts
   export const createUserSchema = z.object({
     username: z.string().min(1, "El nombre de usuario es obligatorio").max(100, "No puede exceder 100 caracteres"),
     // ... refleja las validaciones de UserConfiguration.cs en el Backend
   });
   ```

4. **Uso en formularios:** Los componentes de formulario (`UserForm`, `CompanyForm`, etc.) DEBEN usar estos esquemas para validar antes de enviar al Backend.

### Patrón Listado-Formulario

**ESTÁNDAR OBLIGATORIO:**

1. **Listado (`DataTable`):** Todas las entidades principales DEBEN usar `shared/DataTable` para mostrar listados con:
   - Búsqueda integrada
   - Filtros opcionales
   - Acciones (editar, eliminar) con `data-testid` estandarizados

2. **Formularios Reutilizables:** Crear componentes de formulario en `Cliente/components/admin/` o `Cliente/components/(client)/` que:
   - Usen componentes `shared/Input`, `shared/Button`
   - Validen con esquemas Zod
   - Tengan `data-testid` en todos los campos
   - Manejen estados de carga y errores

3. **Modales (`ModalBase`):** Los formularios de creación/edición DEBEN estar dentro de `shared/ModalBase`.

### Componentes de Formulario Disponibles

- **`Cliente/components/admin/UserForm.tsx`** - Formulario de usuario (crear/editar) con validación Zod
- **`Cliente/components/admin/CompanyForm.tsx`** - Formulario de empresa (crear/editar) con validación Zod

---

## 🔗 Integridad de Datos

### Sincronización Backend ↔ Frontend

**REQUISITO OBLIGATORIO:**

1. **Antes de desarrollar o refactorizar**, analiza los modelos C# del Backend en `Api/src/domain/Entities/` y DTOs en `Api/src/application/DTOs/`.

2. **Define interfaces TypeScript** en el Frontend que reflejen **exactamente** las propiedades del Backend.

3. **Ubicación de tipos:**
   - Tipos de API: `Cliente/lib/types/api.ts`
   - Tipos específicos de módulo: `Cliente/lib/api/[modulo].ts` (ej: `logs.ts`)

### Regla de Unificación

**Cualquier nueva entidad o propiedad agregada al Backend DEBE:**

1. ✅ Reflejarse inmediatamente en las interfaces TypeScript del Frontend
2. ✅ Mantener la misma estructura de nombres (PascalCase en C#, camelCase en TypeScript)
3. ✅ Incluir los mismos campos opcionales/requeridos
4. ✅ Usar tipos equivalentes (Guid → string, DateTime → string ISO, etc.)

### Validación de Tipos

Los componentes `shared/` DEBEN usar estos tipos sincronizados para garantizar la integridad de los datos. Si un tipo no coincide con el Backend, **NO proceder con el desarrollo** hasta sincronizarlo.

---

## 🧪 Estandarización de Tests

### Selectores data-testid

**NOMENCLATURA OBLIGATORIA:**

```
shared-[nombre-componente]-[acción]
```

**Ejemplos:**
- `shared-button-confirm`
- `shared-button-delete-user`
- `shared-input-datetime-from`
- `shared-input-datetime-to`
- `shared-modal-purge-logs`
- `shared-datatable-logs`

### Reglas de Blindaje

1. **Todos los componentes en `shared/` DEBEN incluir el atributo `data-testid`**.

2. **Generación automática:** Si no se proporciona explícitamente, el componente debe generar un `data-testid` basado en sus props (variant, type, etc.).

3. **Desacoplamiento:** Los tests NO deben depender de la estructura HTML (selectores como `button`, `.class-name`, `#id`). **SIEMPRE usar `data-testid`**.

### En Page Object Models

Los Page Objects (`tests/page-objects/`) DEBEN usar `page.getByTestId()` en lugar de selectores CSS o XPath:

```typescript
// ✅ CORRECTO
this.applyFiltersButton = page.getByTestId('shared-button-apply-filters');

// ❌ INCORRECTO
this.applyFiltersButton = page.locator('button:has-text("Aplicar")');
```

---

## 📝 Reglas de Commit

### Formato de Mensaje Obligatorio

Cuando se solicite un commit, el formato DEBE ser:

```
[Título proporcionado]

Summary: [Breve frase técnica del cambio]

Technical Changes:
- [ ] Archivo/Modulo modificado 1
- [ ] Archivo/Modulo modificado 2
- [ ] ...

Validation: ✅ validate-commit.sh passed successfully
```

### Protocolo de Ejecución

1. **Ejecutar validación:** `scripts/validate-commit.sh` (o `.ps1` en Windows)
2. **Si falla:** Corregir errores hasta que pase
3. **Si pasa:** Generar mensaje con el formato indicado
4. **Realizar commit** con ese mensaje

### Ejemplo

```bash
# Usuario solicita: "Haz commit: Refactorización de componente de usuario"

# 1. Ejecutar validación
bash scripts/validate-commit.sh

# 2. Si pasa, commit:
git commit -m "Refactorización de componente de usuario

Summary: Migración del componente UserForm para usar componentes shared inmutables.

Technical Changes:
- [x] Cliente/components/usuarios/user-form.tsx
- [x] Cliente/components/shared/Button.tsx
- [x] Cliente/components/shared/Input.tsx
- [x] Cliente/tests/page-objects/UserFormPage.ts

Validation: ✅ validate-commit.sh passed successfully"
```

---

## 🗂️ Estructura de Rutas

### Grupos de Rutas

El proyecto utiliza **route groups** de Next.js para separar rutas administrativas y de cliente:

- **`app/(admin)/`** - Rutas administrativas (requieren autenticación Admin)
- **`app/(client)/`** - Rutas de cliente (usuarios finales)

### Layouts Independientes

1. **`app/(admin)/layout.tsx`**
   - Contiene autenticación y protección de rutas
   - Integra `SidebarProvider` y `AdminLayoutComponent`
   - Usa `Sidebar` con estado colapsable/expandible

2. **`app/(client)/layout.tsx`**
   - Layout limpio, sin sidebar
   - Pensado para usuarios finales

3. **`app/layout.tsx` (RootLayout)**
   - **Minimalista:** Solo fuentes y etiquetas básicas (html, body)
   - No contiene lógica visual específica
   - La lógica visual vive en los layouts de grupo

### Sidebar Context

El estado del sidebar (colapsado/expandido) se maneja mediante `SidebarContext` (`contexts/sidebar-context.tsx`):

- **Hook:** `useSidebar()` proporciona `isCollapsed`, `toggleSidebar()`, `collapseSidebar()`, `expandSidebar()`
- **Padding dinámico:** El contenido principal ajusta su padding según `isCollapsed`

---

## 🔄 Mantenimiento de Reglas

### Actualización del Manual

**REGLAS ESTRICTAS:**

1. **Cualquier nueva norma arquitectónica acordada** DEBE quedar reflejada en este archivo **ANTES de cerrar la tarea**.

2. **Formato de actualización:**
   - Agregar la nueva regla en la sección correspondiente
   - Actualizar el campo "Última actualización" en el encabezado
   - Si es una regla mayor, agregar una nueva sección

3. **Antes de commit:** Verificar que todas las reglas mencionadas en `AI_GUIDELINES.md` estén implementadas en el código.

4. **Consistencia:** Este archivo es la **fuente de verdad única**. Si hay contradicciones con otros documentos, este archivo tiene prioridad.

### Proceso de Adición de Regla

1. Acordar la nueva regla con el usuario
2. Implementarla en el código
3. Actualizar `AI_GUIDELINES.md` con la nueva regla
4. Validar con `scripts/validate-commit.sh`
5. Commit siguiendo el protocolo establecido

---

## 📚 Referencias Rápidas

### Ubicaciones Clave

- **Componentes shared:** `Cliente/components/shared/`
- **Componentes UI base:** `Cliente/components/ui/`
- **Tipos TypeScript:** `Cliente/lib/types/api.ts`
- **Scripts de validación:** `scripts/validate-commit.sh` / `.ps1` / `.bat`
- **Tests E2E:** `Cliente/tests/e2e/`
- **Page Objects:** `Cliente/tests/page-objects/`
- **Contextos:** `Cliente/contexts/`

### Comandos Útiles

```bash
# Validación completa
bash scripts/validate-commit.sh

# Compilación Backend
cd Api && dotnet build

# Lint Frontend
cd Cliente && npm run lint

# Tests Frontend
cd Cliente && npm test -- --testPathPattern="__tests__"

# Tests Backend
cd Api && dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

---

## ⚠️ Prohibiciones Críticas

1. ❌ **NUNCA** hacer commit sin que `validate-commit.sh` pase exitosamente
2. ❌ **NUNCA** usar elementos HTML nativos cuando existe componente en `shared/`
3. ❌ **NUNCA** modificar componentes `shared/` para casos específicos
4. ❌ **NUNCA** crear tipos TypeScript sin sincronizarlos con el Backend
5. ❌ **NUNCA** usar selectores CSS/XPath en tests, siempre `data-testid`
6. ❌ **NUNCA** agregar lógica visual al RootLayout (`app/layout.tsx`)

---

## ✅ Checklist de Desarrollo

Antes de considerar una tarea completa:

- [ ] Script de validación pasa exitosamente
- [ ] Componentes usan elementos de `shared/` (no nativos)
- [ ] Tipos TypeScript sincronizados con Backend
- [ ] Componentes incluyen `data-testid` con nomenclatura correcta
- [ ] Tests actualizados para usar `data-testid`
- [ ] `AI_GUIDELINES.md` actualizado si se agregaron nuevas reglas
- [ ] Commit realizado siguiendo el formato establecido

---

**Última revisión:** 2026-01-16  
**Próxima revisión:** Cuando se establezcan nuevas reglas arquitectónicas
