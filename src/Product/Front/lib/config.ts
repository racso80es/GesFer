/**
 * Sistema de configuración centralizado para GesFer
 * 
 * Soporta múltiples entornos:
 * - local: Desarrollo local (127.0.0.1)
 * - development: Desarrollo (localhost)
 * - production: Producción
 * - test: Tests (mismo que local)
 */

export interface AppConfig {
  api: {
    url: string;
  };
  client: {
    url: string;
  };
  database: {
    server: string;
    port: number;
    database: string;
    user: string;
    password: string;
    connectionString?: string;
  };
  cache: {
    server: string;
    port: number;
    enabled: boolean;
  };
  environment: string;
}

type Environment = 'local' | 'development' | 'production' | 'test';

/**
 * Obtiene el entorno actual basado en variables de entorno
 */
function getEnvironment(): Environment {
  // En Node.js (tests, scripts)
  if (typeof window === 'undefined') {
    if (process.env.NODE_ENV === 'test' || process.env.TEST_ENV === 'true') {
      return 'test';
    }
    if (process.env.NODE_ENV === 'production') {
      return 'production';
    }
    if (process.env.NODE_ENV === 'development') {
      return 'development';
    }
    // Por defecto en Node.js es local
    return 'local';
  }

  // En el navegador (cliente)
  if (process.env.NEXT_PUBLIC_ENV === 'production') {
    return 'production';
  }
  if (process.env.NEXT_PUBLIC_ENV === 'development') {
    return 'development';
  }
  // Por defecto en navegador es development
  return 'development';
}

/**
 * Carga la configuración según el entorno
 */
function loadConfig(): AppConfig {
  const env = getEnvironment();
  
  try {
    // En Node.js podemos cargar archivos JSON directamente
    if (typeof window === 'undefined') {
      const fs = require('fs');
      const path = require('path');
      const configPath = path.join(process.cwd(), 'config', `${env}.json`);
      
      if (fs.existsSync(configPath)) {
        const configContent = fs.readFileSync(configPath, 'utf-8');
        const config = JSON.parse(configContent);
        
        // Generar connectionString si no está definido
        if (config.database && !config.database.connectionString) {
          config.database.connectionString = 
            `Server=${config.database.server};Port=${config.database.port};Database=${config.database.database};User=${config.database.user};Password=${config.database.password};CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;`;
        }
        
        return config;
      }
    }
    
    // Fallback: usar variables de entorno o valores por defecto
    return getDefaultConfig(env);
  } catch (error) {
    console.warn(`Error loading config for ${env}, using defaults:`, error);
    return getDefaultConfig(env);
  }
}

/**
 * Obtiene configuración por defecto según el entorno
 */
function getDefaultConfig(env: Environment): AppConfig {
  const configs: Record<Environment, AppConfig> = {
    local: {
      api: { url: process.env.NEXT_PUBLIC_API_URL || 'http://127.0.0.1:5000' },
      client: { url: 'http://localhost:3000' },
      database: {
        server: process.env.DB_SERVER || 'localhost',
        port: parseInt(process.env.DB_PORT || '3306', 10),
        database: process.env.DB_DATABASE || 'ScrapDb',
        user: process.env.DB_USER || 'scrapuser',
        password: process.env.DB_PASSWORD || 'scrappassword',
      },
      cache: {
        server: process.env.CACHE_SERVER || 'localhost',
        port: parseInt(process.env.CACHE_PORT || '11211', 10),
        enabled: process.env.CACHE_ENABLED !== 'false',
      },
      environment: 'local',
    },
    development: {
      api: { url: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000' },
      client: { url: 'http://localhost:3000' },
      database: {
        server: process.env.DB_SERVER || 'localhost',
        port: parseInt(process.env.DB_PORT || '3306', 10),
        database: process.env.DB_DATABASE || 'ScrapDb',
        user: process.env.DB_USER || 'scrapuser',
        password: process.env.DB_PASSWORD || 'scrappassword',
      },
      cache: {
        server: process.env.CACHE_SERVER || 'localhost',
        port: parseInt(process.env.CACHE_PORT || '11211', 10),
        enabled: process.env.CACHE_ENABLED !== 'false',
      },
      environment: 'development',
    },
    production: {
      api: { url: process.env.NEXT_PUBLIC_API_URL || 'https://api.gesfer.com' },
      client: { url: process.env.NEXT_PUBLIC_CLIENT_URL || 'https://gesfer.com' },
      database: {
        server: process.env.DB_SERVER || 'localhost',
        port: parseInt(process.env.DB_PORT || '3306', 10),
        database: process.env.DB_DATABASE || 'ScrapDb',
        user: process.env.DB_USER || 'scrapuser',
        password: process.env.DB_PASSWORD || 'scrappassword',
      },
      cache: {
        server: process.env.CACHE_SERVER || 'localhost',
        port: parseInt(process.env.CACHE_PORT || '11211', 10),
        enabled: process.env.CACHE_ENABLED !== 'false',
      },
      environment: 'production',
    },
    test: {
      api: { url: process.env.API_URL || 'http://127.0.0.1:5000' },
      client: { url: process.env.CLIENT_URL || 'http://127.0.0.1:3000' },
      database: {
        server: process.env.DB_SERVER || '127.0.0.1',
        port: parseInt(process.env.DB_PORT || '3306', 10),
        database: process.env.DB_DATABASE || 'ScrapDb',
        user: process.env.DB_USER || 'scrapuser',
        password: process.env.DB_PASSWORD || 'scrappassword',
      },
      cache: {
        server: process.env.CACHE_SERVER || '127.0.0.1',
        port: parseInt(process.env.CACHE_PORT || '11211', 10),
        enabled: process.env.CACHE_ENABLED !== 'false',
      },
      environment: 'test',
    },
  };

  // Generar connectionString si no está definido
  configs[env].database.connectionString = 
    configs[env].database.connectionString || 
    `Server=${configs[env].database.server};Port=${configs[env].database.port};Database=${configs[env].database.database};User=${configs[env].database.user};Password=${configs[env].database.password};CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;`;

  return configs[env];
}

// Cargar configuración
const config = loadConfig();

/**
 * Configuración de la aplicación
 */
export const appConfig: AppConfig = config;

/**
 * URL de la API
 */
export const API_URL = appConfig.api.url;

/**
 * URL del Cliente
 */
export const CLIENT_URL = appConfig.client.url;

/**
 * Entorno actual
 */
export const ENVIRONMENT = appConfig.environment;

/**
 * Configuración de base de datos
 */
export const DATABASE_CONFIG = appConfig.database;

/**
 * Connection string de base de datos
 */
export const DATABASE_CONNECTION_STRING = appConfig.database.connectionString || 
  `Server=${appConfig.database.server};Port=${appConfig.database.port};Database=${appConfig.database.database};User=${appConfig.database.user};Password=${appConfig.database.password};CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;`;

/**
 * Configuración de caché
 */
export const CACHE_CONFIG = appConfig.cache;

/**
 * URL del servidor de caché
 */
export const CACHE_URL = `${appConfig.cache.server}:${appConfig.cache.port}`;

// Exportar por defecto
export default appConfig;

