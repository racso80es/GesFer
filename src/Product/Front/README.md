# GesFer - Cliente Frontend

Aplicación frontend desarrollada con Next.js 14+, TypeScript y Tailwind CSS para el sistema de gestión de compra/venta de chatarra.

## 🚀 Tecnologías

- **Next.js 14+** - Framework React con App Router
- **TypeScript** - Tipado estático
- **Tailwind CSS** - Estilos utilitarios
- **TanStack Query** - Gestión de estado del servidor
- **Lucide React** - Iconos
- **Shadcn/UI** - Componentes UI (estilo)

## 📋 Requisitos Previos

- Node.js 18+ 
- npm o yarn
- API backend ejecutándose (por defecto en `http://localhost:5000`)

## 🛠️ Instalación

1. Instala las dependencias:

```bash
npm install
# o
yarn install
```

2. Configura las variables de entorno:

Copia el archivo `.env.example` a `.env.local`:

```bash
cp .env.example .env.local
```

Edita `.env.local` y configura la URL de tu API:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

## 🏃 Ejecución

### Desarrollo

```bash
npm run dev
# o
yarn dev
```

La aplicación estará disponible en [http://localhost:3000](http://localhost:3000)

### Producción

```bash
npm run build
npm start
# o
yarn build
yarn start
```

## 📁 Estructura del Proyecto

```
Cliente/
├── app/                    # App Router de Next.js
│   ├── dashboard/         # Página del dashboard
│   ├── usuarios/          # Página de usuarios
│   ├── clientes/          # Página de clientes
│   ├── login/             # Página de login
│   ├── layout.tsx         # Layout raíz
│   ├── page.tsx           # Página principal (redirección)
│   └── globals.css        # Estilos globales
├── components/            # Componentes reutilizables
│   ├── ui/                # Componentes base (Button, Input, Card, etc.)
│   ├── auth/              # Componentes de autenticación
│   └── layout/            # Componentes de layout
├── contexts/              # Contextos de React
│   └── auth-context.tsx   # Contexto de autenticación
├── lib/                   # Utilidades y configuraciones
│   ├── api/               # Cliente API y funciones de API
│   ├── providers/         # Providers (QueryProvider, etc.)
│   ├── types/             # Tipos TypeScript
│   └── utils/             # Utilidades generales
└── public/                # Archivos estáticos
```

## 🔐 Autenticación

La aplicación utiliza autenticación basada en tokens almacenados en `localStorage`. El contexto de autenticación (`AuthContext`) gestiona el estado del usuario autenticado.

### Credenciales por defecto

- **Company**: Emp<!-- -->resa Demo
- **Usuario**: admin
- **Contraseña**: admin123

## 🎨 Componentes UI

Los componentes siguen el estilo de Shadcn/UI y están ubicados en `components/ui/`:

- `Button` - Botones con variantes
- `Input` - Campos de entrada
- `Card` - Tarjetas contenedoras
- `Label` - Etiquetas de formulario
- `Loading` - Indicador de carga
- `ErrorMessage` - Mensajes de error

## 📡 API Client

El cliente API está configurado en `lib/api/client.ts` y utiliza `fetch` para las peticiones HTTP. Las funciones de API están organizadas por entidad:

- `lib/api/auth.ts` - Autenticación
- `lib/api/users.ts` - Usuarios
- `lib/api/customers.ts` - Clientes

## 🔄 TanStack Query

Se utiliza TanStack Query para la gestión de estado del servidor. Las queries están configuradas con:

- `staleTime`: 1 minuto
- `refetchOnWindowFocus`: false
- `retry`: 1 intento

## 🛡️ Protección de Rutas

Las rutas protegidas utilizan el componente `ProtectedRoute` que verifica la autenticación antes de renderizar el contenido.

## 📝 Scripts Disponibles

- `npm run dev` - Inicia el servidor de desarrollo
- `npm run build` - Construye la aplicación para producción
- `npm start` - Inicia el servidor de producción
- `npm run lint` - Ejecuta el linter

## 🐛 Solución de Problemas

### Error de conexión a la API

Asegúrate de que:
1. La API backend esté ejecutándose
2. La URL en `.env.local` sea correcta
3. CORS esté configurado correctamente en la API

### Problemas de autenticación

1. Verifica que las credenciales sean correctas
2. Revisa la consola del navegador para errores
3. Limpia el `localStorage` si hay problemas con tokens antiguos

## 📄 Licencia

Este proyecto es parte del sistema GesFer.

