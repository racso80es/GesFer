import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import App from './App';

// Mock the container module
vi.mock('./core/di/container', () => {
  return {
    container: {
      get: () => ({
        getGreeting: () => 'Hello from Test',
      }),
    },
    TYPES: {
      GreetingService: Symbol.for('GreetingService'),
    },
  };
});

describe('App', () => {
  beforeEach(() => {
    // Mock window.calmaAPI
    window.calmaAPI = {
      startSequence: vi.fn(),
      stopAll: vi.fn(),
      getSettings: vi.fn(),
      updateSettings: vi.fn(),
      runAudit: vi.fn(),
      clearCache: vi.fn(),
      syncSpec: vi.fn(),
      onStatusChange: vi.fn(() => () => {}),
    };
  });

  it('renders the header and greeting', () => {
    render(<App />);
    expect(screen.getByText('Calma Desktop')).toBeInTheDocument();
    // The greeting "Hello from Test" should be rendered
    expect(screen.getByText('Hello from Test')).toBeInTheDocument();
  });

  it('renders domain sections', () => {
    render(<App />);
    expect(screen.getByText('Product Domain')).toBeInTheDocument();
    expect(screen.getByText('Admin Domain')).toBeInTheDocument();
  });
});
