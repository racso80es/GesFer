# SPEC-KALMA2-E2E-TESTS: E2E Testing Infrastructure for Kalma2 Desktop

## 1. Introduction
While Unit Tests verify individual components and logic, End-to-End (E2E) tests are required to verify the integration of the Electron Main process, the React Renderer, and the IPC bridge. This ensures the application launches correctly and critical flows work as expected.

## 2. Objectives
- **Framework Setup:** Integrate **Playwright** with Electron support.
- **Scope:** Verify:
    - Application Launch.
    - Main Window creation.
    - IPC communication (via UI interactions).
- **Automation:** Enable headless (or virtual display) testing for CI.

## 3. Technical Requirements
- **Test Runner:** `playwright`.
- **Target:** The built Electron application (`dist-electron/main.js` or development mode).
- **Configuration:** `playwright.config.ts` specific to Electron.
- **Environment:** Must support running in environments without a physical display (Xvfb or similar for Linux CI).

## 4. Acceptance Criteria
- [ ] `npm run e2e` launches the application and runs tests.
- [ ] Test verifies the window title is "Calma Desktop".
- [ ] Test verifies the initial state of the application.
- [ ] Test shuts down the application gracefully.

## 5. Security & Compliance
- **Isolation:** Tests should not interact with the production IOTA Tangle (use Testnet or mocks).
- **Cleanup:** Tests must ensure all Electron processes are killed after execution.
