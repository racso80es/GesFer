import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import App from './App'

// The mocks for window.calmaAPI are in setupTests.ts

describe('App', () => {
  it('renders the dashboard', () => {
    render(<App />)
    expect(screen.getByText('Calma Desktop')).toBeInTheDocument()
    expect(screen.getByText('Product Domain')).toBeInTheDocument()
    expect(screen.getByText('Admin Domain')).toBeInTheDocument()
  })
})
