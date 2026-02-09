# Kalma2 - Hello World Desktop Objective

## 1. Goal
Validation of the base structure through a "Hello World" in a Desktop environment. The application inherits the solid foundation of GesFer but is adapted to be lighter for this start.

## 2. Architecture
- **Base:** Electron + Vite + React + TypeScript + TailwindCSS.
- **Dependency Injection (DI):** InversifyJS will be used to manage dependencies, ensuring decoupling and testability.
- **State Management:** React Context / Hooks for simple UI state, Services for domain logic.
- **Terminology:**

## 3. Dependency Injection Strategy
To resolve initial frictions with DI, we adopt a standard container pattern using InversifyJS.
- **Container:** A central container (`src/core/di/container.ts`) will register all services.
- **Services:** All business logic will reside in services (e.g., `IGreetingService`).
- **Integration:** React components will consume services via a custom hook or Context provider that accesses the container.

## 4. Implementation Details
- **Location:** `src/Kalma2/Desktop` (migrated from `src/Tools/Calma-Desktop`).
- **Hello World:** A simple service `GreetingService` will provide a message to verify the DI setup.
- **Kaizen:** Strict "No Any" policy will be enforced.

## 5. Golden Rules
- **Documentation:** All architectural decisions must be documented here.
- **Tests:** Future steps will include unit tests for services.
- **Logs:** Ensure no duplicate logs in `EVOLUTION_LOG.md`.
