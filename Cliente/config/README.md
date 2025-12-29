# Configuración de Entornos

Este directorio contiene los archivos de configuración para diferentes entornos de la aplicación GesFer.

## Archivos de Configuración

- **local.json**: Configuración para desarrollo local (127.0.0.1)
- **development.json**: Configuración para entorno de desarrollo (localhost)
- **production.json**: Configuración para producción
- **test.json**: Configuración para tests (mismo que local)

## Estructura

Cada archivo de configuración tiene la siguiente estructura:

```json
{
  "api": {
    "url": "http://127.0.0.1:5000"
  },
  "client": {
    "url": "http://localhost:3000"
  },
  "database": {
    "server": "localhost",
    "port": 3306,
    "database": "ScrapDb",
    "user": "scrapuser",
    "password": "scrappassword",
    "connectionString": "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;"
  },
  "cache": {
    "server": "localhost",
    "port": 11211,
    "enabled": true
  },
  "environment": "local"
}
```

## Uso

### En el código de la aplicación

```typescript
import { API_URL, CLIENT_URL, DATABASE_CONFIG, DATABASE_CONNECTION_STRING, CACHE_CONFIG, CACHE_URL, appConfig } from '@/lib/config';

// Usar directamente las URLs
const response = await fetch(`${API_URL}/api/users`);

// Acceder a la configuración completa
console.log(appConfig.environment);

// Acceder a la configuración de base de datos
console.log(DATABASE_CONFIG.server);
console.log(DATABASE_CONFIG.port);
console.log(DATABASE_CONNECTION_STRING);

// Acceder a la configuración de caché
console.log(CACHE_CONFIG.server);
console.log(CACHE_CONFIG.port);
console.log(CACHE_CONFIG.enabled);
console.log(CACHE_URL);
```

### En tests

```typescript
import { TEST_API_URL, TEST_CLIENT_URL, TEST_DATABASE_CONFIG, TEST_DATABASE_CONNECTION_STRING, TEST_CACHE_CONFIG, TEST_CACHE_URL } from '@/lib/config.test';

const apiClient = new ApiClient(request, TEST_API_URL);
console.log(TEST_DATABASE_CONFIG.server);
console.log(TEST_DATABASE_CONNECTION_STRING);
console.log(TEST_CACHE_CONFIG.server);
console.log(TEST_CACHE_URL);
```

## Variables de Entorno

El sistema de configuración respeta las siguientes variables de entorno:

### API y Cliente
- `NODE_ENV`: Determina el entorno (development, production, test)
- `NEXT_PUBLIC_ENV`: Para el navegador (production, development)
- `NEXT_PUBLIC_API_URL`: URL de la API (tiene prioridad sobre la configuración)
- `NEXT_PUBLIC_CLIENT_URL`: URL del cliente (tiene prioridad sobre la configuración)
- `API_URL`: URL de la API para tests
- `CLIENT_URL`: URL del cliente para tests

### Base de Datos
- `DB_SERVER`: Servidor de base de datos (por defecto: localhost)
- `DB_PORT`: Puerto de base de datos (por defecto: 3306)
- `DB_DATABASE`: Nombre de la base de datos (por defecto: ScrapDb)
- `DB_USER`: Usuario de base de datos (por defecto: scrapuser)
- `DB_PASSWORD`: Contraseña de base de datos (por defecto: scrappassword)
- `DB_CONNECTION_STRING`: Cadena de conexión completa (se genera automáticamente si no se proporciona)

### Caché (Memcached)
- `CACHE_SERVER`: Servidor de caché (por defecto: localhost)
- `CACHE_PORT`: Puerto de caché (por defecto: 11211)
- `CACHE_ENABLED`: Habilitar/deshabilitar caché (por defecto: true, usar 'false' para deshabilitar)

## Detección Automática del Entorno

El sistema detecta automáticamente el entorno:

1. **Node.js (servidor/tests)**:
   - `NODE_ENV=test` o `TEST_ENV=true` → `test`
   - `NODE_ENV=production` → `production`
   - `NODE_ENV=development` → `development`
   - Por defecto → `local`

2. **Navegador (cliente)**:
   - `NEXT_PUBLIC_ENV=production` → `production`
   - `NEXT_PUBLIC_ENV=development` → `development`
   - Por defecto → `development`

## Personalización

Para cambiar las URLs en un entorno específico, edita el archivo correspondiente en `config/` o establece las variables de entorno apropiadas.

