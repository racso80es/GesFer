# Documentación de bugs y fixes

**Ruta canónica:** Definida en `openspecs/agents/knowledge-architect.json` → `paths.fixPath` (por defecto `./docs/bugs/`).

Cada bug o fix tiene su carpeta bajo `docs/bugs/{bug}/`, donde `{bug}` es un identificador corto (ej. `admin-back-repeated-failures`). La documentación de análisis, decisión y seguimiento del bug se ubica ahí.

**Consulta:** Los agentes deben obtener la ruta base de documentación de bugs desde el agente de documentación (Knowledge Architect): leer `openspecs/agents/knowledge-architect.json` → `paths.fixPath`.
