# Objetivo de Rama: feature/atomic-console-actions

## 1. Identificación

| Campo | Valor |
| :--- | :--- |
| **Rama** | feature/atomic-console-actions |
| **Convención** | feature/atomic-console-actions (naming protocol) |
| **Documento de entrada** | docs/diagnostics/feature-atomic-console-actions/OBJECTIVE.md |
| **Spec de referencia** | docs/Feature/Atomic_Console_Actions/SPEC-20260209-1041-Atomic_Console_Actions.md |
| **Plan de referencia** | docs/Feature/Atomic_Console_Actions/PLAN-20260209-1041-Atomic_Console_Actions.md |

## 2. Objetivo (Goal)

Consolidar y refinar el menú de **GesFer.Console** en torno a **Acciones Atómicas** (Acción 3), de modo que:

- Las tareas de inicialización (Docker, Seeds, Servicios, BD) sean ejecutables de forma **granular** y **atómica**.
- Se evite ejecutar el pipeline completo de inicialización cuando solo se necesita una parte (ahorro de recursos y tiempo en desarrollo).
- El menú principal quede coherente: sin acciones redundantes (antigua Action 8 integrada en 3.2; antigua Action 3 integrada en 3.4).

## 3. Alcance

- **Incluido:** Submenú Acción 3 con 3.1 Docker, 3.2 Seeds, 3.3 Levantar Servicios (granular), 3.4 Inicialización Completa BD; uso de `StartLocalEnvironmentInput` con flags; eliminación de Action 8 del menú principal.
- **Fuera de alcance:** Cambios en lógica interna de seeders/migraciones o en docker-compose.

## 4. Acuerdo de implementación (Racso-Tormentosa)

- **Opción A** (commit 1): Estabilización y cierre de criterios — checklist verificado, UX en 3.3 (mensaje al volver de Levantar Servicios).
- **Opción B** (commit 2): Refactor ligero + tests — secuencia Docker extraída y reutilizada; test que el menú principal no contiene la opción 8 (Ejecutar seeds).

## 5. Criterios de cierre

- [x] Acción 3 muestra el submenú con las cuatro opciones atómicas.
- [x] 3.1 Inicializa Docker (recrear + esperar MySQL).
- [x] 3.2 Permite Scope/Level y ejecuta seeds correspondientes.
- [x] 3.3 Permite elegir servicios a levantar (Product/Admin API/Front o Todos) y libera puertos correctos.
- [x] 3.4 Ejecuta migraciones + seeds completos.
- [x] Acción 8 no aparece en menú principal; antigua Action 3 reubicada en 3.4.
- [x] Código compila sin errores (`dotnet build`).

## 6. Trazabilidad

- **Documento de entrada (diagnóstico):** Generado al crear la rama.
- **Análisis Fase 1 (Protocolo Racso-Tormentosa):** `docs/diagnostics/feature-atomic-console-actions/ANALISIS_FASE1_RACSO_TORMENTOSA.md`.
