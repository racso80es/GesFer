# Branch Documentation: kaizen/daily-audit-frontend

**Objetivo:** Programar Auditoría Frontend Diaria.
**Descripción:** Se ha creado un workflow de GitHub Actions (`.github/workflows/daily-frontend-audit.yml`) que ejecuta automáticamente el script `scripts/audit_frontend_daily.py` a la medianoche (UTC). Este script escanea los directorios de frontend, genera un reporte y actualiza el log de evolución si es necesario.
**Agente:** FRONT-ARCHITECT
