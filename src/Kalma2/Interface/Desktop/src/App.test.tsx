import { render, screen } from '@testing-library/react';
import App from './App';
import { container } from './core/di/container';

// Mock dependencies if needed, but here we can spy on the container
describe('App Component', () => {
  beforeEach(() => {
    // Clear mocks before each test
    vi.clearAllMocks();
  });

  it('renders without crashing and displays greeting', () => {
    // Mock the container.get method to return a dummy service
    const mockGreetingService = {
      getGreeting: vi.fn().mockReturnValue('Hello from Test Mock!'),
    };

    // We spy on the real container instance
    vi.spyOn(container, 'get').mockReturnValue(mockGreetingService);

    render(<App />);

    // Verify header is present
    expect(screen.getByText('Calma Desktop')).toBeInTheDocument();

    // Verify greeting from mock service
    expect(screen.getByText('Hello from Test Mock!')).toBeInTheDocument();
  });

  it('calls startSequence when product button is clicked', () => {
      const mockGreetingService = {
        getGreeting: vi.fn().mockReturnValue(''),
      };
      vi.spyOn(container, 'get').mockReturnValue(mockGreetingService);

      render(<App />);

      const startButtons = screen.getAllByText('Start Sequence');
      // Product domain is the first one based on layout
      startButtons[0].click();

      expect(window.calmaAPI.startSequence).toHaveBeenCalledWith(1);
  });
});
