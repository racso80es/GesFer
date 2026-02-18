import '@testing-library/jest-dom'
import 'reflect-metadata'
import { vi } from 'vitest'

// Mock window.calmaAPI based on preload.ts
// We use Object.defineProperty to make it writable for tests if needed
Object.defineProperty(window, 'calmaAPI', {
  writable: true,
  value: {
    startSequence: vi.fn(),
    stopAll: vi.fn(),
    getSettings: vi.fn(),
    updateSettings: vi.fn(),
    runAudit: vi.fn(),
    clearCache: vi.fn(),
    syncSpec: vi.fn(),
    onStatusChange: vi.fn(() => vi.fn()), // Returns cleanup function
  },
})

// Mock common missing JSDOM APIs
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
})

Object.defineProperty(window, 'resizeTo', {
  value: (width: number, height: number) => {
    Object.assign(window, { innerWidth: width, innerHeight: height, outerWidth: width, outerHeight: height }).dispatchEvent(new Event('resize'))
  }
})
