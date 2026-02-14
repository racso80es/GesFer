# SPEC-KALMA2-E2E-TESTS: Clarifications

## 1. Testing Strategy
**Q:** Should we test against the Dev server or the Built application?
**A:** Testing against the **Built application** is preferred for "True E2E" as it verifies the final artifact. However, for development speed, testing against Dev is useful.
**Decision:** We will configure Playwright to launch the Electron executable (or `electron .`) which mimics the production startup. For now, we will target the source/dev launch `electron .` or `npm start` equivalent to avoid waiting for a full build on every test run during development.

## 2. Mocking in E2E
**Q:** Should we mock IPC in E2E?
**A:** Generally, no. E2E should test the real integration. However, for external services (like IOTA or Docker commands), we might need to mock at the **Main Process** level or use a "Test Mode" flag in the application to swap real services for mocks.
**Decision:** For the initial setup, we will test the "Happy Path" without complex mocks. Future iterations will add a "Test Mode" to the Main process.
