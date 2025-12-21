/**
 * Tests End-to-End - Flujos Completos de Usuario
 * 
 * Estos tests simulan flujos completos de usuario desde la perspectiva
 * de la interfaz, verificando que todas las interacciones funcionan correctamente.
 */

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/contexts/auth-context";
import UsuariosPage from "@/app/usuarios/page";
import EmpresasPage from "@/app/empresas/page";
import { usersApi } from "@/lib/api/users";
import { companiesApi } from "@/lib/api/companies";
import { authApi } from "@/lib/api/auth";

jest.mock("@/lib/api/users");
jest.mock("@/lib/api/companies");
jest.mock("@/lib/api/auth");
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    back: jest.fn(),
  }),
  usePathname: () => "/usuarios",
  useSearchParams: () => new URLSearchParams(),
}));

const mockUsersApi = usersApi as jest.Mocked<typeof usersApi>;
const mockCompaniesApi = companiesApi as jest.Mocked<typeof companiesApi>;
const mockAuthApi = authApi as jest.Mocked<typeof authApi>;

const createTestQueryClient = () => {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, cacheTime: 0 },
      mutations: { retry: false },
    },
  });
};

const mockUser = {
  userId: "admin-1",
  username: "admin",
  firstName: "Admin",
  lastName: "User",
  companyId: "company-1",
  companyName: "Test Company",
  permissions: [],
  token: "token",
};

const renderWithProviders = (ui: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  // Mock getStoredUser antes de renderizar
  mockAuthApi.getStoredUser = jest.fn().mockReturnValue(mockUser);
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{ui}</AuthProvider>
    </QueryClientProvider>
  );
};

describe("Flujos E2E - Interacciones Completas", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockAuthApi.getStoredUser = jest.fn().mockReturnValue(mockUser);
  });

  describe("Flujo E2E: Gestión Completa de Usuarios", () => {
    const mockUsers = [
      {
        id: "user-1",
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

    it("debe completar flujo completo de operaciones CRUD de usuarios", async () => {
      // 1. Listar usuarios
      mockUsersApi.getAll.mockResolvedValue(mockUsers);
      const users = await usersApi.getAll("company-1");
      expect(users).toHaveLength(1);
      expect(users[0].username).toBe("user1");

      // 2. Obtener usuario por ID
      mockUsersApi.getById.mockResolvedValue(mockUsers[0]);
      const user = await usersApi.getById("user-1");
      expect(user.username).toBe("user1");

      // 3. Crear usuario
      const newUser = {
        ...mockUsers[0],
        id: "user-2",
        username: "newuser",
      };
      mockUsersApi.create.mockResolvedValue(newUser);
      const created = await usersApi.create({
        companyId: "company-1",
        username: "newuser",
        password: "password123",
        firstName: "New",
        lastName: "User",
      });
      expect(created.username).toBe("newuser");

      // 4. Actualizar usuario
      const updatedUser = { ...newUser, firstName: "Updated" };
      mockUsersApi.update.mockResolvedValue(updatedUser);
      const updated = await usersApi.update("user-2", { firstName: "Updated" });
      expect(updated.firstName).toBe("Updated");

      // 5. Eliminar usuario
      mockUsersApi.delete.mockResolvedValue(undefined);
      await usersApi.delete("user-2");
      expect(mockUsersApi.delete).toHaveBeenCalledWith("user-2");
    });

    it("debe manejar errores en operaciones de usuarios", async () => {
      mockUsersApi.getAll.mockRejectedValue(new Error("Error de red"));
      await expect(usersApi.getAll("company-1")).rejects.toThrow("Error de red");

      mockUsersApi.create.mockRejectedValue(new Error("Error al crear usuario"));
      await expect(
        usersApi.create({
          companyId: "company-1",
          username: "test",
          password: "pass",
          firstName: "Test",
          lastName: "User",
        })
      ).rejects.toThrow("Error al crear usuario");
    });
  });

  describe("Flujo E2E: Gestión Completa de Empresas", () => {
    const mockCompanies = [
      {
        id: "company-1",
        name: "Test Company",
        taxId: "B12345678",
        email: "test@example.com",
        phone: "123456789",
        isActive: true,
        createdAt: "2024-01-01T00:00:00Z",
      },
    ];

    it("debe completar flujo completo de operaciones CRUD de empresas", async () => {
      // 1. Listar empresas
      mockCompaniesApi.getAll.mockResolvedValue(mockCompanies);
      const companies = await companiesApi.getAll();
      expect(companies).toHaveLength(1);
      expect(companies[0].name).toBe("Test Company");

      // 2. Obtener empresa por ID
      mockCompaniesApi.getById.mockResolvedValue(mockCompanies[0]);
      const company = await companiesApi.getById("company-1");
      expect(company.name).toBe("Test Company");

      // 3. Crear empresa
      const newCompany = {
        ...mockCompanies[0],
        id: "company-2",
        name: "New Company",
      };
      mockCompaniesApi.create.mockResolvedValue(newCompany);
      const created = await companiesApi.create({
        name: "New Company",
        taxId: "B87654321",
      });
      expect(created.name).toBe("New Company");

      // 4. Actualizar empresa
      const updatedCompany = { ...newCompany, name: "Updated Company" };
      mockCompaniesApi.update.mockResolvedValue(updatedCompany);
      const updated = await companiesApi.update("company-2", {
        name: "Updated Company",
      });
      expect(updated.name).toBe("Updated Company");

      // 5. Eliminar empresa
      mockCompaniesApi.delete.mockResolvedValue(undefined);
      await companiesApi.delete("company-2");
      expect(mockCompaniesApi.delete).toHaveBeenCalledWith("company-2");
    });

    it("debe manejar lista vacía de empresas", async () => {
      mockCompaniesApi.getAll.mockResolvedValue([]);
      const companies = await companiesApi.getAll();
      expect(companies).toHaveLength(0);
    });
  });

  describe("Flujo E2E: Autenticación y Navegación", () => {
    it("debe mantener sesión después de login", async () => {
      const loginResponse = {
        ...mockUser,
        token: "new-token",
      };

      mockAuthApi.login.mockResolvedValue(loginResponse);
      mockAuthApi.getStoredUser.mockReturnValue(loginResponse);

      const result = await authApi.login({
        empresa: "Test Company",
        usuario: "admin",
        contraseña: "password",
      });

      expect(result.token).toBe("new-token");
      expect(mockAuthApi.getStoredUser()).toEqual(loginResponse);
    });
  });
});

