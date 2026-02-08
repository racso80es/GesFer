# PLAN: Mejorar Accesibilidad a Proyecto Electron
**Based on Spec:** SPEC-ELECTRON-INTERFACE-001
**Based on Clarification:** openspecs/specs/mejorar-accesibilidad-electron_CLARIFICATIONS_20260207-1115.md
**Date:** 2026-02-07 11:20
**Agent:** Tekton

## Goal
Create `ejecutar-interfaz.bat` to facilitate the execution of the Electron frontend (`Calma-Desktop`) with a user-friendly and robust script.

## Steps
1. **File Creation**: Create `ejecutar-interfaz.bat` in the project root.
2. **Path Configuration**: Set `APP_DIR` to `%~dp0src\Tools\Calma-Desktop`.
3. **Environment Validation**:
   - Check if `npm` is in PATH. If not, exit with error message.
   - Check if `APP_DIR` exists. If not, exit with error.
4. **Dependency Management**:
   - Check for `node_modules` and `package-lock.json`.
   - If missing, run `npm install` and verify success.
5. **Execution**:
   - Run `npm run dev`.
   - Implement error handling (`if %errorlevel% neq 0`).
   - Use `pause` at the end to allow user to read output.
6. **Cleanup**:
   - Kill any existing electron processes (`electron.exe`, `Calma Desktop.exe`) before starting to avoid file lock issues.

## Verification
- Verify the file exists.
- Verify the content matches the plan.
- (Manual) Run the script to confirm it works (simulated).
