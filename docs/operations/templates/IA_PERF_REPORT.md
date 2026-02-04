# IA Performance Report — Template (S+)

**Repositorio**: GesFer (Paciente 0)  
**Rama**: `<git-branch>`  
**Fecha**: `<YYYY-MM-DD>`  
**Objetivo**: `<objetivo resumido>`

---

## 0) Resumen ejecutivo

Describe el resultado final en 3-6 líneas, incluyendo:
- qué se consolidó,
- qué se blindó,
- qué se eliminó (fragmentación),
- y qué validaciones quedaron activas.

---

## 1) First-shot Success

**Resultado**: `<Alto | Medio | Bajo>`

- **Definición**: porcentaje de acciones correctas al primer intento (sin re-trabajo).
- **Evidencia**:
  - acciones que requirieron iteraciones y por qué,
  - errores de tooling/entorno y cómo se resolvieron.

---

## 2) Refactor Density

**Resultado**: `<Alta | Media | Baja>`

- **Definición**: cambios estructurales (arquitectura, reglas, enforcement, reducción de duplicidad) / cambios totales.
- **Evidencia**:
  - archivos clave consolidados,
  - “constituciones antiguas” eliminadas,
  - enforcement agregado/activado (Juez).

---

## 3) Context Leaks

**Resultado**: `<Cero | Bajo | Medio | Alto>`

- **Definición**: contradicciones o fuentes de verdad múltiples que sobreviven tras la intervención.
- **Resueltas**:
  - `<lista>`
- **Pendientes**:
  - `<lista>`

---

## 4) Manifesto Alignment

Evalúa alineación explícita con `/Tekton/Configuration/MANIFESTO.md`:

- **Soberanía de Racso**: `<OK/NO>` — evidencia
- **Proactividad**: `<OK/NO>` — evidencia
- **Rigor Técnico**: `<OK/NO>` — evidencia

