# Skill: Finalizar tarea (Finish Feature)

**Trigger:** Cuando el usuario diga "finaliza tarea", "finalizar tarea", "cierra tarea", "finish task" o similar.

**Objetivo:** Cerrar el trabajo de forma ordenada: certificar la rama, unificar con master, limpiar y subir.

---

## Pasos obligatorios

1. **Estado del repositorio**
   - Comprobar rama actual y si hay cambios sin commit (`git status`, `git branch --show-current`).
   - Si estás en `master` con cambios sin commit: crear una rama de cierre (p. ej. `chore/finish-<desc>` o `docs/finish-<desc>`), hacer commit y seguir. Si estás en una rama distinta de master con cambios, seguir en esa rama.

2. **Ejecutar Unificar-Rama.ps1**
   - Invocar el script de certificación y unificación con la rama actual (no master).
   - Desde la raíz del repo (Windows PowerShell):
     ```powershell
     .\scripts\skills\Unificar-Rama.ps1 -BranchName "<rama_actual>" -CommitMessage "chore: finalizar tarea"
     ```
   - Si la rama es `master`: omitir este paso (el script exige documentación de rama en `docs/branches/<rama>/OBJETIVO.md`; master no la tiene).
   - Si el script falla por falta de `docs/branches/<rama>/OBJETIVO.md`: crear el documento mínimo o unificar sin script (commit manual, merge a master, push).

3. **Unificar con master**
   - Hacer checkout a `master`.
   - Hacer merge de la rama de trabajo: `git merge <rama> -m "Merge <rama>: <descripción breve>"`.
   - Eliminar la rama local ya fusionada: `git branch -d <rama>`.

4. **Subir y limpiar**
   - Hacer push de master: `git push origin master`.
   - Si el remoto tiene commits que no tienes: `git pull origin master --rebase` y luego `git push origin master`.
   - Comprobar estado final: `git status`, `git branch -vv` (working tree limpio, master actualizado).

5. **Resumen al usuario**
   - Indicar qué se hizo: rama unificada, push a origin/master, y si se ejecutó Unificar-Rama.ps1.

---

## Notas

- **AGENTS.md:** No hacer commits directos a master; usar siempre una rama y luego merge.
- **Unificar-Rama.ps1** realiza: escudo de compilación (dotnet build con reintentos), comprobación de documentación de rama (`docs/branches/<BranchName>/OBJETIVO.md`), `git add .` y `git commit` con mensaje certificado. No hace push (queda para los pasos 3–4).
- Si no puedes ejecutar el script (p. ej. en rama master o sin OBJETIVO.md), realizar commit manual con mensaje convencional y seguir con merge a master y push.
