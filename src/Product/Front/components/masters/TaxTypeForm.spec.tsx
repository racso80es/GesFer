import React from 'react';
import { render, screen } from '@testing-library/react';
import { TaxTypeForm } from './TaxTypeForm';

// Mock translations
jest.mock('next-intl', () => ({
  useTranslations: () => (key: string) => key,
}));

// Mock ResizeObserver for Dialog
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
};

describe('TaxTypeForm', () => {
  const mockOnSubmit = jest.fn();
  const mockOnOpenChange = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders correctly when open', () => {
    render(
      <TaxTypeForm
        open={true}
        onOpenChange={mockOnOpenChange}
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByText('createTitle')).toBeInTheDocument();
    expect(screen.getByLabelText('code')).toBeInTheDocument();
    expect(screen.getByLabelText('name')).toBeInTheDocument();
    expect(screen.getByLabelText('value')).toBeInTheDocument();
  });
});
