import '@testing-library/jest-dom';

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock window.calmaAPI (IPC Interface)
(window as any).calmaAPI = {
  send: vi.fn(),
  receive: vi.fn(),
  invoke: vi.fn(),
  onStatusChange: vi.fn().mockReturnValue(() => {}), // Returns unsubscribe fn
  startSequence: vi.fn(),
  stopAll: vi.fn(),
  runAudit: vi.fn(),
  clearCache: vi.fn(),
  syncSpec: vi.fn(),
};
