# CONSTITUCIÓN DE INTELIGENCIA ARTIFICIAL (IA) - GesFer

> **ESTADO:** Borrador Inicial
> **PROPÓSITO:** Estandarizar la interacción, herramientas y reglas operativas para agentes de IA en el ecosistema GesFer.

---

## 1. Principios Fundamentales
1.  **Soberanía de Racso:** La IA asiste, no decide la estrategia.
2.  **Transparencia:** Toda acción de la IA debe ser explicable y trazable.
3.  **No Alucinación:** Priorizar "No sé" sobre inventar datos. Validar siempre contra el código fuente.

## 2. Herramientas Estándar
*   **Lenguaje:** Español (Exclusivamente).
*   **Formato de Archivos:** Markdown para documentación, JSON para configuración.
*   **Shell:** PowerShell 7+ (Windows).

## 3. Protocolo de Agentes (OpenSpec Standard)
*   **Definición de Roles:** Los agentes se definen mediante especificaciones JSON estrictas ubicadas en `openspecs/agents/`.
*   **Capacidades:** Las habilidades ("skills") son atómicas y reutilizables, ubicadas en `openspecs/skills/`.
*   **Consulta:** `AGENTS.md` actúa como el enrutador principal para activar el rol adecuado.
*   **Ejecución:** Seguir el protocolo "Chain of Thought" antes de ejecutar cambios, validando contra la `constitution.json` en `openspecs/`.

---
*Documento actualizado al estándar OpenSpec (2026).*
