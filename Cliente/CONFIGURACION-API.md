# 🔧 Configuración de la URL de la API

## Ubicación de la Configuración

La URL de la API se configura en el archivo `.env.local` en la raíz del proyecto `Cliente`.

## Cambiar la URL de la API

### Opción 1: Editar `.env.local` (Recomendado)

Edita el archivo `.env.local` y cambia la URL:

```env
# URL de la API backend
NEXT_PUBLIC_API_URL=http://localhost:5001
```

**Nota:** Cambia `5001` por el puerto donde esté ejecutándose tu API.

### Opción 2: Variable de Entorno del Sistema

También puedes establecer la variable de entorno antes de ejecutar:

**Windows PowerShell:**
```powershell
$env:NEXT_PUBLIC_API_URL="http://localhost:5001"
npm run dev
```

**Windows CMD:**
```cmd
set NEXT_PUBLIC_API_URL=http://localhost:5001
npm run dev
```

**Linux/Mac:**
```bash
NEXT_PUBLIC_API_URL=http://localhost:5001 npm run dev
```

## Configuración Actual

La configuración actual está en:
- **Archivo**: `.env.local`
- **Variable**: `NEXT_PUBLIC_API_URL`
- **Valor por defecto en código**: `http://localhost:5001`

## Importante

⚠️ **Después de cambiar la URL, debes reiniciar el servidor de desarrollo:**

1. Detén el servidor (Ctrl+C)
2. Ejecuta nuevamente: `npm run dev`

## Verificar la Configuración

Para verificar qué URL está usando la aplicación, puedes:

1. Abrir la consola del navegador (F12)
2. Ir a la pestaña "Network" (Red)
3. Realizar una petición (por ejemplo, intentar hacer login)
4. Verificar la URL en las peticiones HTTP

## Puertos Comunes

- **Desarrollo local**: `http://localhost:5000` o `http://localhost:5001`
- **Producción**: Tu URL de producción (ej: `https://api.tudominio.com`)

## Solución de Problemas

### La aplicación no se conecta a la API

1. Verifica que la API esté ejecutándose
2. Verifica que el puerto en `.env.local` sea correcto
3. Verifica que CORS esté configurado en la API
4. Reinicia el servidor de Next.js después de cambiar `.env.local`

### Error: "Cannot connect to API"

- Verifica que la URL en `.env.local` no tenga una barra final (`/`)
- Verifica que la API esté accesible desde tu navegador
- Revisa la consola del navegador para ver el error exacto

