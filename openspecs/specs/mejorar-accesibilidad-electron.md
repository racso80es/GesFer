# SPECIFICATION: Mejorar Accesibilidad a Proyecto Electron
**ID:** SPEC-ELECTRON-INTERFACE-001
**Date:** 2026-02-07
**Author:** google-labs-jules[bot]
**Status:** DRAFT

## 1. Context
The project currently has a console application with a helper script (`ejecutar-consola.bat`) for easy execution. The Electron frontend (`Calma-Desktop`) exists but lacks a dedicated, easily accessible helper script named specifically for the interface, as requested by the user. While `ejecutar-desktop.bat` exists, the user explicitly requested creating "another bat" to improve accessibility to the "front made with electron".

## 2. Goal
Create a new batch script, likely named `ejecutar-interfaz.bat` (or similar), to facilitate the execution of the Electron frontend, mirroring the convenience of `ejecutar-consola.bat`.

## 3. Analysis (Current vs Desired)
- **Current State:** Users have `ejecutar-consola.bat` for the console. There is `ejecutar-desktop.bat` which targets `src\Tools\Calma-Desktop`, but the user perceives a gap or wants a specific "interface" launcher.
- **Desired State:** A clear, accessible batch file (e.g., `ejecutar-interfaz.bat`) that launches the Electron frontend.
- **Gap:** The specific "interface" launcher requested by the user is missing or the existing one is not recognized as such.

## 4. Security Implications
- The batch script must validate the environment (npm existence).
- It must handle errors gracefully.
- It should not hardcode sensitive credentials.
- It should verify dependencies before running.

## 5. Implementation Plan
1. [ ] Create `ejecutar-interfaz.bat` in the root directory.
2. [ ] Ensure it targets `src\Tools\Calma-Desktop`.
3. [ ] Implement checks for `npm` and `node_modules`.
4. [ ] Implement error handling.

## 6. Verification
- [ ] Verify the script exists.
- [ ] Verify it successfully launches the Electron app (simulated by checking script content and paths).
