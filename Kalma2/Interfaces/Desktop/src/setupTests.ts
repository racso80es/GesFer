import '@testing-library/jest-dom';

// Mock window.calmaAPI for all tests
// This prevents "cannot read properties of undefined" when components try to use IPC
Object.defineProperty(window, 'calmaAPI', {
  value: {
    checkStatus: vi.fn().mockResolvedValue(true),
    startSequence: vi.fn().mockResolvedValue(undefined),
    stopAll: vi.fn().mockResolvedValue(undefined),
    runAudit: vi.fn().mockResolvedValue('iota:mock-block-id'),
    clearCache: vi.fn().mockResolvedValue(undefined),
    syncSpec: vi.fn().mockResolvedValue(undefined),
    getSettings: vi.fn().mockResolvedValue({}),
    saveSettings: vi.fn().mockResolvedValue(undefined)
  },
  writable: true
});

// Mock console.error to keep test output clean if expected
// const originalError = console.error;
// console.error = (...args) => {
//   if (/Warning.*not wrapped in act/.test(args[0])) return;
//   originalError(...args);
// };
