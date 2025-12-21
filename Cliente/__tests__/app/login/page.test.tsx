import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LoginPage from '@/app/login/page'
import { useAuth } from '@/contexts/auth-context'

// Mock the auth context
jest.mock('@/contexts/auth-context')
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
  }),
}))

const mockLogin = jest.fn()
const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>

describe('LoginPage', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockUseAuth.mockReturnValue({
      user: null,
      isLoading: false,
      login: mockLogin,
      logout: jest.fn(),
      isAuthenticated: false,
    })
  })

  it('should render login form', () => {
    render(<LoginPage />)
    
    expect(screen.getByLabelText(/empresa/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/usuario/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/contraseña/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /iniciar sesión/i })).toBeInTheDocument()
  })

  it('should have default values in form', () => {
    render(<LoginPage />)
    
    const empresaInput = screen.getByLabelText(/empresa/i) as HTMLInputElement
    const usuarioInput = screen.getByLabelText(/usuario/i) as HTMLInputElement
    const contraseñaInput = screen.getByLabelText(/contraseña/i) as HTMLInputElement
    
    expect(empresaInput.value).toBe('Empresa Demo')
    expect(usuarioInput.value).toBe('admin')
    expect(contraseñaInput.value).toBe('admin123')
  })

  it('should handle form submission', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValue(undefined)
    
    render(<LoginPage />)
    
    const submitButton = screen.getByRole('button', { name: /iniciar sesión/i })
    await user.click(submitButton)
    
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        empresa: 'Empresa Demo',
        usuario: 'admin',
        contraseña: 'admin123',
      })
    })
  })

  it('should show error message on login failure', async () => {
    const user = userEvent.setup()
    const errorMessage = 'Credenciales inválidas'
    mockLogin.mockRejectedValue(new Error(errorMessage))
    
    render(<LoginPage />)
    
    const submitButton = screen.getByRole('button', { name: /iniciar sesión/i })
    await user.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument()
    })
  })

  it('should disable submit button while loading', async () => {
    const user = userEvent.setup()
    mockLogin.mockImplementation(() => new Promise(() => {})) // Never resolves
    
    render(<LoginPage />)
    
    const submitButton = screen.getByRole('button', { name: /iniciar sesión/i })
    await user.click(submitButton)
    
    await waitFor(() => {
      expect(submitButton).toBeDisabled()
    })
  })
})

