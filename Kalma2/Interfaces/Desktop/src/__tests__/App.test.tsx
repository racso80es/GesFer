import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
// App is in ../App relative to src/__tests__/
import App from '../App';
// Core is in ../../../../Core relative to src/__tests__/
// OR use absolute alias if we set one up? No, let's use relative for now.
import { container } from '../../../../Core/di/container';

// Mock the container module
vi.mock('../../../../Core/di/container', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../../../../Core/di/container')>();
  return {
    ...mod,
    container: {
      get: vi.fn(),
      bind: vi.fn().mockReturnThis(),
      to: vi.fn().mockReturnThis(),
      inSingletonScope: vi.fn(),
    },
    TYPES: mod.TYPES, // Keep original types
  };
});

// Mock GesFer config (Paths are relative to the test file too?)
// App imports '../../../Projects/GesFer/initial.json'
// From src/App.tsx: ../../../Projects -> Desktop/Interfaces/Kalma2/Projects -> ../../../Projects
// Wait, from src/App.tsx:
// ../ -> Desktop/
// ../../ -> Interfaces/
// ../../../ -> Kalma2/
// So Kalma2/Projects.

// From src/__tests__/App.test.tsx:
// ../../../../Projects -> Kalma2/Projects.
vi.mock('../../../../Projects/GesFer/initial.json', () => ({
  default: {
    id: 'TEST-PROJECT',
    name: 'Test Project',
    version: '1.0.0'
  }
}));

vi.mock('../../../../Projects/GesFer/services.json', () => ({
  default: [
    {
      name: 'Test Service',
      family: 'Test',
      verifyStatusUrl: 'http://localhost:3000/health',
      actions: []
    }
  ]
}));

describe('App Component', () => {
  beforeEach(() => {
    vi.resetAllMocks();

    // Setup container mock for GreetingService
    (container.get as any).mockReturnValue({
      getGreeting: () => 'Hello Test World'
    });
  });

  it('renders the title and greeting', () => {
    render(<App />);

    expect(screen.getByText('Calma Desktop')).toBeInTheDocument();
    expect(screen.getByText('Hello Test World')).toBeInTheDocument();
    expect(screen.getByText(/Test Project/)).toBeInTheDocument();
  });

  it('renders the service list', async () => {
    render(<App />);

    expect(screen.getByText('Test Domain')).toBeInTheDocument();
    expect(screen.getByText('Test Service')).toBeInTheDocument();
  });
});
