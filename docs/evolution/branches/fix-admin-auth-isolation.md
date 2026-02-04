# fix/admin-auth-isolation — Bifurcación estructural de dominio (Admin Auth Isolation)

## Objetivo

Desacoplar el dominio **Admin** del dominio **Cliente (multi‑tenant)**, garantizando que:

- El **login administrativo** no contiene ni depende de lógica de empresa/tenant.
- Los DTOs de autenticación **Admin** no reutilizan DTOs del dominio Cliente.
- El estado/almacenamiento de auth **Admin** no “contamina” el namespace de tenants.

## Alcance

- Frontend (`Cliente`): Login Admin, estado/almacenamiento asociado y llamadas a API Admin.
- Backend (`Api/src/Api`): DTOs/contratos y endpoint de login Admin.

## Ley aplicada

Se aplica la **LEY DE INVARIANZA Y SOBERANÍA DE DOMINIO** (ver `Tekton/Rules/GOLDEN_RULES.md`):

- Admin y Cliente son dominios **no heredables** en autenticación.
- Lo único compartible entre dominios es **infraestructura neutral** (componentes UI puros, utilidades sin semántica de dominio).

## Cambios realizados (resumen)

- DTOs Admin: `AdminLoginRequest` como contrato estándar de identidad global (no tenant).
- Frontend Admin: formulario sin campo empresa y sin acoplamiento al contexto/almacenamiento de Cliente.
- Aislamiento de estado: Admin y Cliente usan namespaces independientes para persistencia.

## Validación (Juez)

- `scripts/validate-pr.ps1`

