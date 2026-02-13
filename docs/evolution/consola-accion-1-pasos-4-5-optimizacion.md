# Análisis: Consola Acción 1 – Pasos [4/12] y [5/12] (Frontends)

**Plan de tarea de optimización:** `openspecs/plans/PLAN-CONSOLA-ACCION-1-FRONT-OPTIMIZACION.json`

## Flujo actual

- **Ubicación:** `src/Console/Services/MenuService.cs` (aprox. líneas 392–418 y 690–751).
- **Qué hace:** Ejecuta en **paralelo** dos tareas, una por frontend:
  - **Product:** `cd src/Product/Front && npm install && npm run build`
  - **Admin:** `cd src/Admin/Front && npm install && npm run build`
- **Detalle:** Cada proceso usa `cmd /c` (Windows) con `RedirectStandardOutput/Error` y lectura asíncrona de streams para evitar deadlock.

## Lo que ya está bien

1. **Paralelismo:** Product y Admin se lanzan con `Task.WhenAll`; el tiempo total es el del más lento, no la suma de ambos.
2. **Buffers:** Se lee stdout/stderr en paralelo mientras el proceso corre, evitando bloqueos de npm.

## Ineficiencias detectadas

| # | Problema | Impacto |
|---|----------|---------|
| 1 | **Siempre se ejecuta `npm install`** antes de cada build, aunque `node_modules` exista y no haya cambios en dependencias. | +30–90 s por frontend (red + disco). En la mayoría de ejecuciones locales es redundante. |
| 2 | **Dos `npm install` en paralelo** (Product y Admin a la vez) compiten por red y disco. | Posible saturación I/O y uso de ancho de banda; en máquinas lentas puede no ser más rápido que hacer uno tras otro. |
| 3 | **Un solo proceso por frontend:** `install && build` en la misma invocación. | No se puede reutilizar un install reciente compartido ni saltar install sin tocar la lógica. |

## Optimizaciones recomendadas

### 1. (Recomendada) Omitir `npm install` cuando no sea necesario

- **Condición:** En cada directorio de frontend, si existe `node_modules` y no se detecta cambio en dependencias, ejecutar **solo** `npm run build`.
- **Detección de cambio:** Por ejemplo: existe `node_modules/.package-lock.json` y la fecha de modificación de `package.json` o `package-lock.json` es anterior a la de `node_modules` (o comparar hash de `package-lock.json` con uno guardado en `node_modules/.package-lock.json` si se usa esa convención).
- **Efecto:** En desarrollo local típico se ahorra 1–2 minutos (dos installs evitados). La primera vez o tras cambiar dependencias se sigue haciendo install.

### 2. Instalación en secuencia, build en paralelo

- **Flujo:**  
  1) `npm install` en Product → esperar.  
  2) `npm install` en Admin → esperar.  
  3) Lanzar `npm run build` en Product y Admin en paralelo.
- **Ventaja:** Menos contención de disco/red que dos installs a la vez; los builds siguen en paralelo.
- **Inconveniente:** Si se mantiene “siempre install”, el tiempo total puede ser similar o mayor (installs secuenciales).

### 3. Variable de entorno o flag “solo build”

- Ej.: `CONSOLA_SKIP_NPM_INSTALL=1` o argumento `--build-only` para la acción 1.
- Si está definido, ejecutar solo `npm run build` en ambos frontends (en paralelo).
- Útil para CI o cuando el desarrollador ya ha ejecutado install manualmente.

### 4. Usar `npm ci` cuando exista `package-lock.json`

- En entornos de integración, `npm ci` suele ser más rápido y determinista que `npm install`.
- Se podría usar en la consola cuando se detecte que estamos en CI (variable de entorno) y exista lockfile.

## Resumen

- **Más impacto:** Evitar `npm install` cuando `node_modules` esté actualizado (opción 1), manteniendo el paralelismo actual de los builds.
- **Complementario:** Instalación en secuencia y build en paralelo (opción 2) si se quiere reducir contención I/O sin cambiar la política de “cuándo” hacer install.
- **Flexibilidad:** Variable o flag “solo build” (opción 3) para casos en los que se sepa que no hace falta instalar.

Implementar la opción 1 en `RunNpmCompilationCheckAsync` (comprobar existencia y “freshness” de `node_modules` y, en ese caso, ejecutar solo `npm run build`) es la optimización recomendada para que [4/12] y [5/12] sean más eficientes sin perder fiabilidad.

---

## Nueva tarea de optimización

Objetivo: hacer los pasos [4/12] y [5/12] más eficientes y robustos, con reintento ante fallo y error bien aislado para su gestión.

### Ítems de la tarea

| # | Ítem | Prioridad | Descripción |
|---|------|-----------|-------------|
| 1 | Omitir `npm install` cuando no haga falta | Alta | Comprobar existencia y vigencia de `node_modules`; si aplica, ejecutar solo `npm run build`. |
| 2 | Reintento ante fallo de compilación | Alta | Si falla la compilación de uno o ambos frontends, **reintentar una vez** el paso (solo los que fallaron) antes de dar por fallido el flujo. |
| 3 | Aislamiento del error | Alta | Aislar el error de compilación para que sea **gestionable**: resultado tipado, mensaje claro, registro en log y sin tragar excepciones genéricas. |
| 4 | (Opcional) Install secuencial, build paralelo | Media | Si se mantiene install, valorar install secuencial y build en paralelo para reducir contención I/O. |

---

## Acción: Reintento y aislamiento del error

### Comportamiento requerido

1. **Primera ejecución del paso [4/12] y [5/12]**  
   Se ejecuta la compilación de ambos frontends en paralelo (como ahora).

2. **Si falla uno o ambos**  
   - No se considera fallo definitivo aún.  
   - Se **salta** la consideración de "acción fallida" y se **repite el proceso** de compilación **una sola vez** (reintento).  
   - En el reintento solo se vuelven a ejecutar los frontends que fallaron (Product, Admin o ambos).

3. **Después del reintento**  
   - Si tras el reintento sigue habiendo fallo, entonces sí se trata como error: se muestra mensaje, se registra y se pide "Presione cualquier tecla…" (o se devuelve fallo al flujo superior).  
   - Si en el reintento todos pasan, el flujo continúa con el paso [6/12].

### Aislamiento del error (para gestión correcta)

El error de compilación debe quedar **aislado** para poder gestionarlo bien:

| Requisito | Descripción |
|-----------|-------------|
| **Resultado tipado** | Usar un tipo claro (p. ej. `(bool Success, string? ErrorDetail, int? ExitCode)`) o un DTO/record con `ProjectName`, `Success`, `ErrorDetail`, `ExitCode`, `Attempt`. No depender solo de excepciones para el flujo normal. |
| **Mensaje claro** | Incluir en el mensaje: qué frontend falló (Product/Admin), si fue en intento 1 o 2, y las últimas líneas de stderr/stdout (p. ej. últimas 15–20 líneas). |
| **Registro en log** | Escribir en el log del servicio (p. ej. `_logService`) el resultado de cada intento (éxito/fallo, proyecto, intento, exit code y detalle del error). |
| **No tragar excepciones** | En `catch`, registrar la excepción completa y, si se decide "fallo", propagar o devolver un resultado de error tipado; no hacer `return (true, null)` silencioso cuando realmente hubo fallo. |
| **Reintento acotado** | El reintento debe ser **una sola vez** por ejecución del paso [4/12]/[5/12], y solo para los proyectos que fallaron en el primer intento. |

### Flujo resumido

```
[4/12] y [5/12] Compilación fronts
  → Ejecutar Product y Admin en paralelo (intento 1)
  → Si ambos OK → continuar a [6/12]
  → Si alguno falla:
      → Registrar error (aislado: proyecto, detalle, exit code)
      → Reintentar solo los que fallaron (intento 2)
      → Si tras reintento ambos OK → continuar a [6/12]
      → Si tras reintento sigue fallo → mostrar error, pedir tecla, salir del paso con error
```

### Ubicación en código

- **Lógica de reintento y orquestación:** en el método que ejecuta la acción 1 (donde se llama a `RunNpmCompilationCheckAsync` para Product y Admin), alrededor de las líneas 392–426 de `MenuService.cs`.  
- **Resultado y aislamiento del error:** en `RunNpmCompilationCheckAsync` (aprox. 694–751): devolver resultado tipado, no lanzar/tragar excepciones de forma genérica, y dejar que el llamador decida el reintento según ese resultado.
