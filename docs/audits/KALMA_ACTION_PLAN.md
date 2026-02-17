# Kalma2 Action Plan
**Date:** 2026-02-15
**Project:** Kalma2/Interface/Desktop
**Author:** Jules (Agent)

## Executive Summary
This document outlines the prioritized actions required to establish a robust testing infrastructure for the Kalma2 Desktop interface. The goal is to move from 0% coverage to a state where new code is automatically verified.

## Priority 1: Infrastructure & "Walking Skeleton" (Immediate)
**Objective:** Enable the execution of automated tests.
**Status:** Planned.
**Impact:** Critical (Enables all future testing).
**Effort:** Low (Configuration).

### Tasks:
1.  **Correct Skill Definition:** Update `openspecs/skills/frontend-test.json` to point to the correct directory (`Kalma2/Interface/Desktop`).
2.  **Install Dependencies:** Add `vitest`, `jsdom`, `@testing-library/react`, `@testing-library/jest-dom`, and `@vitejs/plugin-react` to `package.json`.
3.  **Configure Scripts:** Add `test`, `test:run`, and `test:coverage` scripts.
4.  **Create Configuration:** Set up `vitest.config.ts` for React/JSDOM environment.
5.  **Smoke Test:** create a simple test (`src/App.test.tsx`) to verify the setup.

## Priority 2: Core Coverage (Short Term)
**Objective:** Verify critical business logic and security controls.
**Status:** Pending.
**Impact:** High (Reduces risk on critical paths).
**Effort:** Medium.

### Tasks:
1.  Identify critical components (e.g., Auth forms, Data input).
2.  Create unit tests for `Kalma2/Core` logic (if applicable/importable).
3.  Establish mocking strategy for Electron IPC and native modules.

## Priority 3: Component Coverage (Medium Term)
**Objective:** Broaden coverage to UI components.
**Status:** Pending.
**Impact:** Medium (Catches UI regressions).
**Effort:** High.

### Tasks:
1.  Systematically add tests for existing components.
2.  Enforce test creation for all new components (TDD).

## Priority 4: E2E Integration (Long Term)
**Objective:** Verify full user flows.
**Status:** Pending (via Playwright).
**Impact:** High.
**Effort:** High.

### Tasks:
1.  Review existing E2E setup.
2.  Add scenarios for critical workflows.
