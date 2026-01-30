# Guía de Configuración - GesFer Cliente

## ⚠️ Requisitos Previos

Antes de continuar, asegúrate de tener instalado:

1. **Node.js 18+** - [Descargar Node.js](https://nodejs.org/)
2. **npm** (viene incluido con Node.js)

Para verificar que están instalados, ejecuta en tu terminal:

```bash
node --version
npm --version
```

## 📦 Instalación de Dependencias

Una vez que tengas Node.js instalado, ejecuta:

```bash
cd Cliente
npm install
```

Este comando instalará todas las dependencias necesarias:
- Next.js 14+
- React 18
- TypeScript
- Tailwind CSS
- TanStack Query
- Lucide React
- Y todas las demás dependencias

## ⚙️ Configuración de Variables de Entorno

El archivo `.env.local` ya ha sido creado con la configuración por defecto:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

Si tu API está ejecutándose en un puerto diferente, edita este archivo.

## 🚀 Ejecutar la Aplicación

### Modo Desarrollo

```bash
npm run dev
```

La aplicación estará disponible en: **http://localhost:3000**

### Modo Producción

```bash
npm run build
npm start
```

## 🔍 Verificar la Instalación

Después de ejecutar `npm install`, deberías ver:

1. Una carpeta `node_modules/` creada
2. Un archivo `package-lock.json` generado
3. Sin errores en la terminal

## 🐛 Solución de Problemas

### Error: "npm no se reconoce"

- Asegúrate de tener Node.js instalado
- Reinicia tu terminal después de instalar Node.js
- Verifica que Node.js esté en tu PATH

### Error: "Cannot find module"

- Ejecuta `npm install` nuevamente
- Elimina `node_modules/` y `package-lock.json` y vuelve a ejecutar `npm install`

### Error de conexión a la API

- Verifica que la API backend esté ejecutándose
- Comprueba la URL en `.env.local`
- Asegúrate de que CORS esté configurado en la API

## 📝 Próximos Pasos

1. ✅ Instalar Node.js (si no lo tienes)
2. ✅ Ejecutar `npm install` en la carpeta Cliente
3. ✅ Verificar que la API backend esté ejecutándose
4. ✅ Ejecutar `npm run dev`
5. ✅ Abrir http://localhost:3000 en el navegador

## 🔐 Credenciales de Prueba

- **Empresa**: Empresa Demo
- **Usuario**: admin
- **Contraseña**: admin123

