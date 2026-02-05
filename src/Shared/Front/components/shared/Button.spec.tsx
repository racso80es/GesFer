import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { Button } from './Button';

describe('Shared Button', () => {
  it('renders correctly with default props', () => {
    render(<Button>Click me</Button>);
    const button = screen.getByRole('button', { name: /click me/i });
    expect(button).toBeInTheDocument();
    // Default variant class from button.tsx
    expect(button).toHaveClass('bg-primary');
  });

  it('renders correctly with variant destructive', () => {
    render(<Button variant="destructive">Delete</Button>);
    const button = screen.getByRole('button', { name: /delete/i });
    expect(button).toHaveClass('bg-destructive');
  });

  it('handles click events', () => {
    const handleClick = jest.fn();
    render(<Button onClick={handleClick}>Click me</Button>);
    const button = screen.getByRole('button', { name: /click me/i });
    fireEvent.click(button);
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('generates correct data-testid', () => {
    render(<Button variant="outline">Test</Button>);
    const button = screen.getByRole('button', { name: /test/i });
    expect(button).toHaveAttribute('data-testid', 'shared-button-outline');
  });

  it('respects provided data-testid', () => {
    render(<Button data-testid="my-custom-id">Test</Button>);
    const button = screen.getByRole('button', { name: /test/i });
    expect(button).toHaveAttribute('data-testid', 'my-custom-id');
  });
});
