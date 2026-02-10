# Análisis Fase 1 — Protocolo Racso-Tormentosa: Atomic Console Actions

## 1. Información del análisis

| Campo | Valor |
| :--- | :--- |
| **Rama** | feature/atomic-console-actions |
| **Fase** | 1 (Análisis detallado + opciones de solución) |
| **Fecha** | 2026-02-10 |
| **Código afectado** | GesFer.Console (MenuService, StartLocalEnvironmentCommand, DTOs) |

---

## 2. Análisis detallado del código actual afectado

### 2.1. Componentes revisados

| Archivo | Responsabilidad | Líneas aprox. | Estado respecto a SPEC |
| :--- | :--- | :--- | :--- |
| `src/Console/Services/MenuService.cs` | Menú principal, submenú Acción 3, delegación a comandos | ~860 | Alineado con SPEC |
| `src/Console/Commands/StartLocalEnvironmentCommand.cs` | Levantar servicios con flags granulares, liberar puertos, build selectivo | ~415 | Alineado con SPEC |
| `src/Console/Commands/Dtos` (implícito) | `StartLocalEnvironmentInput` | — | Definido en mismo archivo del comando |

### 2.2. Flujo actual (Acción 3 — Acciones Atómicas)

1. **Menú principal** (`ShowMenu`): Opción 3 → "Acciones Atómicas (Docker, Seeds, Servicios)". Opción 8 no aparece. ✅  
2. **ExecuteOptionAsync(3)** → `ExecuteAtomicActionsMenuAsync()`: bucle que muestra submenú con 5 opciones (1–5, siendo 5 Volver). ✅  
3. **Subopciones:**
   - **3.1** → `ExecuteDockerInitializationAsync()`: RemoveContainers → CreateContainers → WaitMySqlReady. ✅  
   - **3.2** → `ExecuteSeedsMenuAsync()`: Scope (Shared/Admin/Product/All) + Level (Master/Demo/Test). ✅  
   - **3.3** → `ExecuteStartServicesMenuAsync()`: menú 1–6; construye `StartLocalEnvironmentInput` con flags y llama a `StartLocalEnvironmentCommand.HandleAsync(input)`. ✅  
   - **3.4** → `ExecuteDatabaseInitializationStep8Async()`: migraciones + seeds vía `InitializeDatabaseCommand`. ✅  

### 2.3. StartLocalEnvironmentCommand — Comportamiento granular

- **DTO** `StartLocalEnvironmentInput`: `StartProductApi`, `StartAdminApi`, `StartProductFront`, `StartAdminFront`; `IsStartAll` cuando todos son false (retrocompatibilidad con Action 2). ✅  
- **HandleAsync:** Si `IsStartAll`, rellena los cuatro flags a true. Compila solo los backends solicitados; prepara solo los frontends solicitados; libera solo los puertos de los servicios a iniciar; arranca solo los procesos correspondientes. ✅  
- **Puertos:** Product API 5000, Admin API 5010 (o desde launchSettings), fronts desde package.json. ✅  

### 2.4. Puntos fuertes del estado actual

- SPEC y PLAN cubiertos a nivel funcional: submenú, granularidad, eliminación de Action 8, reubicación de init BD en 3.4.  
- Action 2 se mantiene como shortcut (nuevo `StartLocalEnvironmentInput()` → IsStartAll).  
- Menú 3.3 claro: 1=Todos, 2–5=servicio individual, 6=Volver.  
- Liberación de puertos y compilación condicional evitan trabajo innecesario.

### 2.5. Gaps y riesgos identificados

| Id | Descripción | Severidad |
| :--- | :--- | :--- |
| G1 | **MenuService** concentra toda la orquestación y strings de UI (~860 líneas): difícil de testear unitariamente y de extender. | Media |
| G2 | **ExecuteStartServicesMenuAsync**: tras `HandleAsync` no hay mensaje explícito "Presione cualquier tecla para continuar" al volver (el comando tiene loop 'q'); el flujo vuelve al submenú atómico. Comportamiento correcto pero la UX al "volver" podría mejorarse con un mensaje breve. | Baja |
| G3 | **Tests**: E2E en `Option1IntegrationTest` instancian MenuService y StartLocalEnvironmentCommand; no hay tests unitarios específicos para el submenú 3.1–3.4 ni para `StartLocalEnvironmentInput` con combinaciones de flags. | Media |
| G4 | **Duplicación de lógica**: `ExecuteDockerInitializationAsync` repite la secuencia Remove→Create→Wait que también existe dentro de `ExecuteFullInitializationAsync` (pasos 6–8). Mantenimiento duplicado. | Baja |
| G5 | **Criterios de aceptación SPEC**: No hay checklist automatizado (p. ej. tests que verifiquen que la opción 8 no existe o que 3.3 llama con los flags correctos). | Baja |

### 2.6. Resumen del diagnóstico

El código actual **cumple los requisitos funcionales** de la SPEC y del PLAN. La base está sana y la compilación del proyecto Console es correcta. Los gaps son sobre todo de **mantenibilidad**, **testabilidad** y **pequeños ajustes de UX/documentación**, no de corrección funcional.

---

## 3. Opciones de solución razonadas

### Opción A — Estabilización y cierre de criterios (recomendada para cerrar la rama)

**Objetivo:** Cerrar la rama con el mínimo cambio necesario: verificar criterios de aceptación, documentar y opcionalmente pequeños ajustes de UX.

**Acciones:**

1. **Verificación manual/checklist:** Recorrer los criterios de aceptación de la SPEC (sección 5) y marcar en `OBJECTIVE.md` o en la SPEC que se cumplen, con evidencia breve (ej. “Acción 8 no aparece en ShowMenu”, “3.3 opciones 2–5 verificadas”).  
2. **UX:** En `ExecuteStartServicesMenuAsync`, tras el regreso de `HandleAsync`, mostrar un mensaje corto: “Servicios detenidos. Volviendo al menú de acciones atómicas.” y luego `SafeReadKey()` antes de que el bucle de `ExecuteAtomicActionsMenuAsync` repinta el menú (solo si se considera necesario para claridad).  
3. **Documentación:** Dejar en `docs/diagnostics/feature-atomic-console-actions/` este análisis y el OBJECTIVE.md como documento de entrada de la rama. Actualizar HISTORY en `docs/audits/diagnostics/` si el proyecto lo exige.  
4. **Build y smoke:** Mantener `dotnet build` del Console en verde; opcional ejecutar una pasada manual del menú (1, 2, 3 → 3.1, 3.2, 3.3, 3.4) para smoke test.

**Ventajas:**  
- Mínimo riesgo y mínimo código nuevo.  
- Rama cerrable en poco tiempo con criterios de aceptación documentados.

**Desventajas:**  
- No reduce la complejidad de MenuService ni añade tests automáticos para las nuevas rutas.

**Cuándo elegirla:** Cuando el objetivo prioritario es **cerrar la feature con base estable** y dejar mejoras estructurales para una rama futura.

---

### Opción B — Refactor ligero + tests de aceptación

**Objetivo:** Mantener el comportamiento actual pero mejorar testabilidad y trazabilidad, y reducir duplicación mínima.

**Acciones:**

1. **Extracción de “orquestadores” (opcional y acotada):** Introducir métodos privados o clases auxiliares que encapsulen: (a) “Docker init” (Remove + Create + Wait) y (b) “Start services from input”. Tanto `ExecuteFullInitializationAsync` como `ExecuteDockerInitializationAsync` usarían el mismo flujo Docker; `ExecuteStartServicesMenuAsync` delegaría en un método que reciba el `StartLocalEnvironmentInput` ya construido. Sin cambiar la API pública de MenuService.  
2. **Tests:**  
   - Unit tests (o tests de integración ligeros) que verifiquen: para opción 3, que se muestra el submenú con las 5 opciones; que al elegir 3.3 y opción 2 se invoca `StartLocalEnvironmentCommand.HandleAsync` con `StartProductApi == true` y el resto false (mediante mock o wrapper de bajo nivel si es posible sin tocar demasiado el diseño).  
   - Un test que verifique que el menú principal no contiene la cadena “8. Ejecutar seeds” (o equivalente) para asegurar que la Action 8 no aparece.  
3. **Checklist de criterios:** Igual que en A, más evidencia automática vía los tests anteriores.  
4. **UX y docs:** Los mismos ajustes que en Opción A (mensaje al volver de 3.3 si se desea, y documentación de entrada en diagnostics).

**Ventajas:**  
- Mejor base para futuras extensiones del menú y menos riesgo de regresiones.  
- Criterios de aceptación parcialmente automatizados.

**Desventajas:**  
- Más tiempo y más cambios; requiere definir bien los puntos de inyección/mock para no acoplar tests al detalle de implementación.

**Cuándo elegirla:** Cuando se quiera **dejar la rama no solo estable sino más mantenible y con cobertura** para las nuevas acciones atómicas.

---

## 4. Recomendación y acuerdo

- **Para un cierre rápido y seguro de la feature:** **Opción A**.  
- **Para invertir un poco más y mejorar mantenibilidad y cobertura:** **Opción B**.

Se recomienda **acordar explícitamente** con el responsable (Racso/Tormentosa) cuál de las dos opciones se ejecutará en esta rama, y dejar constancia en `OBJECTIVE.md` o en un comentario en este análisis (ej. “Acordado: Opción A” o “Acordado: Opción B con refactor acotado”).

---

## 5. Trazabilidad

- **Check inicial:** `dotnet build` sobre `GesFer.Console.csproj` — correcto (0 errores).  
- **Referencias:** SPEC-20260209-1041-Atomic_Console_Actions, PLAN-20260209-1041-Atomic_Console_Actions, CLARIFICATIONS mismo feature.
