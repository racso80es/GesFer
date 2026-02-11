# SPEC-ACCESIBILIDAD-ELECTRON-003: Clarifications

**ID:** SPEC-ACCESIBILIDAD-ELECTRON-003_CLARIFICATIONS
**Date:** 2026-02-09
**Author:** Clarifier
**Related:** SPEC-ACCESIBILIDAD-ELECTRON-003

## 1. Questions & Answers

### Q1: What is the correct target directory for the Electron application?
**A1:** The correct directory is `Kalma2/Interfaces/Desktop`.
**Evidence:** `ejecutar-interface.bat` and `ejecutar-electron.bat` both target this directory. The memory confirms the project structure was reorganized, moving `Interfaces/Desktop` out of `src/`.

### Q2: Should existing scripts like `ejecutar-interface.bat` be removed?
**A2:** Yes, `ejecutar-interface.bat` is redundant if `ejecutar-electron.bat` serves the same purpose.
**Action:** The implementation plan should consider deprecating or removing `ejecutar-interface.bat` to reduce confusion, or ensure they remain identical. For this task, we focus on perfecting `ejecutar-electron.bat` as the primary interface.

### Q3: What specific security checks are required?
**A3:**
- **Path Traversal:** Use `%~dp0` to anchor execution to the project root.
- **Dependency Integrity:** Verify `package.json` exists before running `npm`.
- **Process Isolation:** Kill stale `electron.exe` processes to prevent locking issues.
- **Output Sanitization:** Ensure no sensitive information is leaked (though unlikely in a launcher script).

### Q4: How should the audit be logged?
**A4:** The creation of the script and its successful execution should be logged in `docs/audits/ACCESS_LOG.md` (if it exists) or `docs/EVOLUTION_LOG.md` as per the standard protocol. The `Audit` agent is responsible for this validation.

## 2. Updated Requirements
- **Action:** Ensure `ejecutar-electron.bat` is the single source of truth for launching the Electron app.
- **Action:** Add a comment header in the script indicating it is auto-generated and maintained by the Tekton agent.
