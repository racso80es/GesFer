# SPEC-KALMA2-UNIT-TESTS: Unit Testing Infrastructure for Kalma2 Desktop

## 1. Introduction
The `Kalma2/Interfaces/Desktop` application, which consumes core logic from `Kalma2/Core`, currently lacks any automated testing infrastructure. To ensure software reliability, facilitate refactoring, and comply with the "Vision Zero" mandate, a robust Unit Testing framework must be established.

## 2. Objectives
- **Framework Setup:** Integrate **Vitest** as the test runner (due to its native Vite compatibility) and **React Testing Library** for component testing.
- **Scope:** Enable unit testing for:
    - **React Components** in `Interfaces/Desktop`.
    - **Business Logic** in `Interfaces/Desktop/src/services`.
    - **Core Domain Logic** imported from `Kalma2/Core` (e.g., `JudgeService`, `ModeController`).
- **Developer Experience:** Provide `npm run test` and `npm run test:ui` scripts.
- **Coverage:** Establish a baseline for code coverage reporting.

## 3. Technical Requirements
- **Test Runner:** `vitest` (Fast, Vite-native).
- **DOM Environment:** `jsdom` (Simulate browser).
- **Component Testing:** `@testing-library/react`, `@testing-library/dom`.
- **Assertions:** `@testing-library/jest-dom` (for DOM-specific matchers).
- **Configuration:** `vitest.config.ts` must coexist with `vite.config.ts`.
- **Mocking:** Ability to mock `electron` modules and `window.calmaAPI` IPC bridge.

## 4. Acceptance Criteria
- [ ] `npm run test` executes successfully and reports pass/fail.
- [ ] At least one "Hello World" component test passes.
- [ ] At least one logic test for a `Core` service (e.g., `JudgeService`) passes.
- [ ] `npm run test:coverage` generates a coverage report.
- [ ] CI/CD friendly (headless execution).

## 5. Security & Compliance
- **Zero Trust:** Tests must not execute in a production environment or affect production data.
- **State Isolation:** Each test must run in a clean state (mocked `container` if necessary).
- **No Secrets:** Tests must not require or log real secrets/tokens.
