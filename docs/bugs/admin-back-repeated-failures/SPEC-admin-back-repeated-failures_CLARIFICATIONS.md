# CLARIFICATION: Fix Admin Back fallos reiterados

**ID:** SPEC-admin-back-repeated-failures-CLARIFY  
**Date:** 2026-02-13  
**Author:** Clarifier  
**Status:** CLOSED  
**Spec:** docs/bugs/admin-back-repeated-failures/SPEC-admin-back-repeated-failures.md

---

## Questions & Answers

### 1. Gate en CI / pre-push
**Q:** ¿La SPEC del fix exige incluir los smoke tests en un paso de CI o pre-push en esta misma iteración?  
**A:** No. El alcance del fix es: smoke tests implementados, correcciones de Swagger/LogController/config, documentación bajo fixPath. Incluir el gate en pipeline (CI/pre-push) es una **acción Kaizen posterior**, especificable en otra SPEC (ej. Gate smoke tests CI). Para este fix basta con que los tests existan y pasen localmente.

### 2. MySQL opcional en Development
**Q:** ¿El fix debe implementar que Admin API arranque sin MySQL en Development?  
**A:** Fuera de alcance en este fix. Se documenta como **recomendación futura** (análisis y checklist). El entorno Testing con InMemory ya permite que los smoke tests no dependan de MySQL.

### 3. Nombre del fichero generado por Consola
**Q:** Si se invoca `GesFer.Console --spec` con `--context ./docs/bugs/admin-back-repeated-failures/`, ¿el nombre del fichero debe ser exactamente `SPEC-admin-back-repeated-failures.md`?  
**A:** La acción Spec decide el nombre según su convención actual (ej. `SPEC-{YYYYMMdd-HHmm}-{SanitizedTitle}.md`). Para alinear con la ruta del fix, se recomienda usar `--title admin-back-repeated-failures` para que el nombre sea coherente con el bug-id. La SPEC ya persistida en esta carpeta puede ser la referencia canónica aunque se haya creado manualmente.

### 4. Product Back smoke tests
**Q:** ¿Este fix incluye smoke tests equivalentes para Product Back?  
**A:** No. Alcance limitado a Admin Back. Product Back puede tener su propio fix/SPEC si se detectan fallos similares; el checklist "proyecto no funcional" ya contempla extensión a Product.

### 5. Seguridad: SharedSecret en repositorio
**Q:** ¿SharedSecret en appsettings.Development.json debe estar en el repositorio?  
**A:** Sí, solo en **Development** con valores no productivos (documentado en checklist). En producción la configuración viene de variables de entorno o secretos externos. No hardcodear secretos reales.

---

## Decision

Proceder con la implementación ya realizada: smoke tests, Program.cs, LogController, config y documentación. Verificación Tekton: build + tests Admin. Plan documenta tareas como completadas y pasos de verificación.
