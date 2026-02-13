# Clarificaciones: SPEC CRUD Familia de Artículo (ArticleFamily)

**ID:** SPEC-GF-2026-ARTICLE-FAMILY_CLARIFICATIONS  
**Fecha:** 2026-02-13  
**Origen:** [SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md](SPEC-ARTICLE-FAMILY-CRUD-PROPUESTA.md)  
**Estado:** Resuelto

---

## 1. Preguntas y respuestas

### 1.1. Relación con la entidad existente `Family` (CRÍTICO)
- **Q1.1:** ¿ArticleFamily sustituye a Family (y por tanto Article pasará a tener ArticleFamilyId en una fase posterior), o conviven ambos maestros con fines distintos?  
  **A1.1:** Sí. **Family sale**; ArticleFamily es la entidad correcta. Article pasará a ArticleFamilyId en una iteración posterior (fuera de esta SPEC).

- **Q1.2:** Si conviven: ¿qué maestro debe usar la UI "Familias de Artículos"?  
  **A1.2:** N/A — no conviven. La UI "Familias de Artículos" es solo para ArticleFamily.

- **Q1.3:** Si ArticleFamily sustituye a Family: ¿migración Article en esta SPEC o después?  
  **A1.3:** Inicialmente fuera de SPEC. **Actualización (planificación):** el reemplazo completo de Family queda incluido en esta planificación: migración Article.FamilyId → ArticleFamilyId, migración de datos, eliminación de Family y limpieza. Ver [PLAN-ARTICLE-FAMILY-CRUD.md](PLAN-ARTICLE-FAMILY-CRUD.md).

---

### 1.2. Nombre de la clave de seed en `demo-data.json`
- **Q2.1:** ¿Se confirma la clave `articleFamilies`?  
  **A2.1:** Sí.

- **Q2.2:** ¿El DTO de seed se llamará `ArticleFamilySeed`?  
  **A2.2:** Sí. `ArticleFamilySeed` en `JsonDataSeeder`.

---

### 1.3. Borrado: solo soft delete o permitir físico
- **Q3.1:** ¿Solo soft delete para ArticleFamily (y TaxType)?  
  **A3.1:** Solo soft delete.

- **Q3.2:** ¿El estándar en Product para maestros es únicamente soft delete?  
  **A3.2:** Sí. En el futuro el estándar en Product será soft delete para maestros (regla documentada en agente de seguridad).

---

### 1.4. Permisos y autorización
- **Q4.1:** ¿Qué permiso(s) para crear/editar/eliminar?  
  **A4.1:** Permisos CRUD básicos: **(1) Consultar** (lectura) y **(2) Gestionar** (crear, editar, eliminar). Por defecto los CRUD de maestros tendrán estos dos permisos (regla documentada en agente de seguridad).

- **Q4.2:** ¿Menú según permiso o todo usuario autenticado?  
  **A4.2:** Visibilidad del menú según permiso (consultar o gestionar).

---

### 1.5. UI: modal vs página dedicada
- **Q5.1:** ¿Formulario en modal o rutas dedicadas?  
  **A5.1:** **Modal** (drawer/sheet) desde el listado. Por defecto los maestros serán así (norma añadida al arquitecto frontend).

- **Q5.2:** ¿Referencia TaxType u otro maestro?  
  **A5.2:** Patrón modal como en maestros existentes (TaxType como referencia).

---

### 1.6. Orden de carga de seeds y dependencias
- **Q6.1:** ¿Companies antes que TaxTypes/ArticleFamilies? ¿Validación de companyId?  
  **A6.1:** Sí. Company está en Admin; las familias están en Product. El orden ha de ser coherente con dependencias: no depende de Article, sí de TaxType. Companies (Admin o demo-data) deben existir antes; el seeder validará que cada `companyId` de taxTypes/articleFamilies sea coherente (exista donde corresponda según arquitectura).

- **Q6.2:** ¿Guids de TaxTypes estables y referenciados por articleFamilies?  
  **A6.2:** Sí. Mismo `companyId` y referencias por id en el mismo JSON.

---

### 1.7. ValueObjects en dominio
- **Q7.1:** ¿ValueObject para Code en esta iteración?  
  **A7.1:** **ValueObject como deuda técnica.** En esta iteración se usa string con validación FluentValidation; se documenta la deuda para introducir ValueObject (p. ej. para código de familia) en el futuro.

- **Q7.2:** ¿ValueObjects existentes para códigos de maestros?  
  **A7.2:** Revisar en implementación; deuda técnica aplica si no existe uno reutilizable.

---

### 1.8. Seguridad y auditoría
- **Q8.1:** ¿Operaciones CRUD registradas en log de auditoría de la aplicación?  
  **A8.1:** **Sí. Log de operaciones en BD** (quién creó/editó/eliminó y cuándo). La implementación debe respetar la regla del auditor.back: Product DbContext no alberga DbSets de Audit/Log; el log se implementará vía servicio/API de auditoría o base de datos de auditoría separada.

- **Q8.2:** ¿Product DbContext sin Audit/Log y esta feature no añade tablas de auditoría en Product?  
  **A8.2:** Product DbContext sigue sin DbSets de Audit/Log. El log de operaciones CRUD se persiste en BD mediante el mecanismo que cumpla la restricción (ej. servicio de auditoría, BD de auditoría).

---

## 2. Acciones aplicadas

- SPEC actualizada: Family sustituida por ArticleFamily; migración Article en iteración posterior.
- Clave seed `articleFamilies` y DTO `ArticleFamilySeed` fijados.
- Solo soft delete; estándar Product = soft delete para maestros (seguridad).
- Permisos: Consultar + Gestionar; regla por defecto para CRUD (seguridad).
- UI maestros: modal por defecto (arquitecto frontend).
- Orden seeds y dependencias (Company, TaxType; no Article) documentado.
- ValueObject para Code registrado como deuda técnica.
- Log de operaciones en BD especificado; implementación respetando aislamiento Product DbContext.
