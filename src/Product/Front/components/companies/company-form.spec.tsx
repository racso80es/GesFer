import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CompanyForm } from './company-form';

// Mock translations
jest.mock('next-intl', () => ({
  useTranslations: () => (key: string) => key,
}));

describe('CompanyForm', () => {
  const mockOnSubmit = jest.fn();
  const mockOnCancel = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders correctly', () => {
    render(
      <CompanyForm
        onSubmit={mockOnSubmit}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByLabelText(/name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/taxId/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/address/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'create' })).toBeInTheDocument();
  });

  it('submits valid data', async () => {
    const user = userEvent.setup();
    render(
      <CompanyForm
        onSubmit={mockOnSubmit}
        onCancel={mockOnCancel}
      />
    );

    const nameInput = screen.getByLabelText(/name/i);
    const addressInput = screen.getByLabelText(/address/i);
    const emailInput = screen.getByLabelText(/email/i);

    await user.type(nameInput, 'Test Company');
    await user.type(addressInput, '123 Test St');
    await user.type(emailInput, 'test@example.com');

    const submitBtn = screen.getByRole('button', { name: 'create' });
    await user.click(submitBtn);

    await waitFor(() => {
        expect(mockOnSubmit).toHaveBeenCalledTimes(1);
    });

    expect(mockOnSubmit).toHaveBeenCalledWith(expect.objectContaining({
        name: 'Test Company',
        address: '123 Test St',
        email: 'test@example.com'
    }));
  });

  it('handles edit mode correctly', () => {
     const initialData: any = {
         id: '1',
         name: 'Existing Company',
         address: 'Existing Address',
         isActive: true
     };

     render(
      <CompanyForm
        company={initialData}
        onSubmit={mockOnSubmit}
        onCancel={mockOnCancel}
      />
    );

    expect(screen.getByDisplayValue('Existing Company')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'update' })).toBeInTheDocument();
    expect(screen.getByLabelText(/isActive/i)).toBeInTheDocument();
  });
});
