/**
 * Configuración específica para tests
 * 
 * Este archivo se usa cuando NODE_ENV=test o TEST_ENV=true
 * Carga la configuración desde config/test.json
 */

import type { AppConfig } from './config';

export const testConfig: AppConfig = {
  api: {
    url: process.env.API_URL || 'http://127.0.0.1:5000',
  },
  client: {
    url: process.env.CLIENT_URL || 'http://127.0.0.1:3000',
  },
  database: {
    server: process.env.DB_SERVER || '127.0.0.1',
    port: parseInt(process.env.DB_PORT || '3306', 10),
    database: process.env.DB_DATABASE || 'ScrapDb',
    user: process.env.DB_USER || 'scrapuser',
    password: process.env.DB_PASSWORD || 'scrappassword',
    connectionString: process.env.DB_CONNECTION_STRING || 
      `Server=${process.env.DB_SERVER || '127.0.0.1'};Port=${process.env.DB_PORT || '3306'};Database=${process.env.DB_DATABASE || 'ScrapDb'};User=${process.env.DB_USER || 'scrapuser'};Password=${process.env.DB_PASSWORD || 'scrappassword'};CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;`,
  },
  cache: {
    server: process.env.CACHE_SERVER || '127.0.0.1',
    port: parseInt(process.env.CACHE_PORT || '11211', 10),
    enabled: process.env.CACHE_ENABLED !== 'false',
  },
  environment: 'test',
};

export const TEST_API_URL = testConfig.api.url;
export const TEST_CLIENT_URL = testConfig.client.url;
export const TEST_DATABASE_CONFIG = testConfig.database;
export const TEST_DATABASE_CONNECTION_STRING = testConfig.database.connectionString || 
  `Server=${testConfig.database.server};Port=${testConfig.database.port};Database=${testConfig.database.database};User=${testConfig.database.user};Password=${testConfig.database.password};CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;`;
export const TEST_CACHE_CONFIG = testConfig.cache;
export const TEST_CACHE_URL = `${testConfig.cache.server}:${testConfig.cache.port}`;

