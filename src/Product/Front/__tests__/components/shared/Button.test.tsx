import { render, screen } from '@testing-library/react';
import { Button } from '@shared/components/shared/Button';

describe('Shared Button Component', () => {
  it('should render button with text', () => {
    render(<Button>Click me</Button>);
    
    const button = screen.getByTestId('shared-button-default');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('Click me');
  });

  it('should render button with custom data-testid', () => {
    render(<Button data-testid="shared-button-custom">Custom Button</Button>);
    
    const button = screen.getByTestId('shared-button-custom');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('Custom Button');
  });

  it('should render button with different variants', () => {
    const { rerender } = render(<Button variant="destructive">Delete</Button>);
    
    let button = screen.getByTestId('shared-button-destructive');
    expect(button).toBeInTheDocument();

    rerender(<Button variant="outline">Outline</Button>);
    button = screen.getByTestId('shared-button-outline');
    expect(button).toBeInTheDocument();
  });

  it('should be disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled Button</Button>);
    
    const button = screen.getByTestId('shared-button-default');
    expect(button).toBeDisabled();
  });

  it('should apply custom className', () => {
    render(<Button className="custom-class">Custom Class</Button>);
    
    const button = screen.getByTestId('shared-button-default');
    expect(button).toHaveClass('custom-class');
  });
});
