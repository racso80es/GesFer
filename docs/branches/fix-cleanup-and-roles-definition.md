# fix/cleanup-and-roles-definition — Purificación y soberanía de roles

## Objetivo

Consolidar el modelo conceptual de roles sustituyendo la noción de “Tenant” por **Administrador de la Empresa** (soberano de su instancia), y eliminar ruido de diagnóstico (logs temporales / debug / código muerto) introducido durante el fix de login en Frontend y Backend.

## Enfoque (permisos)

- **No hay jerarquías fijas impuestas por GesFer**: el sistema valida acciones de forma granular por derecho requerido.
- La **gestión y asignación de derechos** es potestad exclusiva de cada empresa (vía su Administrador de la Empresa).

## Alcance

- Documentación: `docs/BUSINESS_DOMAIN.md`, `Tekton/Rules/GOLDEN_RULES.md`
- Limpieza de ruido:
  - Frontend: `Cliente/` (runtime y tests si aplica)
  - Backend: `Api/` (especialmente zonas de login/telemetría si aplica)
  - Docs: eliminación de trazas temporales/diagnósticos no soberanos en `docs/`

## Resultado esperado

- El término **Tenant** queda reemplazado (conceptualmente) por **Administrador de la Empresa** en la documentación soberana del dominio.
- Se define explícitamente la triada de roles:
  - Administrador Global (nosotros)
  - Administrador de la Empresa
  - Usuarios Operativos (perfiles operativos definidos por empresa)
- Se elimina el ruido (logs de consola, instrumentación temporal y/o código muerto) asociado al fix de login.

