# SPEC-ACCESIBILIDAD-ELECTRON-003: Secure Electron Launcher

**ID:** SPEC-ACCESIBILIDAD-ELECTRON-003
**Date:** 2026-02-09
**Author:** Knowledge Architect
**Status:** DRAFT
**Related:** SPEC-ACCESIBILIDAD-ELECTRON-002, launcher-desktop.md

## 1. Introduction
The project requires a streamlined, secure way to launch the Electron frontend interface, mirroring the ease of use provided by `ejecutar-consola.bat` for the console application. The current ecosystem includes multiple or potentially outdated scripts (`ejecutar-interface.bat`, `ejecutar-electron.bat`). This specification defines the standard for a single, authoritative launcher: `ejecutar-electron.bat`.

## 2. Objectives
- **Standardize:** Establish `ejecutar-electron.bat` as the canonical entry point for the Desktop Interface.
- **Secure:** Implement strict path handling using `%~dp0` to prevent execution context vulnerabilities.
- **Automate:** Handle dependency checks (`node_modules`) and process cleanup (`electron.exe`) automatically.
- **Align:** Target the correct source directory: `Kalma2/Interfaces/Desktop`.

## 3. Scope
- **Target Component:** Electron Frontend (`Kalma2/Interfaces/Desktop`).
- **Deliverable:** `ejecutar-electron.bat` (Windows Batch Script).
- **Users:** Developers, QA, Stakeholders.

## 4. Technical Requirements (Tekton & Security)
1.  **Path Resolution:**
    - MUST use `%~dp0` to resolve the repository root.
    - MUST explicitly navigate to `Kalma2/Interfaces/Desktop`.
    - MUST verify the existence of `package.json` before attempting execution.

2.  **Environment Validation:**
    - MUST check for `node` and `npm` availability in `%PATH%`.
    - IF tools are missing, MUST exit with a clear error message.

3.  **Dependency Management:**
    - MUST check for `node_modules` directory or `package-lock.json`.
    - IF missing, MUST attempt `npm install` automatically or prompt the user.

4.  **Process Hygiene:**
    - MUST attempt to terminate existing `electron.exe` or `Calma Desktop.exe` processes to prevent locking issues.
    - MUST suppress errors from `taskkill` if no process is running.

5.  **Execution:**
    - MUST execute `npm run dev` to start the application.
    - MUST handle exit codes and pause on error to allow reading logs.

## 5. Audit & Compliance
- **Traceability:** The script creation and updates must be documented.
- **Safety:** The script must not execute arbitrary external code (only local npm scripts).
- **Language:** Output messages should be clear (Spanish preferred for consistency with "Ejecutar...").

## 6. Success Criteria
- The script launches the Electron app successfully from the root.
- The script correctly handles a fresh clone (installs dependencies).
- The script cleans up zombie processes.
