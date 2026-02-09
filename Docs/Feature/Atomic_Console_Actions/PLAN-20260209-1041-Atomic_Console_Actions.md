# PLAN: Atomic Console Actions (Action 3)

## 1. Information

| Field | Detail |
| :--- | :--- |
| **ID** | PLAN-GF-2026-003 |
| **Source Spec** | Docs/Feature/Atomic_Console_Actions/SPEC-20260209-1041-Atomic_Console_Actions.md |
| **Source Clarify** | Docs/Feature/Atomic_Console_Actions/SPEC-20260209-1041-Atomic_Console_Actions_CLARIFICATIONS.md |
| **Date** | 2026-02-09 |
| **Status** | Approved |

## 2. Goal
Implement a new "Atomic Actions" menu (Action 3) in `GesFer.Console` that allows granular control over the initialization process (Docker, Seeds, Services, DB Init), while cleaning up redundant actions (Action 8) and relocating legacy ones (Action 3).

## 3. Context
The current console menu has scattered actions for initialization (Action 1, 2, 3, 8). The goal is to consolidate them into a coherent structure where users can perform specific tasks atomically without running the full initialization pipeline every time. Specifically, starting services should be granular (Back/Front, Admin/Product) to save resources and time during development.

## 4. Implementation Plan

### 4.1. Refactor `StartLocalEnvironmentCommand`
- [ ] Update `StartLocalEnvironmentInput` DTO to include flags:
    - `bool StartProductApi`
    - `bool StartAdminApi`
    - `bool StartProductFront`
    - `bool StartAdminFront`
- [ ] Modify `StartLocalEnvironmentCommand.HandleAsync` to respect these flags.
    - If no flags are set (default), assume "Start All" behavior (backward compatibility for Action 2).
    - Only free ports for the services being started.
    - Only compile/prepare/start the services requested.

### 4.2. Update `MenuService`
- [ ] **Remove Legacy Actions:**
    - Remove "3. Inicialización de base de datos" from the main menu switch/case.
    - Remove "8. Ejecutar seeds de datos" from the main menu switch/case.
- [ ] **Implement New Action 3 ("Acciones Atómicas"):**
    - Create a new method `ExecuteAtomicActionsMenuAsync()`.
    - Display sub-menu:
        1. **Inicializar Docker:** Call `CheckDockerCommand` -> `RemoveContainersCommand` -> `CreateContainersCommand` -> `WaitMySqlReadyCommand`. (Logic extracted from `ExecuteFullInitializationAsync` or reused).
        2. **Restaurar Datos Seed:** Call `ExecuteSeedsMenuAsync()` (reused from old Action 8).
        3. **Levantar Servicios:** Show sub-menu for service selection (Product Back, Admin Back, Product Front, Admin Front, All). Call `StartLocalEnvironmentCommand` with appropriate flags.
        4. **Inicialización Completa BD:** Call `ExecuteDatabaseInitializationStep8Async()` (reused from old Action 3).
        5. **Volver.**

### 4.3. Cleanup & Verification
- [ ] Ensure Action 2 ("Levantar entorno local") still works as a shortcut for "Start All".
- [ ] Verify that `ExecuteFullInitializationAsync` (Action 1) still works correctly (it uses the underlying commands, so it should remain unaffected if commands are reused properly).
- [ ] Verify that the new Action 3 sub-menus work as expected.

## 5. Risks & Mitigation
- **Risk:** Refactoring `StartLocalEnvironmentCommand` might break Action 1 or Action 2 if default behavior isn't preserved.
    - *Mitigation:* Ensure default `StartLocalEnvironmentInput` values trigger "Start All", or handle null/empty flags explicitly.
- **Risk:** Port conflicts if granular startup doesn't clean up specific ports.
    - *Mitigation:* Ensure `FreePort` is called for the specific ports of the services being started.

## 6. Audit Trace
- **Plan Generation:** Manual creation in `Docs/Feature/Atomic_Console_Actions/`.
- **Log Entry:** Added to `docs/audits/ACCESS_LOG.md`.
