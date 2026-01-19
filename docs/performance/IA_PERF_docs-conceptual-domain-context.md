# IA_PERF — docs/conceptual-domain-context

## Resumen

Movimiento documental (S+) para anclar el **Norte Conceptual** del dominio de GesFer y reforzar reglas de alineación.

## Objetivo cumplido

- Se creó `docs/BUSINESS_DOMAIN.md` como fuente soberana del dominio (compra → stock → venta; dualidad Admin/Tenant).
- Se añadió el pilar **Pragmatismo de Sector** en `docs/MANIFESTO.md`.
- Se añadió la ley **[INTEGRIDAD CONCEPTUAL]** en `docs/rules/GOLDEN_RULES.md`.

## Restricciones respetadas

- Sin cambios en código (`.cs`, `.js`, `.vue`) ni creación de entidades/BD/servicios.

## Evidencia / Artefactos

- Pasaporte de rama: `docs/branches/docs-conceptual-domain-context.md`
- Norte de dominio: `docs/BUSINESS_DOMAIN.md`

## Validación

- `scripts/validate-pr.ps1` (Juez Modular) — ejecutar antes de push.

## Kaizen (aprendizaje)

- Para la próxima iteración, **generar el pasaporte de rama automáticamente al inicio** (y su `IA_PERF_<rama>.md`) para evitar bloqueos tempranos del Juez por artefactos faltantes (ej. incidente registrado como `image_ec94e4.png`).

