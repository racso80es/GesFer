# 🧪 Testing - GesFer Cliente

## Configuración de Tests

El proyecto utiliza **Jest** y **React Testing Library** para testing.

## 📦 Dependencias de Testing

- `jest` - Framework de testing
- `jest-environment-jsdom` - Entorno de testing para componentes React
- `@testing-library/react` - Utilidades para testing de componentes React
- `@testing-library/jest-dom` - Matchers adicionales para Jest
- `@testing-library/user-event` - Simulación de interacciones de usuario

## 🚀 Ejecutar Tests

### Ejecutar todos los tests
```bash
npm test
```

### Ejecutar tests en modo watch (desarrollo)
```bash
npm run test:watch
```

### Ejecutar tests con cobertura
```bash
npm run test:coverage
```

## 📁 Estructura de Tests

Los tests están organizados en la carpeta `__tests__`:

```
Cliente/
├── __tests__/
│   ├── app/              # Tests de páginas
│   ├── components/       # Tests de componentes
│   └── lib/              # Tests de utilidades y servicios
├── jest.config.js        # Configuración de Jest
└── jest.setup.js         # Configuración inicial de tests
```

## ✍️ Escribir Tests

### Ejemplo: Test de Componente

```typescript
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from '@/components/ui/button'

describe('Button Component', () => {
  it('should render button with text', () => {
    render(<Button>Click me</Button>)
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument()
  })

  it('should handle click events', async () => {
    const handleClick = jest.fn()
    const user = userEvent.setup()
    
    render(<Button onClick={handleClick}>Click me</Button>)
    await user.click(screen.getByRole('button'))
    
    expect(handleClick).toHaveBeenCalledTimes(1)
  })
})
```

### Ejemplo: Test de Utilidad

```typescript
import { cn } from '@/lib/utils/cn'

describe('cn utility', () => {
  it('should merge class names correctly', () => {
    const result = cn('text-red-500', 'bg-blue-500')
    expect(result).toContain('text-red-500')
    expect(result).toContain('bg-blue-500')
  })
})
```

### Ejemplo: Test de API

```typescript
import { apiClient } from '@/lib/api/client'

global.fetch = jest.fn()

describe('ApiClient', () => {
  it('should make GET request successfully', async () => {
    const mockData = { id: 1, name: 'Test' }
    ;(fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      status: 200,
      json: async () => mockData,
    })

    const result = await apiClient.get('/api/test')
    expect(result).toEqual(mockData)
  })
})
```

## 🎯 Buenas Prácticas

1. **Usa queries accesibles**: Prefiere `getByRole`, `getByLabelText`, etc.
2. **Testea comportamiento, no implementación**: Enfócate en lo que el usuario ve y hace
3. **Mockea dependencias externas**: API calls, localStorage, router, etc.
4. **Usa nombres descriptivos**: Los nombres de los tests deben describir qué están probando
5. **Organiza tests por funcionalidad**: Agrupa tests relacionados con `describe`

## 🔧 Configuración

### Jest Config (`jest.config.js`)

- Configurado para trabajar con Next.js
- Mapeo de rutas `@/*` configurado
- Entorno: `jsdom` para testing de componentes React
- Cobertura configurada para `app/`, `components/`, `lib/`, `contexts/`

### Setup (`jest.setup.js`)

- Configuración de `@testing-library/jest-dom`
- Mocks de Next.js router
- Mocks de `window.matchMedia`
- Mocks de `localStorage`

## 📊 Cobertura de Código

Para ver la cobertura de código:

```bash
npm run test:coverage
```

Esto generará un reporte en la carpeta `coverage/` con:
- Cobertura por archivo
- Líneas cubiertas/no cubiertas
- Funciones y branches cubiertos

## 🐛 Debugging Tests

### Ejecutar un test específico

```bash
npm test -- button.test.tsx
```

### Ejecutar tests que coincidan con un patrón

```bash
npm test -- --testNamePattern="should render"
```

### Ver output detallado

```bash
npm test -- --verbose
```

## 📝 Tests Incluidos

### Componentes UI
- ✅ `Button` - Renderizado, eventos, variantes, tamaños
- ✅ `Input` - Renderizado, entrada de usuario, estados

### Utilidades
- ✅ `cn` - Merge de clases, condiciones, override de clases

### API Client
- ✅ GET requests
- ✅ POST requests
- ✅ Manejo de errores
- ✅ Autenticación con tokens

### Páginas
- ✅ Login - Formulario, validación, manejo de errores

## 🚧 Próximos Tests a Implementar

- [ ] Tests para componentes `Card`, `Label`, `Loading`, `ErrorMessage`
- [ ] Tests para contexto de autenticación
- [ ] Tests para páginas `Dashboard`, `Usuarios`, `Clientes`
- [ ] Tests de integración para flujos completos
- [ ] Tests E2E con Playwright o Cypress

## 📚 Recursos

- [Jest Documentation](https://jestjs.io/docs/getting-started)
- [React Testing Library](https://testing-library.com/react)
- [Testing Library User Event](https://testing-library.com/docs/user-event/intro)
- [Next.js Testing](https://nextjs.org/docs/testing)

