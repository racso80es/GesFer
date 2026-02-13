# Objetivo de la Rama: Chore/Daily-Frontend-Audit

## Contexto
Esta rama se utiliza para la ejecución automática diaria de la auditoría de calidad del Frontend.
El objetivo es escanear los directorios `src/Shared/Front`, `src/Product/Front` y `src/Admin/Front` para detectar:
1. Violaciones de terminología ("empresa" vs "organización").
2. Deuda técnica (`any`, `@ts-ignore`).
3. Problemas de accesibilidad (imágenes sin `alt`).

## Entregables
- Reporte de auditoría en `docs/audits/AUDITORIA_FRONTEND_[FECHA].md`.
- Actualización de `docs/EVOLUTION_LOG.md` en caso de fallas críticas.
