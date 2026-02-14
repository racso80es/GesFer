# PLAN-KALMA2-UNIT-TESTS: Implementation Plan

## 1. Overview
This plan outlines the steps to install and configure Vitest and React Testing Library in `Kalma2/Interfaces/Desktop`.

## 2. Phases & Tasks

### Phase 1: Installation & Configuration
1.  **Install Dependencies:**
    - Run `npm install -D vitest jsdom @testing-library/react @testing-library/jest-dom @testing-library/dom` in `Kalma2/Interfaces/Desktop`.
2.  **Configure Vitest:**
    - Create `vitest.config.ts` in `Kalma2/Interfaces/Desktop`.
    - Configure `test.environment: 'jsdom'`.
    - Configure `test.setupFiles: './src/setupTests.ts'`.
    - Ensure `resolve.alias` matches `vite.config.ts` (if any).
3.  **Setup Environment:**
    - Create `src/setupTests.ts`.
    - Import `@testing-library/jest-dom`.
    - Mock `window.calmaAPI`.
4.  **Update `package.json`:**
    - Add scripts:
        - `"test": "vitest"`
        - `"test:ui": "vitest --ui"`
        - `"test:run": "vitest run"`
        - `"test:coverage": "vitest run --coverage"`

### Phase 2: Test Implementation
1.  **Component Test:**
    - Create `src/__tests__/App.test.tsx`.
    - Write a basic test: Render `<App />` and check for "Calma Desktop" title.
    - Mock `IGreetingService` or `container` if necessary to prevent startup errors.
2.  **Core Logic Test:**
    - Create `src/__tests__/Core/JudgeService.test.ts`.
    - Test `JudgeService` (if accessible) or a simple utility from `Core` to prove TypeScript path mapping works.

### Phase 3: Verification
1.  **Execute Tests:** Run `npm test`.
2.  **Verify Coverage:** Run `npm run test:coverage` (might need `@vitest/coverage-v8`).
3.  **Audit:** Ensure no console errors or warnings.

## 3. Rollback Plan
- If installation fails or breaks the build (`npm run build`), uninstall packages and revert `package.json` changes.
