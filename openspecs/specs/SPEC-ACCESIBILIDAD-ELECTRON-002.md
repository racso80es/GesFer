# SPEC-ACCESIBILIDAD-ELECTRON-002: Improved Electron Launcher

## 1. Introduction
The current codebase includes scripts for launching the console application (`ejecutar-consola.bat`) and the Electron frontend (`ejecutar-desktop.bat`, `ejecutar-interfaz.bat`). To improve accessibility and provide a robust, consistent experience for developers launching the Electron frontend, a new, optimized batch script is required. This script will be named `ejecutar-electron.bat`.

## 2. Objectives
- Create a new batch file `ejecutar-electron.bat` in the project root.
- Ensure the script is secure, using absolute paths (`%~dp0`) and validating inputs/dependencies.
- Automate the setup process (dependency checks, process cleanup) to mirror the ease of use of the console launcher.
- Adhere to security and audit logging requirements.

## 3. Scope
- **Input:** None (execution via command line or double-click).
- **Output:** Launch of the Electron application (`npm run dev` in `src/Tools/Calma-Desktop`).
- **Security:**
    - Validate existence of `npm` and `node`.
    - Use strict path resolution.
    - Clean up stale `electron.exe` processes securely.
    - Avoid executing arbitrary code; only run defined `npm` scripts.

## 4. detailed Requirements
- **Path Handling:** The script must use `%~dp0` to correctly identify the project root regardless of execution context.
- **Dependency Check:** It must check if `node_modules` exists in the target directory. If not, it should prompt or automatically install dependencies (safely).
- **Process Cleanup:** It should attempt to kill existing `electron.exe` instances to prevent conflicts, suppressing errors if no process is running.
- **Feedback:** Provide clear console output indicating steps (e.g., "Checking dependencies...", "Launching application...").
- **Error Handling:** If `npm` is missing or the directory is invalid, it should exit with a clear error message and pause for the user to read it.

## 5. Audit
- The creation and usage of this script must be logged in `docs/audits/ACCESS_LOG.md` with the appropriate auditor token.
