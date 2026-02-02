# Propuesta de Reorganización y Limpieza Documental

**Fecha:** 20 de Enero de 2026
**Estado:** ✅ Completado
**Objetivo:** Alinear la estructura del directorio `docs/` con las directrices de `AGENTS.md` y mejorar la navegabilidad.

---

## 1. Prioridad 1: Limpieza (Reducción de Ruido)
*Eliminar archivos sueltos de la raíz de `docs/` para garantizar que solo existan las directrices maestras.*

### Acciones Realizadas:
1.  **Creado directorio de archivo:** `docs/diagnostics/history/`.
2.  **Movido Bitácoras Técnicas:**
    *   `docs/CAMBIOS-RESOLUCION-TABLE-ALREADY-EXISTS.md` ➔ `docs/diagnostics/history/`
    *   `docs/CAMBIOS-SEEDING-SINCRONIZACION.md` ➔ `docs/diagnostics/history/`
    *   `docs/INFORME_CORRECCIONES_INTEGRIDAD_REFERENCIAL.md` ➔ `docs/diagnostics/history/`
    *   `docs/INFORME_CORRECCION_TESTS_DBINITIALIZER.md` ➔ `docs/diagnostics/history/`
    *   `docs/INFORME_TECNICO_SOLUCION.md` ➔ `docs/diagnostics/history/`
3.  **Reubicado Documentos de Gobierno:**
    *   `docs/EVOLUTION_LOG.md` ➔ `docs/governance/EVOLUTION_LOG.md` (Para centralizar la "consciencia del sistema").

> **Resultado:** La raíz de `docs/` ha quedado limpia, conteniendo únicamente `AGENTS.md`, `BUSINESS_DOMAIN.md`, `CHANGELOG.md` y este informe.

---

## 2. Prioridad 2: Consistencia Estructural
*Unificar carpetas redundantes y purificar directorios de roles.*

### Acciones Realizadas:
1.  **Unificado Auditorías:**
    *   Movido contenido de `docs/audit/` (ej. `INTEGRIDAD_GLOBAL_*.md`) ➔ `docs/governance/audits/`.
    *   Eliminado directorio `docs/audit/`.
2.  **Purificado Directorio de Agentes:**
    *   Movido `docs/agents/OPTIMIZATION_REPORT.md` ➔ `docs/kaizen/OPTIMIZATION_REPORT_AGENTS.md`.

---

## 3. Prioridad 3: Integridad del Dominio y Naming
*Ajustes menores para consistencia semántica.*

### Observaciones:
*   **`BUSINESS_DOMAIN.md`**: Se mantiene en la raíz de `docs/` como "Norte Conceptual".
*   **`Output/`**: Se sugiere verificar si contiene artefactos generados automáticamente y, de ser así, añadirla al `.gitignore` o moverla a un directorio temporal fuera de la documentación oficial.

---

**Ejecución finalizada el 20 de Enero de 2026.**
