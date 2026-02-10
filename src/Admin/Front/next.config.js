/**
 * Configuración de Next.js para Admin
 *
 * La URL de la API se obtiene de:
 * 1. Variable de entorno NEXT_PUBLIC_API_URL (tiene prioridad)
 * 2. Valor por defecto según el entorno (development: localhost:5011, production: desde env)
 */
const getDefaultApiUrl = () => {
  if (process.env.NODE_ENV === 'production') {
    return process.env.NEXT_PUBLIC_API_URL || 'https://admin-api.gesfer.com';
  }
  return process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5011';
};

const path = require('path');
const createNextIntlPlugin = require('next-intl/plugin');

const withNextIntl = createNextIntlPlugin('./i18n/request.ts');

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NEXT_PUBLIC_API_URL: getDefaultApiUrl(),
  },
  experimental: {
    optimizePackageImports: ['@tanstack/react-query'],
  },
  output: "standalone",
  webpack: (config) => {
    config.resolve.modules.push(path.resolve(__dirname, 'node_modules'));
    return config;
  },
};

module.exports = withNextIntl(nextConfig);
