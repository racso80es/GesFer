import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import MyCompanyPage from './page';
import { toast } from 'sonner';

// Mock sonner
jest.mock('sonner', () => ({
  toast: {
    success: jest.fn(),
    error: jest.fn(),
  },
}));

// Mock next-intl
jest.mock('next-intl', () => ({
  useTranslations: () => (key: string) => key,
}));

// Mock the CompanyForm component since we are testing the page logic, not the form itself
jest.mock('../../components/companies/company-form', () => ({
  CompanyForm: ({ onSubmit, company, isLoading }: any) => (
    <div data-testid="company-form">
      {isLoading ? 'Loading Form' : 'Form Loaded'}
      <button
        data-testid="submit-button"
        onClick={() => onSubmit({ name: 'Updated Company' })}
      >
        Submit
      </button>
    </div>
  ),
}));

// Mock global fetch
global.fetch = jest.fn();

describe('MyCompanyPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders loading state initially', async () => {
    (global.fetch as jest.Mock).mockImplementationOnce(() =>
      new Promise(() => {}) // Never resolves
    );

    render(<MyCompanyPage />);
    expect(screen.getByText('loading')).toBeInTheDocument();
  });

  it('renders company form when data is loaded', async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: '1', name: 'Test Company' }),
    });

    render(<MyCompanyPage />);

    await waitFor(() => {
      expect(screen.getByText('title')).toBeInTheDocument();
      expect(screen.getByTestId('company-form')).toBeInTheDocument();
    });
  });

  it('shows success toast on successful update', async () => {
    // Initial load
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: '1', name: 'Test Company' }),
    });

    render(<MyCompanyPage />);

    await waitFor(() => {
      expect(screen.getByTestId('company-form')).toBeInTheDocument();
    });

    // Update request
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: '1', name: 'Updated Company' }),
    });

    fireEvent.click(screen.getByTestId('submit-button'));

    await waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith('updatedSuccessfully');
    });
  });

  it('shows error toast on update failure', async () => {
    // Initial load
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: '1', name: 'Test Company' }),
    });

    render(<MyCompanyPage />);

    await waitFor(() => {
      expect(screen.getByTestId('company-form')).toBeInTheDocument();
    });

    // Update request failure
    (global.fetch as jest.Mock).mockRejectedValueOnce(new Error('Update failed'));

    // Silence console.error
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    fireEvent.click(screen.getByTestId('submit-button'));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith('updateError');
    });

    consoleSpy.mockRestore();
  });
});
