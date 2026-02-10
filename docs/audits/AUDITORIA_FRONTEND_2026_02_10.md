# AUDITORÍA FRONTEND — 2026-02-10

**Auditor:** FRONT-ARCHITECT (Senior Frontend Quality Assurance & Accessibility Auditor)
**Fecha:** 2026-02-10 (UTC)
**Estado:** 🔴 FALLA CRÍTICA

---

## 1. Resumen Ejecutivo

La auditoría diaria ha finalizado.
Estado Global: **🔴 FALLA CRÍTICA**

Se han detectado **FALLAS CRÍTICAS** relacionadas con terminología prohibida ('empresa').

## 2. Métricas Clave

| Métrica | Valor | Estado |
| :--- | :--- | :--- |
| **Violaciones de Terminología ("empresa")** | **178** | 🔴 CRÍTICO |
| **Integridad de Dependencias (Lockfiles)** | **True** | 🟢 PASA |
| **Deuda Técnica (`any`)** | **7** | 🟢 PASA |
| **Deuda Técnica (`@ts-ignore`)** | **0** | 🟢 PASA |
| **Accesibilidad (Imágenes sin Alt)** | **0** | 🟢 PASA |

## 3. Hallazgos Detallados

### 3.1 Terminología Prohibida ("empresa")
Se han encontrado las siguientes violaciones:

- **src/Product/Front/I18N-GUIDE.md**
  - Line 18: `empresas/...`
  - Line 19: `page.tsx          # Lista de empresas...`
  - Line 21: `page.tsx        # Detalle de empresa...`
  - ... y 1 más.
- **src/Product/Front/COMANDOS-GIT.md**
  - Line 13: `- Tests de integridad (integrity.test.tsx): 26 tests que cubren autenticación, CRUD de usuarios y em...`
- **src/Product/Front/I18N-STATUS.md**
  - Line 25: `- ✅ Páginas de login, dashboard, empresas, usuarios, clientes - Traducidas...`
  - Line 37: `2. **Redirige** al idioma correcto según la configuración del usuario/empresa/país...`
- **src/Product/Front/README.md**
  - Line 98: `- **Empresa**: Empresa Demo...`
- **src/Product/Front/INSTRUCCIONES.md**
  - Line 82: `- **Organización**: `Empresa Demo`...`
- **src/Product/Front/auth.ts**
  - Line 34: `empresa: credentials.company,...`
- **src/Product/Front/SETUP.md**
  - Line 99: `- **Organización**: Empresa Demo...`
- **src/Product/Front/components/usuarios/user-form.tsx**
  - Line 30: `// La empresa siempre es la del usuario logueado...`
- **src/Product/Front/__tests__/integration/e2e-flows.test.tsx**
  - Line 145: `describe("Flujo E2E: Gestión Completa de Empresas", () => {...`
  - Line 158: `it("debe completar flujo completo de operaciones CRUD de empresas", async () => {...`
  - Line 159: `// 1. Listar empresas...`
  - ... y 5 más.
- **src/Product/Front/__tests__/integration/integrity.test.tsx**
  - Line 62: `empresa: "Test Company",...`
  - Line 69: `empresa: "Test Company",...`
  - Line 80: `empresa: "Test Company",...`
  - ... y 13 más.
- **src/Product/Front/__tests__/integration/language-id-integrity.test.ts**
  - Line 14: `* - Credenciales de prueba: empresa "Empresa Demo", usuario "admin", contraseña "admin123"...`
- **src/Product/Front/__tests__/integration/id-validation.test.ts**
  - Line 69: `empresa: "Empresa Demo",...`
  - Line 104: `// Obtener una empresa válida (solo si tenemos token)...`
  - Line 121: `console.warn("No se pudo obtener empresas. Algunos tests pueden fallar.");...`
  - ... y 3 más.
- **src/Product/Front/__tests__/integration/users-companies-integrity.test.ts**
  - Line 2: `* Tests de integridad E2E para Usuarios y Empresas...`
  - Line 5: `* de usuarios y empresas contra la API real....`
  - Line 9: `* - Credenciales de prueba: empresa "Empresa Demo", usuario "admin", contraseña "admin123"...`
  - ... y 17 más.
- **src/Product/Front/__tests__/integration/api-contracts.test.ts**
  - Line 113: `describe("Contrato: Empresas API", () => {...`
  - Line 181: `empresa: "Test Company",...`
- **src/Product/Front/__tests__/integration/system-integrity.test.ts**
  - Line 99: `empresa: "Empresa Demo",...`
  - Line 121: `expect(loginJson.companyName).toBe("Empresa Demo");...`
  - Line 144: `empresa: "Empresa Demo",...`
  - ... y 3 más.
- **src/Product/Front/__tests__/lib/utils/id-validation.test.ts**
  - Line 64: `expect(validateId(validId, "empresa")).toBe(validId);...`
  - Line 69: `expect(() => validateId("11.1111-111111111111:1", "empresa")).toThrow(...`
  - Line 70: `"El ID de empresa no es válido"...`
  - ... y 2 más.
- **src/Product/Front/__tests__/lib/api/id-validation-api.test.ts**
  - Line 28: `"El ID de empresa no es válido"...`
  - Line 36: `).rejects.toThrow("El ID de empresa no es válido");...`
  - Line 41: `await expect(companiesApi.delete("")).rejects.toThrow("El ID de empresa es requerido");...`
  - ... y 1 más.
- **src/Product/Front/__tests__/app/login/page.test.tsx**
  - Line 17: `'auth.company': 'Empresa',...`
  - Line 45: `expect(screen.getByLabelText(/empresa|company/i)).toBeInTheDocument()...`
  - Line 54: `const empresaInput = screen.getByLabelText(/empresa|company/i) as HTMLInputElement...`
  - ... y 2 más.
- **src/Product/Front/__tests__/app/usuarios/page.test.tsx**
  - Line 25: `'navigation.companies': 'Empresas',...`
  - Line 46: `'users.table.company': 'Empresa',...`
- **src/Product/Front/messages/ca.json**
  - Line 27: `"company": "Empresa",...`
  - Line 58: `"company": "Empresa",...`
  - Line 69: `"company": "Empresa",...`
  - ... y 16 más.
- **src/Product/Front/messages/es.json**
  - Line 27: `"company": "Empresa",...`
- **src/Product/Front/tests/README-BEST-PRACTICES.md**
  - Line 43: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 108: `data-testid="login-empresa-input"...`
  - Line 109: `id="empresa"...`
  - ... y 1 más.
- **src/Product/Front/tests/README.md**
  - Line 89: `- Empresa: "Empresa Demo"...`
- **src/Product/Front/tests/TEST-IDS.md**
  - Line 10: `| Input Empresa | `login-empresa-input` | Campo de entrada para empresa |...`
  - Line 22: `| Link Empresas | `dashboard-empresas-link` | Link de navegación a empresas |...`
- **src/Product/Front/tests/page-objects/CompaniesPage.ts**
  - Line 21: `this.title = page.getByRole('heading', { name: /empresas|companies/i }).first();...`
  - Line 22: `this.newCompanyButton = page.getByRole('button', { name: /nova empresa|new company|crear|create/i })...`
- **src/Product/Front/tests/page-objects/DashboardPage.ts**
  - Line 21: `this.companiesLink = page.getByTestId('dashboard-companies-link').or(page.getByRole('link', { name: ...`
  - Line 43: `* Navega a la sección de empresas...`
- **src/Product/Front/tests/page-objects/LoginPage.ts**
  - Line 21: `this.organizationInput = page.getByTestId('login-company-input').or(page.getByTestId('shared-input-t...`
- **src/Product/Front/tests/e2e/login.spec.ts**
  - Line 21: `// Nota: "Empresa Demo" es el dato de seed actual, se mantiene como literal....`
  - Line 23: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 38: `await loginPage.login('Empresa Demo', 'admin', 'admin123');...`
  - ... y 1 más.
- **src/Product/Front/tests/e2e/companies.spec.ts**
  - Line 7: `test.describe('Empresas E2E Tests', () => {...`
  - Line 17: `await loginPage.login('Empresa Demo', 'admin', 'admin123');...`
  - Line 24: `test('debe crear una nueva empresa correctamente', async ({ page }) => {...`
  - ... y 3 más.
- **src/Product/Front/tests/e2e/usuarios.spec.ts**
  - Line 13: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 18: `await loginPage.login('Empresa Demo', 'admin', 'admin123');...`
- **src/Product/Front/tests/e2e/logging-persistence.spec.ts**
  - Line 56: `await page.waitForSelector('input[type="text"], input[name="empresa"]', { timeout: 10000 });...`
  - Line 59: `const empresaInput = page.locator('input[name="empresa"]').or(page.locator('input[type="text"]').fir...`
  - Line 64: `await empresaInput.fill('Empresa Demo');...`
- **src/Product/Front/tests/e2e/login-prod.spec.ts**
  - Line 15: `* - Base de datos con datos de prueba (Empresa Demo / admin / admin123)...`
  - Line 45: `organization: 'Empresa Demo',...`
- **src/Product/Front/tests/e2e/usuario-completo.spec.ts**
  - Line 12: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 25: `await loginPage.login('Empresa Demo', 'admin', 'admin123');...`
- **src/Product/Front/tests/helpers/test-data-cleanup.ts**
  - Line 22: `async setAuthToken(empresa: string, usuario: string, contraseña: string): Promise<void> {...`
  - Line 24: `this.authToken = await this.apiClient.login(empresa, usuario, contraseña);...`
  - Line 38: `* Registra un ID de empresa creado para limpieza posterior...`
  - ... y 5 más.
- **src/Product/Front/tests/fixtures/auth.fixture.ts**
  - Line 20: `const token = await apiClient.login('Empresa Demo', 'admin', 'admin123');...`
  - Line 24: `await loginPage.login('Empresa Demo', 'admin', 'admin123');...`
- **src/Product/Front/tests/api/auth-api.spec.ts**
  - Line 13: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 22: `const loginData = await apiClient.loginFull('Empresa Demo', 'admin', 'admin123');...`
  - Line 35: `empresa: 'Empresa Demo',...`
  - ... y 2 más.
- **src/Product/Front/tests/api/api-client.ts**
  - Line 73: `async login(empresa: string, usuario: string, contraseña: string): Promise<string> {...`
  - Line 75: `empresa,...`
  - Line 93: `async loginFull(empresa: string, usuario: string, contraseña: string): Promise<LoginResponse> {...`
  - ... y 2 más.
- **src/Product/Front/tests/api/usuarios-api.spec.ts**
  - Line 17: `authToken = await apiClient.login('Empresa Demo', 'admin', 'admin123');...`
  - Line 18: `await cleanup.setAuthToken('Empresa Demo', 'admin', 'admin123');...`
  - Line 62: `// Primero obtener el companyId de la empresa "Empresa Demo" desde el login...`
  - ... y 1 más.
- **src/Product/Front/lib/validations/user.ts**
  - Line 9: `companyId: z.string().uuid("El ID de empresa debe ser un UUID válido"),...`
- **src/Product/Front/lib/types/api.ts**
  - Line 10: `empresa: string;...`
- **src/Product/Front/lib/api/users.ts**
  - Line 7: `const params = companyId ? { companyId: validateId(companyId, "empresa") } : undefined;...`
- **src/Product/Front/lib/api/auth.ts**
  - Line 8: `empresa: credentials.company,...`
- **src/Product/Front/app/[locale]/login/page.tsx**
  - Line 16: `company: "Empresa Cliente",...`
- **src/Product/Front/app/[locale]/companies/page.tsx**
  - Line 114: `console.error("Error al eliminar empresa:", error);...`
- **src/Product/Front/app/[locale]/companies/[id]/page.tsx**
  - Line 73: `: "Empresa no encontrada"...`
  - Line 114: `Información de la Empresa...`
- **src/Product/Front/app/(client)/login/page.tsx**
  - Line 15: `// GUID de Empresa Cliente: 33333333-3333-3333-3333-333333333333...`
  - Line 17: `company: "Empresa Cliente",...`
- **src/Product/Front/app/(client)/companies/[id]/page.tsx**
  - Line 49: `<Loading size="lg" text="Cargando empresa..." />...`
  - Line 65: `data-testid="shared-button-empresas-back"...`
  - Line 74: `: "Empresa no encontrada"...`
  - ... y 3 más.

### 3.2 Integridad de Dependencias
- `src/Product/Front/package-lock.json`: PRESENTE
- `src/Admin/Front/package-lock.json`: PRESENTE
- `src/Shared/Front`: N/A (Librería compartida)

### 3.3 Calidad de Código
- Se detectaron **7** usos de `any`.
- Se detectaron **0** usos de `@ts-ignore`.

## 4. Conclusión

El estado actual es **CRÍTICO**. Se requiere intervención inmediata.