# SPEC-KALMA2-UNIT-TESTS: Clarifications

## 1. Framework Selection
**Q:** Why Vitest instead of Jest?
**A:** `Kalma2/Interfaces/Desktop` is a Vite project. Vitest shares the same configuration, plugins, and transformation pipeline, eliminating the need for complex Jest transforms (Babel/ts-jest) for TypeScript and ESM support. It is faster and lighter for this ecosystem.

## 2. Testing `Kalma2/Core`
**Q:** Should `Kalma2/Core` be tested separately?
**A:** Ideally, yes. However, `Core` is currently just a source directory without its own `package.json`. For now, it will be tested as part of the consuming application (`Desktop`). Tests for `Core` logic will reside in `Kalma2/Interfaces/Desktop/src/__tests__/Core/` or co-located (depending on decision).
**Decision:** Co-location is preferred for components, but for `Core` logic (which is outside `src`), we will place tests in `Kalma2/Interfaces/Desktop/tests/Core/` to mirror the structure.

## 3. Mocking Strategy
**Q:** How to handle `window.calmaAPI`?
**A:** We will create a `src/setupTests.ts` file that mocks the `window.calmaAPI` object globally, ensuring components don't crash when accessing IPC methods.

**Q:** How to handle InversifyJS container?
**A:** We should use `container.snapshot()` and `container.restore()` in `beforeEach`/`afterEach` to ensure test isolation when testing services that modify the global container.
