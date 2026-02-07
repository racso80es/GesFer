# PLAN: Electron Desktop Launcher Implementation
**ID:** LAUNCHER-DESKTOP-PLAN-001
**Date:** 2024-05-22
**Status:** ACTIVE

## Objective
Implement a robust `.bat` script (`ejecutar-desktop.bat`) to launch the Calma-Desktop application, ensuring environment preparation and error visibility.

## Implementation Steps

### 1. Script Initialization
- Create `ejecutar-desktop.bat` at the repository root.
- Use `@echo off` and `chcp 65001` for Unicode support.
- Set `ROOT_DIR=%~dp0` to ensure path consistency regardless of execution context.

### 2. Dependency Verification
- Navigate to `src/Tools/Calma-Desktop` using absolute paths.
- Check for the existence of the `node_modules` directory.
- If missing, echo "Instalando dependencias..." and execute `npm install`.
- Check `errorlevel` after installation; exit with error if installation fails.

### 3. Application Launch
- Echo "Iniciando Calma-Desktop..."
- Execute `npm run dev` to start Vite and Electron.
- Check `errorlevel` after execution; pause if an error occurred.

### 4. Verification
- Verify the script exists.
- Verify the content correctly points to `src/Tools/Calma-Desktop`.
- Verify error handling logic is present.

## Dependencies
- Node.js and npm must be installed on the user's machine (prerequisite).
- `src/Tools/Calma-Desktop` must exist.
