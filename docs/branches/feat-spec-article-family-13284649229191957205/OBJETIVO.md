# OBJETIVO: Especificación e implementación Article Family CRUD

## Resumen
Unificación del modelo Family en ArticleFamily: backend (entidad, migraciones, seeds, API), frontend (tipos, API, i18n, página Familias de artículos, formulario, sidebar) y corrección de front (componentes Shared UI, form, sonner, tax-types-api).

## Alcance
*   Backend: ArticleFamily, migraciones, JsonDataSeeder articleFamilies, demo-data, albaranes con IVA por ArticleFamily.TaxType, tests.
*   Frontend: article-families-api, página /maestros/familias-articulos, ArticleFamilyForm, i18n, navegación. Componentes Shared: table, alert-dialog, select. Form en Front (react-hook-form). Toaster (sonner). tax-types-api con apiClient.
*   Tests: kaizen test unitario article-families-api (Front).
