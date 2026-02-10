# 🚀 Instrucciones de Instalación - GesFer Cliente

## ✅ Estado Actual

He creado toda la estructura del proyecto frontend con:
- ✅ Configuración de Next.js 14+ con TypeScript
- ✅ Tailwind CSS con tema Shadcn/UI
- ✅ TanStack Query configurado
- ✅ Componentes UI completos
- ✅ Sistema de autenticación
- ✅ Páginas principales (Login, Dashboard, Usuarios, Clientes)
- ✅ Script de configuración automática

## 📋 Próximos Pasos Manuales

### 1. Instalar Node.js (si no lo tienes)

**Descarga e instala Node.js desde:** https://nodejs.org/

- Recomendado: Versión LTS (Long Term Support)
- Esto también instalará npm automáticamente

**Verificar instalación:**
```powershell
node --version
npm --version
```

### 2. Ejecutar el Script de Configuración

Una vez que tengas Node.js instalado, ejecuta en PowerShell:

```powershell
cd Cliente
.\setup.ps1
```

Este script:
- ✅ Verificará que Node.js y npm estén instalados
- ✅ Instalará todas las dependencias (`npm install`)
- ✅ Creará el archivo `.env.local` con la configuración

### 3. Iniciar la Aplicación

Después de que el script termine exitosamente:

```powershell
npm run dev
```

La aplicación estará disponible en: **http://localhost:3000**

## 🔧 Configuración Manual (Alternativa)

Si prefieres hacerlo manualmente:

### Paso 1: Instalar Dependencias
```powershell
cd Cliente
npm install
```

### Paso 2: Crear Archivo de Variables de Entorno

Crea un archivo `.env.local` en la carpeta `Cliente` con el siguiente contenido:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

**Nota:** Si tu API está en otro puerto, cambia `5000` por el puerto correcto.

### Paso 3: Iniciar la Aplicación
```powershell
npm run dev
```

## 🔐 Credenciales de Prueba

Una vez que la aplicación esté ejecutándose, puedes iniciar sesión con:

- **Organización**: `Demo Company`
- **Usuario**: `admin`
- **Contraseña**: `admin123`

## 📁 Estructura del Proyecto

```
Cliente/
├── app/                    # Páginas y rutas
│   ├── dashboard/         # Dashboard principal
│   ├── usuarios/          # Gestión de usuarios
│   ├── clientes/          # Gestión de clientes
│   └── login/             # Página de login
├── components/            # Componentes reutilizables
│   ├── ui/                # Componentes base (Button, Input, Card, etc.)
│   ├── auth/              # Componentes de autenticación
│   └── layout/            # Componentes de layout
├── contexts/              # Contextos de React (Auth)
├── lib/                   # Utilidades y configuraciones
│   ├── api/               # Cliente API y funciones
│   ├── providers/         # Providers (QueryProvider)
│   ├── types/             # Tipos TypeScript
│   └── utils/             # Utilidades generales
└── setup.ps1              # Script de configuración automática
```

## 🐛 Solución de Problemas

### Error: "npm no se reconoce"
- **Solución**: Instala Node.js desde https://nodejs.org/
- Reinicia tu terminal después de instalar

### Error: "Cannot find module"
- **Solución**: Ejecuta `npm install` nuevamente
- Si persiste, elimina `node_modules` y `package-lock.json` y vuelve a ejecutar `npm install`

### Error de conexión a la API
- **Verifica**: Que la API backend esté ejecutándose
- **Verifica**: La URL en `.env.local` sea correcta
- **Verifica**: Que CORS esté configurado en la API

### El script de PowerShell no se ejecuta
- **Solución**: Ejecuta PowerShell como administrador o cambia la política de ejecución:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## 📝 Comandos Útiles

```powershell
# Desarrollo
npm run dev

# Construir para producción
npm run build

# Iniciar en producción
npm start

# Linting
npm run lint
```

## ✨ Características Implementadas

- ✅ **TypeScript estricto** con interfaces para todas las respuestas de la API
- ✅ **Tailwind CSS** con diseño moderno estilo Shadcn/UI
- ✅ **TanStack Query** para gestión de estado del servidor
- ✅ **Manejo de errores** y estados de carga
- ✅ **Autenticación completa** con protección de rutas
- ✅ **Layout responsive** con sidebar móvil/desktop
- ✅ **Componentes reutilizables** y tipados

## 🎯 Siguiente Paso

**Instala Node.js y ejecuta el script de configuración:**

```powershell
.\setup.ps1
```

¡Listo para comenzar! 🚀

