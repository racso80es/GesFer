# SPECIFICATION: Electron Desktop Launcher
**ID:** LAUNCHER-DESKTOP-001
**Date:** 2024-05-22
**Author:** Architect
**Status:** DRAFT

## 1. Context
The Calma-Desktop application is an Electron-based development orchestrator located in `src/Tools/Calma-Desktop`. Currently, running this application requires navigating to the directory and manually executing npm commands. To improve accessibility and align with existing console execution patterns (`ejecutar-consola.bat`), a dedicated launcher script is needed.

## 2. Goal
Create a Windows batch file (`ejecutar-desktop.bat`) that automates the setup and execution of the Calma-Desktop Electron application, providing a seamless developer experience similar to the existing console launcher.

## 3. Analysis (Current vs Desired)
- **Current State:** Developers must manually open a terminal, navigate to `src/Tools/Calma-Desktop`, and run `npm run dev`. This is inconsistent with the `ejecutar-consola.bat` experience.
- **Desired State:** A single double-clickable `.bat` file in the root directory handles navigation, dependency checking, and application launch.
- **Gap:** Absence of the `ejecutar-desktop.bat` script.

## 4. Security Implications
- The script must use absolute paths derived from `%~dp0` to prevent path traversal or execution in incorrect directories.
- It should validate the existence of critical files (e.g., `package.json`) before execution to fail fast and safely.
- No sensitive tokens are embedded; the script relies on the user's environment and the application's internal configuration.

## 5. Implementation Plan
1. [ ] Create `ejecutar-desktop.bat` in the repository root.
2. [ ] Implement logic to navigate to `src/Tools/Calma-Desktop`.
3. [ ] specific check for `node_modules` and run `npm install` if missing.
4. [ ] Execute `npm run dev` to start the development server and Electron app.
5. [ ] Add error handling and pause on exit for visibility.

## 6. Verification
- [ ] Verify the script exists in the root directory.
- [ ] Verify the script content correctly points to `src/Tools/Calma-Desktop`.
- [ ] Verify the script handles missing `node_modules` by attempting installation.
