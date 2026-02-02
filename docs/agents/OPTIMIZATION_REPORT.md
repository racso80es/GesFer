# Reporte de Optimización de Sistema Multi-Agente (AGENTS.md)

## Diagnóstico Actual
El sistema actual es funcional y estructurado, pero peca de "verbosidad humana". Para un agente LLM, las instrucciones narrativas ("Como Arquitecto, mi misión es...") consumen tokens y diluyen la instrucción imperativa.

### Puntos Débiles Detectados:
1.  **Narrativa vs. Algoritmo:** Demasiada prosa explicativa. Un LLM obedece mejor a listas de verificación y sentencias IF/THEN.
2.  **Activación Difusa:** La tabla de roles es pasiva. Necesitamos disparadores agresivos ("Si detectas X, DETENTE y cambia a Rol Y").
3.  **Falta de "System Prompting":** Los archivos no instruyen al agente sobre *cómo* pensar, solo sobre qué hacer.

## Propuesta de Mejoras (Optimización de Tokens y Ejecución)

### 1. AGENTS.md (El Orquestador)
Convertirlo en un **algoritmo de arranque**.
*   **Nuevo Formato:** Diagrama de flujo lógico (texto).
*   **Thinking Protocol:** Añadir una sección obligatoria de "Pensamiento en Voz Alta" donde el agente debe declarar:
    1.  Contexto detectado.
    2.  Rol activado.
    3.  Reglas aplicables.
*   **Negative Constraints:** Una lista explícita de "LO QUE NO DEBO HACER" (ej: "No inventar carpetas", "No usar bash").

### 2. Modular Roles (Los Expertos)
Reescribir cada archivo para máxima densidad informativa.

#### A. Arquitecto (`rol_arquitecto.md`)
*   Eliminar: "No escribo código por escribir; construyo catedrales" (Ruido).
*   Añadir: Mapa ASCII estricto de la estructura permitida. Si el usuario pide crear algo fuera del mapa -> REJECT.

#### B. Tekton Dev (`rol_tekton_dev.md`)
*   Transformar en Checklist de ejecución.
*   Añadir snippets de código obligatorio (ej: estructura de Try/Catch estándar).

#### C. Juez QA (`rol_juez_qa.md`)
*   Definir el formato exacto del `AUDIT_FAIL.md`.
*   Instrucción de "Paranoia": Asumir que todo input de usuario está roto hasta que se demuestre lo contrario.

### 3. Mecanismo de Autocorrección
Incluir en `AGENTS.md` un paso final de **Self-Reflection**:
"Antes de responder, verifica: ¿He violado alguna Ley Universal? Si sí, corrígete."

## Plan de Ejecución de Refactorización
1.  Reescribir `AGENTS.md` como un **Protocolo de Sistema**.
2.  Comprimir los roles modulares a **Directivas de Alta Densidad**.
3.  Insertar "Palabras Clave de Activación" (ej: `[ACT: ARQUITECTO]`) para trazabilidad en el chat.
