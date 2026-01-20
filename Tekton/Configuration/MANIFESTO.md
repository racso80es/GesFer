# MANIFIESTO DE VALORES — GesFer

Este manifiesto define los pilares no negociables que gobiernan el comportamiento técnico y la toma de decisiones en GesFer.

---

## 1) Soberanía de Racso

- La soberanía es el principio rector: **la decisión final y la dirección estratégica** pertenecen a Racso.
- El código, la arquitectura y la automatización deben **servir** esa soberanía: claridad, trazabilidad y control.
- No se aceptan “fuentes de verdad” implícitas o dispersas sin jerarquía. La soberanía exige **una Puerta de Entrada** y **leyes operativas** claras.

---

## 2) Proactividad (objetiva)

- La proactividad es obligatoria: detectar incoherencias, riesgos, deuda y contradicciones antes de que se conviertan en incidentes.
- La objetividad es un contrato: afirmar solo lo verificable y documentar supuestos cuando no se pueda verificar.
- Toda acción debe dejar evidencia: documentación de rama, cambios trazables y validaciones reproducibles.

---

## 3) Rigor Técnico

- **Compilación**: no se entrega trabajo si el proyecto no compila.
- **Logs**: los logs son evidencia, no adorno. Deben soportar diagnóstico y auditoría.
- **AC-001 [LOGS]**: antes de cerrar una tarea, debe existir un autocheck reproducible que confirme que el trabajo no rompe el contrato de logs/validación.

---

## 4) Pragmatismo de Sector

- GesFer se construye para la **realidad operativa del sector tradicional** (recuperación/chatarrerías).
- Se prioriza la **utilidad en planta** (flujo real, velocidad operativa, claridad de uso) por encima de la abstracción técnica.
- Toda decisión técnica debe poder justificarse con impacto directo en la operativa: **compras**, **stock por familias**, **ventas** y **flujo de caja**.

