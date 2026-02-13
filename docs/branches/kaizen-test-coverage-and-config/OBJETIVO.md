# Objetivo de la Rama: kaizen/test-coverage-and-config

Esta rama tiene como objetivo ejecutar acciones correctivas identificadas en la auditoría `AUDITORIA_TESTS_2026_02_13.md` para mejorar la métrica de cobertura de código y robustecer la suite de pruebas.

## Acciones Principales

1.  **Configuración de Cobertura (.runsettings):**
    -   Se añade un archivo `.runsettings` para excluir del análisis de cobertura el código generado automáticamente (Migraciones de EF Core y archivos Designer).
    -   Esto permite obtener métricas de cobertura más realistas y centradas en la lógica de negocio.

2.  **Unit Tests para Comandos (Company & PostalCode):**
    -   Se crean tests unitarios para los comandos `Create`, `Update` y `Delete` de las entidades `Company` y `PostalCode`.
    -   Estos tests validan la correcta instanciación de los comandos y la asignación de propiedades desde los DTOs, cubriendo áreas previamente reportadas con 0% de cobertura.

3.  **Mejora de Tests de Seguridad (SensitiveDataSanitizer):**
    -   Se amplía la suite de tests para `SensitiveDataSanitizer` añadiendo validaciones de bordes (longitud inválida), unicidad y comportamiento determinista.

## Resultado Esperado

-   Aumento en la métrica de cobertura de código "limpia" (al excluir ruido).
-   Eliminación de áreas críticas con cobertura nula en comandos básicos.
-   Suite de pruebas más robusta y confiable.
