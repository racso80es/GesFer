# Rama: docs/conceptual-domain-context

## Propósito

Anclar el **Norte Conceptual del Dominio** de GesFer a partir de la visión de negocio de Racso, consolidando:

- Un documento soberano de dominio: `docs/BUSINESS_DOMAIN.md`
- Un pilar explícito en valores: **Pragmatismo de Sector** (`Tekton/Configuration/MANIFESTO.md`)
- Una ley bloqueante de alineación: **[INTEGRIDAD CONCEPTUAL]** (`Tekton/Rules/GOLDEN_RULES.md`)

## Visión de Negocio (Racso) — Resumen Operativo

- **Naturaleza**: GesFer es una plataforma comercial para la gestión operativa del sector recuperación/chatarrerías.
- **Modelo (Agregación de Valor)**: compra minorista (pequeñas cantidades) → almacenamiento/stock → venta mayorista (grandes paquetes).
- **Estructura Dual**:
  - **Portal Admin**: orquestador global (alta de empresas, contratos marco, analítica de crecimiento).
  - **Portal Tenant**: operativa real (proveedores, clientes, stock por familias, flujo de caja).

## Alcance y Restricciones (estricto)

- **Solo documentación y reglas**: no se modifica código (`.cs`, `.js`, `.vue`), no se crean entidades/BD/servicios.
- Este movimiento es de **Soberanía Documental**: el dominio define el rumbo, la técnica obedece.

## Cambios realizados

- `docs/BUSINESS_DOMAIN.md`: creado como Norte Conceptual del Dominio.
- `Tekton/Configuration/MANIFESTO.md`: añadido pilar **Pragmatismo de Sector**.
- `Tekton/Rules/GOLDEN_RULES.md`: añadida ley **[INTEGRIDAD CONCEPTUAL]**.

## Evidencia de cumplimiento (Juez Modular)

- Pasaporte de rama (este documento) requerido por `Tekton/Rules/GOLDEN_RULES.md`.
- Reporte IA por rama: `docs/performance/IA_PERF_docs-conceptual-domain-context.md`.

## Validación

- Ejecutar: `scripts/validate-pr.ps1`

