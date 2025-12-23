import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import UsuariosPage from "@/app/[locale]/usuarios/page";
import { useAuth } from "@/contexts/auth-context";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";

jest.mock("@/contexts/auth-context");
jest.mock("@tanstack/react-query");
jest.mock("@/lib/api/users");
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    back: jest.fn(),
  }),
  usePathname: () => "/es/usuarios",
}));
jest.mock('next-intl', () => ({
  useTranslations: () => (key: string) => {
    const translations: Record<string, string> = {
      'navigation.dashboard': 'Panel de control',
      'navigation.users': 'Usuarios',
      'navigation.companies': 'Empresas',
      'navigation.customers': 'Clientes',
    };
    return translations[key] || key;
  },
  useLocale: () => 'es',
}))

const mockUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;
const mockUseQuery = useQuery as jest.MockedFunction<typeof useQuery>;
const mockUseMutation = useMutation as jest.MockedFunction<typeof useMutation>;
const mockUseQueryClient = useQueryClient as jest.MockedFunction<typeof useQueryClient>;

describe("UsuariosPage", () => {
  const mockUser = {
    userId: "1",
    username: "test",
    firstName: "Test",
    lastName: "User",
    companyId: "company-1",
    companyName: "Test Company",
    permissions: [],
    token: "token",
  };

  const mockUsers = [
    {
      id: "1",
      companyId: "company-1",
      companyName: "Test Company",
      username: "user1",
      firstName: "John",
      lastName: "Doe",
      email: "john@example.com",
      phone: "123456789",
      isActive: true,
      createdAt: "2024-01-01T00:00:00Z",
    },
  ];

  beforeEach(() => {
    jest.clearAllMocks();
    mockUseAuth.mockReturnValue({
      user: mockUser,
      isLoading: false,
      login: jest.fn(),
      logout: jest.fn(),
      isAuthenticated: true,
    });

    mockUseQuery.mockReturnValue({
      data: mockUsers,
      isLoading: false,
      error: null,
      refetch: jest.fn(),
    } as any);

    mockUseMutation.mockReturnValue({
      mutateAsync: jest.fn().mockResolvedValue({}),
      isPending: false,
    } as any);

    mockUseQueryClient.mockReturnValue({
      invalidateQueries: jest.fn(),
    } as any);
  });

  it("should render usuarios list", () => {
    render(<UsuariosPage />);
    // Buscar el título h1 específico
    const title = screen.getByRole("heading", { name: "Usuarios", level: 1 });
    expect(title).toBeInTheDocument();
    expect(screen.getByText("user1")).toBeInTheDocument();
  });

  it("should open create modal when clicking new user button", async () => {
    const user = userEvent.setup();
    render(<UsuariosPage />);

    const newUserButton = screen.getByText("Nuevo Usuario");
    await user.click(newUserButton);

    await waitFor(() => {
      expect(screen.getByText("Crear Nuevo Usuario")).toBeInTheDocument();
    });
  });
});

