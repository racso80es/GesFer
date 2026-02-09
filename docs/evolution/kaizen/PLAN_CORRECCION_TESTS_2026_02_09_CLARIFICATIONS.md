# Clarificaciones para el Plan de Corrección de Tests 2026-02-09

**Fecha:** 2026-02-09
**Auditor:** Jules (AI Assistant)
**Usuario:** Owner

Este documento registra las aclaraciones y decisiones tomadas respecto a `PLAN_CORRECCION_TESTS_2026_02_09.md` durante la fase de análisis.

## 1. Proyectos de Tests Huérfanos (KAIZEN-01)
**Pregunta:** ¿Se puede confirmar que los proyectos (`GesFer.Shared.Back.UnitTests`, `GesFer.Architecture.Tests`, `GesFer.Admin.IntegrationTests`) compilan correctamente?
**Respuesta:** No se puede confirmar. Es necesario realizar un análisis previo.
**Acción:** Se añadirá una fase de verificación de compilación antes de integrarlos permanentemente.

## 2. Desacoplamiento de Infraestructura en Tests (KAIZEN-03 - DockerService)
**Pregunta:** ¿Dónde ubicar `IDockerService` y si debe soportar cross-platform?
**Respuesta:** Omitir esta acción. Guardar como posible refactor futuro.
**Acción:** Se marca como **DIFERIDO** en el plan principal.

## 3. Doble Guardado / IsActive (KAIZEN-03b)
**Pregunta:** ¿Riesgo de regresión al eliminar `entry.Entity.IsActive = true` en DbContext?
**Respuesta:** No estamos seguros. Omitir esta acción. Guardar como posible refactor futuro.
**Acción:** Se marca como **DIFERIDO** en el plan principal.

## 4. Estrategia de Cobertura (KAIZEN-02)
**Pregunta:** ¿Confirmar creación de nuevos tests para `AdminAuthService` usando Moq?
**Respuesta:** Sí.
**Acción:** Se procede según plan original.

## 5. Deuda Técnica en Benchmarks (KAIZEN-04)
**Pregunta:** ¿Confirmar uso de `null!` para suprimir CS8618?
**Respuesta:** Ok.
**Acción:** Se procede según plan original.

## 6. Plan de Rollback
**Pregunta:** ¿Se requiere un plan de rollback explícito?
**Respuesta:** No hace falta.
**Acción:** No se añade sección de rollback.
