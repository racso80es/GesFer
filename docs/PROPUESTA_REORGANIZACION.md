# Propuesta de Reorganización y Limpieza Documental

**Fecha:** 20 de Enero de 2026
**Estado:** Pendiente de Aprobación
**Objetivo:** Alinear la estructura del directorio `docs/` con las directrices de `AGENTS.md` y mejorar la navegabilidad.

---

## 1. Prioridad 1: Limpieza (Reducción de Ruido)
*Eliminar archivos sueltos de la raíz de `docs/` para garantizar que solo existan las directrices maestras.*

### Acciones Sugeridas:
1.  **Crear directorio de archivo:** `docs/diagnostics/history/`.
2.  **Mover Bitácoras Técnicas:**
    *   Mover `docs/CAMBIOS-RESOLUCION-TABLE-ALREADY-EXISTS.md` ➔ `docs/diagnostics/history/`
    *   Mover `docs/CAMBIOS-SEEDING-SINCRONIZACION.md` ➔ `docs/diagnostics/history/`
    *   Mover `docs/INFORME_CORRECCIONES_INTEGRIDAD_REFERENCIAL.md` ➔ `docs/diagnostics/history/`
    *   Mover `docs/INFORME_CORRECCION_TESTS_DBINITIALIZER.md` ➔ `docs/diagnostics/history/`
    *   Mover `docs/INFORME_TECNICO_SOLUCION.md` ➔ `docs/diagnostics/history/`
3.  **Reubicar Documentos de Gobierno:**
    *   Mover `docs/EVOLUTION_LOG.md` ➔ `docs/governance/EVOLUTION_LOG.md` (Para centralizar la "consciencia del sistema").

> **Resultado Esperado:** La raíz de `docs/` quedará limpia, conteniendo únicamente `AGENTS.md`, `BUSINESS_DOMAIN.md` y `CHANGELOG.md` (archivos de alto nivel).

---

## 2. Prioridad 2: Consistencia Estructural
*Unificar carpetas redundantes y purificar directorios de roles.*

### Acciones Sugeridas:
1.  **Unificar Auditorías:**
    *   Mover contenido de `docs/audit/` (ej. `INTEGRIDAD_GLOBAL_*.md`) ➔ `docs/governance/audits/`.
    *   Eliminar directorio vacío `docs/audit/`.
    *   *Justificación:* Centralizar toda la evidencia de gobierno y validación en un solo lugar.
2.  **Purificar Directorio de Agentes:**
    *   Mover `docs/agents/OPTIMIZATION_REPORT.md` ➔ `docs/kaizen/OPTIMIZATION_REPORT_AGENTS.md`.
    *   *Justificación:* `docs/agents/` debe contener exclusivamente los perfiles de roles (`rol_*.md`) para evitar contaminación de contexto al cargar roles.

---

## 3. Prioridad 3: Integridad del Dominio y Naming
*Ajustes menores para consistencia semántica.*

### Observaciones:
*   **`BUSINESS_DOMAIN.md`**: Se sugiere mantener en la raíz de `docs/` o mover a `docs/governance/` si se desea una raíz "inmaculada". Por ahora, su presencia en raíz es aceptable como "Norte Conceptual".
*   **`Output/`**: Existe una carpeta `Output/` en `docs/`. Se sugiere verificar si contiene artefactos generados automáticamente y, de ser así, añadirla al `.gitignore` o moverla a un directorio temporal fuera de la documentación oficial.

---

## Plan de Ejecución Inmediata
Si este informe es aprobado, procederé con los comandos de movimiento (`mv`) y validación de enlaces rotos.
