# PLAN-KALMA2-E2E-TESTS: Implementation Plan

## 1. Overview
This plan outlines the steps to install and configure Playwright for Electron testing in `Kalma2/Interfaces/Desktop`.

## 2. Phases & Tasks

### Phase 1: Installation & Configuration
1.  **Install Dependencies:**
    - `npm install -D playwright @playwright/test` in `Kalma2/Interfaces/Desktop`.
2.  **Configure Playwright:**
    - Create `playwright.config.ts` in `Kalma2/Interfaces/Desktop`.
    - Configure it to launch Electron.
3.  **Update `package.json`:**
    - Add script: `"e2e": "playwright test"`

### Phase 2: Test Implementation
1.  **Create Test File:**
    - `tests/e2e/startup.spec.ts`.
    - Content: Launch Electron, check `window.title()`, check for specific element.
2.  **Helpers:**
    - Create `tests/e2e/helpers.ts` (optional) for common actions.

### Phase 3: Verification
1.  **Execute:** Run `npm run e2e`.
2.  **Verify:** Check if Electron launches and closes automatically.

## 3. Rollback Plan
- Uninstall Playwright and remove scripts if it destabilizes the project.
