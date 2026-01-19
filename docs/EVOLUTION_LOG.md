# EVOLUTION LOG — GesFer (Consciencia del Sistema)

Este archivo registra **hitos de evolución** del producto y su gobierno (dominio, roles, reglas, empaquetado SaaS), de forma cronológica y trazable.

> Regla: este log no reemplaza `docs/CHANGELOG.md`; aquí se registran decisiones estructurales y de soberanía.

---

## KPIs de Salud (4) — Norte de control

Estos KPIs definen la salud del sistema como producto SaaS (S+). Su objetivo es medir **soberanía**, **operabilidad**, **seguridad de permisos** y **capacidad de venta/contratación**.

### 1) Aislamiento de Dominio

- **Definición**: frontera estructural Admin (global) ↔ Empresa (operativa) sin contaminación de contratos, estado o semántica.
- **Señales**:
  - No hay DTOs compartidos indebidos entre dominios.
  - Persistencia/cookies/storage segregados por namespace (ej. `admin_*` vs empresa).
  - Guardas/servicios de auth separados por dominio.

### 2) Latencia de Operación

- **Definición**: fricción temporal para completar operaciones de planta (compra/venta, caja, stock) con consistencia.
- **Señales**:
  - Operaciones críticas ejecutables con mínimo número de pasos.
  - Validaciones y feedback rápidos (sin esperas y sin re-trabajo por reglas ocultas).
  - Ausencia de bloqueos recurrentes por tooling/entorno.

### 3) Integridad de Derechos

- **Definición**: el sistema valida de forma granular que el usuario posee el derecho requerido para cada acción; la empresa define y asigna derechos.
- **Señales**:
  - Controles de autorización por acción (acceso/consulta/edición/modificación/borrado).
  - No existen “bypasses” por sesión genérica o por dominio incorrecto.
  - Auditoría y trazabilidad de rechazos por falta de derecho.

### 4) Agilidad de Contratación

- **Definición**: capacidad de habilitar y gobernar el producto por contrato activo (tiers), con cambios de alcance controlados y sin reingeniería.
- **Señales**:
  - Tiers explícitos (Demo/Funcional/Premium) sin alterar el dominio.
  - Soberanía de empresa condicionada al contrato de producto activo.
  - Contratos marco como habilitadores (especialmente en Premium).

---

## 2026-01-19 — Infraestructura S+ (Juez Modular + soberanía documental)

- Se consolidó una **Puerta de Entrada**: `docs/MANIFESTO.md` + `docs/rules/GOLDEN_RULES.md`.
- Se rearmó el **Juez Modular**:
  - bloqueo S‑Grade por falta de **pasaporte de rama**,
  - bloqueo S‑Grade por falta de **telemetría IA** (global + reporte por rama).
- Se absorbió fragmentación documental, reduciendo “constituciones paralelas” y dejando una jerarquía explícita.
- Se ejecutó Kaizen real como prueba de infraestructura:
  - eliminación de `confirm()` nativo en el cliente (fuera de `node_modules`),
  - refuerzo de contrato de componentes `shared/` con `data-testid`.
- Aprendizaje operativo: ante limitaciones de tooling (ej. `rg` no disponible), se debe usar alternativa reproducible (ej. `git grep`) sin degradar trazabilidad.
- Aprendizaje recurrente: crear **pasaporte** + `IA_PERF_<rama>.md` al inicio evita bloqueos tempranos del Juez.

---

## 2026-01-19 — Aislamiento Admin ↔ Empresa (Ley de Invariancia)

- Se definió una frontera **no negociable** entre:
  - **Admin (global)**: identidad global, sin semántica de empresa.
  - **Empresa (operativa)**: multi‑empresa; operación real (planta, caja, stock).
- Se explicitó el set de prohibiciones: no herencia de contrato, no contaminación de almacenamiento, no semántica de empresa en Admin, no bypass por sesión genérica.

---

## 2026-01-19 — Soberanía de roles y validación granular

- Se consolidó el modelo conceptual: **Administrador Global** / **Administrador de la Empresa** / **Usuarios Operativos**.
- Se definió la responsabilidad del sistema: **validación granular de acción** por derecho requerido; la empresa es soberana en la gestión de permisos.

---

## 2026-01-19 — Tiers SaaS (Demo / Funcional / Premium) en el Norte Conceptual

- Se incorporó al Norte Conceptual (`docs/BUSINESS_DOMAIN.md`) el empaquetado SaaS del producto en **Demo**, **Funcional** y **Premium**.
- Se añadió la regla: la soberanía operativa de la empresa está supeditada al **contrato de producto activo** (tier).

---

## 2026-01-19 — Movimiento 12 (Sellado): consolidación de inteligencia + sincronización total

- Se centralizó la inteligencia histórica de reportes `IA_PERF_*` en este `docs/EVOLUTION_LOG.md` como consciencia única.
- Se definió DoD explícito de cierre: **local y nube como espejo** (ver `[DOD: REGLA DE SINCRONIZACIÓN]` en `docs/rules/GOLDEN_RULES.md`).
- Aprendizajes operativos consolidados:
  - PowerShell puede no soportar encadenamiento con `&&` (usar secuencias compatibles).
  - `dotnet build` puede fallar si existe un binario en uso (bloqueo por proceso activo); el cierre exige entorno limpio.
  - E2E puede fallar por servicios no disponibles; el Juez distingue advertencia de falla real cuando detecta `ECONNREFUSED`.

