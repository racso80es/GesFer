# CLARIFICATION: Electron Desktop Launcher
**ID:** LAUNCHER-DESKTOP-CLARIFICATION-001
**Date:** 2024-05-22
**Author:** Clarifier
**Status:** CLOSED

## Questions & Answers

### 1. Execution Environment
**Q:** Should the launcher script run the application in development (`dev`) mode or production (`build`) mode?
**A:** The launcher is intended for developers using the Calma-Desktop tool to orchestrate their environment. Therefore, executing `npm run dev` (Vite dev server + Electron) is appropriate as it allows hot reloading and immediate feedback. For production distribution, a separate build pipeline would be used.

### 2. Dependency Management
**Q:** Should the script automatically handle dependency installation?
**A:** Yes. To ensure a smooth "one-click" experience, the script should check for the presence of `node_modules` within `src/Tools/Calma-Desktop`. If missing, it should execute `npm install` before attempting to start the application.

### 3. Error Handling
**Q:** How should errors be communicated to the user?
**A:** The script should pause on exit if an error occurs (non-zero exit code), allowing the user to read the error message before the window closes. It should also use `setlocal enabledelayedexpansion` and error checking after critical commands.

## Decision
Proceed with implementing `ejecutar-desktop.bat` to run `npm run dev`, with automatic `npm install` if `node_modules` is missing.
