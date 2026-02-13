# Rama: chore/finish-logs-persistence

## Objetivo

Unificar en master la persistencia de logs en la ejecución de servicios (ejecutar-servicios.bat): registro estructurado en logs/services/ de toda salida y errores de ProductApi, AdminApi y ProductFront.

## Contenido

- **Scripts:** `scripts/run-service-with-log.ps1` (formato timestamp|level|service|message).
- **Bat:** `ejecutar-servicios.bat` invoca el script por cada servicio y crea logs/services.
- **Docs:** `docs/operations/LOGS_SERVICES_REFERENCE.md` actualizado con formato estructurado.

## Certificación

Commit manual; merge a master y push. Referencia: openspecs/skills/FinishFeature.md.
