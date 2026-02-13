# Acción: Clarificación de Requisitos (TaxType CRUD)

**Fecha:** 2026-01-XX
**Estado:** Aprobado

## 1. Objetivo
Asegurar la comprensión total de los requisitos para la implementación del CRUD de "Tipo de Tasa" antes de iniciar el desarrollo.

## 2. Preguntas Realizadas y Respuestas

### P1: Terminología
*   **Pregunta:** ¿Confirmar uso de `TaxType` en código y "Tipo de Tasa" en UI?
*   **Respuesta:** Confirmado.
*   **Acción:** Se usará `TaxType` para clases, tablas y API. UI mostrará "Tipo de Tasa". URL: `/maestros/tipotasa`.

### P2: Estructura de Datos
*   **Pregunta:** Confirmar campos: Id, Code, Name, Description, Value, CompanyId. ¿Value es decimal porcentual (21.0)?
*   **Respuesta:** Confirmado (implícito en "decimal correspondiente con %").
*   **Acción:** `Value` será `decimal(18,2)` almacenando 21.00 para 21%.

### P3: Datos Demo
*   **Pregunta:** ¿Qué valores iniciales se requieren?
*   **Respuesta:** Impuestos habituales en España.
*   **Acción:** Se añadirán:
    *   IVA General (21%)
    *   IVA Reducido (10%)
    *   IVA Superreducido (4%)
    *   Exento (0%)

### P4: Ubicación UI
*   **Pregunta:** ¿Dónde se ubicará en el menú?
*   **Respuesta:** Nuevo punto de menú "Maestros" -> "Tipo de Tasa".
*   **Acción:** Modificar `Sidebar.tsx` para incluir el grupo "Maestros" y el enlace.

## 3. Conclusiones
Los requisitos están claros y validados. Se procede con la implementación siguiendo el plan `PLAN-001-TaxType-CRUD.md`.
