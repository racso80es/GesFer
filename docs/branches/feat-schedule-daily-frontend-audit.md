# Objetivo de la Rama: feat/schedule-daily-frontend-audit

Esta rama implementa la programación diaria de la auditoría del Frontend ("Auditoría Frontend Diaria"). Se añade un script automatizado \`scripts/audits/frontend-daily.js\` para escanear los directorios \`Shared/Front\`, \`Product/Front\` y \`Admin/Front\`, generando el documento \`AUDITORIA_FRONTEND_YYYY_MM_DD.md\`. El proceso está automatizado mediante un flujo de trabajo de GitHub Actions configurado para ejecutarse cada noche y crear un PR si no hay fallas críticas.
