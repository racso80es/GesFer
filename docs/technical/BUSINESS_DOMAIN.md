# BUSINESS DOMAIN — GesFer (Norte Conceptual)

Este documento define el **Norte Conceptual** del producto GesFer. Es la referencia soberana para validar que cualquier propuesta técnica (arquitectura, modelos, flujos, UI, reglas) **sirve al negocio real** descrito por Racso.

> **Regla**: si una decisión técnica contradice este documento, la decisión técnica debe corregirse o quedar explícitamente justificada y alineada.

---

## 1) Naturaleza del Producto

**GesFer es una plataforma comercial para la gestión operativa** en el sector de la recuperación (chatarrerías/recuperadores).

El foco principal no es “software genérico de inventario”, sino **operación en planta y movimiento comercial**: compras recurrentes de material, clasificación por familias, control de stock, control de caja y preparación de ventas mayoristas.

---

## 2) Modelo de Negocio — Agregación de Valor

GesFer se apoya en un flujo de **Agregación de Valor** que transforma entradas minoristas en salidas mayoristas:

\[
\text{Compra minorista (pequeñas cantidades)} \rightarrow \text{Almacenamiento / Stock} \rightarrow \text{Venta mayorista (grandes paquetes)}
\]

### 2.1 Compra minorista

- Se compra material en **pequeñas cantidades** y con alta frecuencia.
- La compra se registra como operación comercial y operativa (lo que entra, en qué familia cae, a qué precio, y su impacto en caja).

### 2.2 Almacenamiento / Stock

- El stock se gestiona por **familias** (clasificación operativa del material).
- El stock es “operación viva”: entradas, salidas, ajustes y trazabilidad mínima para sostener la gestión diaria.

### 2.3 Venta mayorista

- Se vende en **grandes paquetes/lotes** a un cliente mayorista.
- La venta debe reflejar el lote y su composición por familias/stock, y su impacto en caja.

---

## 3) Estructura Dual del Producto

GesFer existe como una estructura de dos portales con responsabilidades no negociables.

### 3.1 Portal Admin (Orquestador Global)

**Propósito**: administrar la capa global del ecosistema (multi‑empresa) y su crecimiento.

Responsabilidades canónicas:

- Alta y gestión de **empresas** (instancias).
- Gestión de **contratos marco** (estándares/condiciones globales que habilitan consistencia).
- **Analítica de crecimiento** y visión global (orquestación, no operación de planta).

### 3.2 Portal Empresa (Operativa Real)

**Propósito**: ejecutar la operación cotidiana en la empresa (instancia).

Responsabilidades canónicas:

- Gestión de **proveedores** (origen de compras) y **clientes** (destino de ventas).
- Gestión de **stock por familias** y sus movimientos.
- Gestión de **flujo de caja** vinculado a compras/ventas.

### 3.3 Roles de Usuario

- **Administrador Global (Nosotros)**: Gestión de la infraestructura y métricas de salud del ecosistema.
- **Administrador de la Empresa**: Soberano absoluto de su instancia. Gestiona sus propios usuarios, grupos y asignación de derechos.
- **Usuarios Operativos**: Perfiles (Planta, Caja, etc.) definidos y limitados por los permisos que su Administrador de la Empresa les otorgue.

---

## 4) Implicaciones Operativas (criterios de alineación)

Estas preguntas son obligatorias antes de implementar lógica de dominio o reglas:

- ¿La propuesta refleja el flujo **compra → stock → venta** sin inventar un modelo alternativo?
- ¿Está claro si cae en **Admin (global)** o en **Empresa (operativa)**?
- ¿Se preserva el **pragmatismo de sector** (utilidad en planta) por encima de abstracciones técnicas?

---

## 5) No‑Objetivos (para evitar deriva)

- No convertir GesFer en un ERP genérico “para todo”. El núcleo es **recuperación/chatarrería**.
- No implementar lógica de dominio “por inspiración técnica” sin trazabilidad al flujo de agregación de valor.

---

## 6) Tiers SaaS (Demo / Funcional / Premium)

GesFer se empaqueta comercialmente en tres niveles (tiers). **Los tiers no cambian el dominio**: solo delimitan alcance, límites operativos y capacidades habilitadas para cada empresa.

- **Demo**: funciones base de evaluación (**Báscula**) con límites explícitos de **tiempo** y/o **volumen**; onboarding guiado; datos de ejemplo; sin integraciones avanzadas.
- **Funcional**: operativa completa de planta para la empresa (**Caja**, **Stock**, **Usuarios**) sosteniendo el flujo compra → stock → venta; multi‑usuario y permisos por empresa.
- **Premium**: capacidades avanzadas (**Analítica**, **multi‑sede**, **contratos marco**) y garantías superiores (SLA/soporte según contrato).

### Regla de soberanía contractual (SaaS)

La soberanía operativa de una empresa (qué módulos/capacidades puede usar y bajo qué límites) está supeditada al **contrato de producto activo** (tier).

