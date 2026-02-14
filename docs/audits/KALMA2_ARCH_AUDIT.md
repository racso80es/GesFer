# Architectural Audit: Kalma2 Ecosystem

**Date:** 2026-02-14
**Auditor:** Architect Agent
**Scope:** `Kalma2/Core`, `Kalma2/Interfaces/Desktop`

## 1. Executive Summary
The Kalma2 ecosystem is structured as a modular Monolith, separating Core Business Logic (`Core`) from the Presentation Layer (`Interfaces/Desktop`). While the architectural separation is sound, utilizing Dependency Injection (InversifyJS) for decoupling, the complete absence of a testing infrastructure poses a significant stability risk.

## 2. Component Analysis

### 2.1 Kalma2/Core (The Brain)
- **Role:** Central repository for business logic, "Conscience" (Decision Engine), and "Duality" (Operational Modes).
- **Tech Stack:** TypeScript, InversifyJS (DI), Reflect-Metadata.
- **Key Artifacts:**
    - `di/container.ts`: Dual-container strategy (Node vs Web) for dependency resolution.
    - `conscience/`: Contains critical logic like `JudgeService` and `ConscienceService`.
    - `duality/`: Manages system states (`BossMode`, `CalmMode`).
- **Status:** **CRITICAL**. This module contains pure logic that is highly testable but currently completely unverified.

### 2.2 Kalma2/Interfaces/Desktop (The Body)
- **Role:** User Interface and System Orchestrator.
- **Tech Stack:** Electron (Main), React (Renderer), Vite, TailwindCSS.
- **Integration:** Consumes `Core` services via the DI container. Uses `window.calmaAPI` for IPC communication with the Electron Main process.
- **Status:** **VULNERABLE**. UI components (`App.tsx`) contain mixed concerns (Presentation + Service Orchestration) and lack regression barriers.

## 3. Architecture Violations & Risks
1.  **No Safety Net:** Modifications to `Core` have no automated way to verify impact on `Desktop`.
2.  **IPC Blind Spot:** The `window.calmaAPI` bridge is a critical security boundary. Lack of contract testing here invites runtime errors and security gaps.
3.  **Tightly Coupled State:** `App.tsx` manages significant local state (`serviceStatus`, `auditStatus`) that should ideally be lifted to a testable store or hook.

## 4. Recommendations
1.  **Establish Unit Testing Root:** Implement Vitest at the `Interfaces/Desktop` level to cover both React components and imported `Core` logic.
2.  **Isolate Core Tests:** While `Core` is tested via `Desktop` now, future architectural evolution should consider making `Core` a standalone npm workspace to enforce strict boundaries.
3.  **Mock IPC:** Implement a robust mocking strategy for `window.calmaAPI` to test the Renderer in isolation.
