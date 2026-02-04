import { render, screen } from '@testing-library/react';
import { Button } from '@shared/components/shared/Button';

describe('Shared Button Component (Admin)', () => {
  it('should render button with text', () => {
    render(<Button>Admin Button</Button>);
    
    const button = screen.getByTestId('shared-button-default');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('Admin Button');
  });

  it('should render button with custom data-testid', () => {
    render(<Button data-testid="shared-button-admin-action">Admin Action</Button>);
    
    const button = screen.getByTestId('shared-button-admin-action');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('Admin Action');
  });

  it('should be disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled Admin Button</Button>);
    
    const button = screen.getByTestId('shared-button-default');
    expect(button).toBeDisabled();
  });
});
