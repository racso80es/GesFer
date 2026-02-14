# Action Plan: Kalma2 Test Infrastructure

**Date:** 2026-02-14
**Status:** Approved for Execution

## Prioritized Actions

This plan prioritizes actions based on **Impact/Effort** ratio and **Risk Mitigation**.

| Priority | Action ID | Name | Weight (1-10) | Reasoning |
|----------|-----------|------|---------------|-----------|
| **1** | **ACT-001** | **Unit Testing Foundation (Vitest)** | **10** | **Foundational.** Impossible to have reliable software without unit tests. Covers `Core` logic (High Risk) and UI components. Fastest feedback loop. |
| **2** | **ACT-002** | **E2E Testing (Playwright)** | **8** | **Integration.** Verifies the entire stack (Electron + React + Core). Critical for catching IPC and startup issues. Slower feedback but high confidence. |
| **3** | **ACT-003** | **CI/CD Integration** | **7** | **Automation.** Ensures tests run on every commit/PR. Depends on ACT-001 and ACT-002. |

---

## Execution Roadmap

### Phase 1: The Foundation (ACT-001)
**Goal:** Enable `npm test` in `Kalma2/Interfaces/Desktop`.
**Scope:**
- Install `vitest`, `jsdom`, `@testing-library/react`.
- Configure `vitest.config.ts`.
- Write Unit Tests for:
    - `Kalma2/Core/conscience/services/JudgeService.ts` (Logic)
    - `Kalma2/Interfaces/Desktop/src/components/ActionButton.tsx` (UI - if extracted) or `App.tsx` smoke test.

### Phase 2: The Integration (ACT-002)
**Goal:** Enable `npm run e2e`.
**Scope:**
- Install `playwright`.
- Configure `playwright.config.ts` for Electron.
- Write E2E Test:
    - Application Launch.
    - Window Title Verification.
    - Basic Service Health Check (Mocked).

### Phase 3: The Guardian (ACT-003)
**Goal:** Block broken commits.
**Scope:**
- Update `scripts/skills/commit-skill.sh` to include `Kalma2` tests.
- Update `openspecs/skills/frontend-test.json`.
