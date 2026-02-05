import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { Input } from './Input';

describe('Shared Input', () => {
  it('renders correctly', () => {
    render(<Input placeholder="Enter text" />);
    const input = screen.getByPlaceholderText(/enter text/i);
    expect(input).toBeInTheDocument();
  });

  it('handles change events', () => {
    const handleChange = jest.fn();
    render(<Input onChange={handleChange} />);
    const input = screen.getByRole('textbox');
    fireEvent.change(input, { target: { value: 'test' } });
    expect(handleChange).toHaveBeenCalledTimes(1);
  });

  it('generates correct data-testid based on type', () => {
    // Input with type email usually has role textbox unless specific support is enabled in jsdom
    render(<Input type="email" name="user-email" />);
    const input = screen.getByRole('textbox');
    expect(input).toHaveAttribute('data-testid', 'shared-input-email-user-email');
  });

  it('respects provided data-testid', () => {
    render(<Input data-testid="custom-input" />);
    const input = screen.getByRole('textbox');
    expect(input).toHaveAttribute('data-testid', 'custom-input');
  });
});
