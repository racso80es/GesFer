# HEALTH RADAR - Sistema de KPIs de Salud del Sistema

**Última Actualización**: 2025-01-27  
**Rama Actual**: `feature/arch-kpi-telemetry`

---

## Métricas Sagradas

### 1. Índice de Sincronización
**Fórmula**: `(DTOs Autogenerados / Entidades Totales) × 100`

**Descripción**: Mide la coherencia entre el dominio (entidades) y la capa de aplicación (DTOs). Un índice alto indica que todas las entidades principales tienen sus correspondientes DTOs para exposición a través de la API.

**Baseline Actual**: `53.8%`
- Entidades Totales: `26`
- DTOs Principales (Response): `14`
- Porcentaje: `53.8%`

**Objetivo**: > 90%

---

### 2. Densidad Kaizen
**Fórmula**: `Conteo de basura técnica eliminada`

**Descripción**: Métrica acumulativa que cuenta la cantidad de basura técnica eliminada durante el proceso de mejora continua:
- Imports/usings no utilizados
- Código muerto (funciones/clases nunca llamadas)
- Lógica duplicada consolidada
- Comentarios obsoletos
- Archivos innecesarios

**Baseline Actual**: `0` (valor inicial)

**Valor Acumulado**: `5` (usos de `confirm()` detectados que requieren `DestructiveActionConfirm`)

**Objetivo**: Incremento continuo (mejora continua)

---

### 3. Inmunidad de Test
**Fórmula**: `(% de tests con data-test-id / Total de tests) × 100`

**Descripción**: Mide la robustez de los tests frontend mediante la presencia de selectores estables (`data-test-id`). Un alto índice de inmunidad indica que los tests son resistentes a cambios en el HTML/CSS.

**Baseline Actual**: `0%`
- Tests Totales: `16`
- Tests con data-test-id: `0`
- Porcentaje: `0%`

**Objetivo**: > 80%

---

## Evolución Histórica

| Fecha | Rama | Índice Sincronización | Densidad Kaizen | Inmunidad Test | Notas |
|-------|------|----------------------|-----------------|----------------|-------|
| 2025-01-27 | `feature/arch-kpi-telemetry` | `53.8%` | `0` | `0%` | Baseline inicial |

---

## Certificación

**Estado Actual**: 🔴 BASELINE EN PROGRESO

Para certificar esta fase, se requiere:
- [ ] Auditoría completa realizada
- [ ] Valores baseline documentados
- [ ] Densidad Kaizen inicial reportada (> 0)
- [ ] Basura técnica identificada y limpiada
- [ ] CURRENT_REF.md actualizado
