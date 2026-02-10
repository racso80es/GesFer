const createNextIntlPlugin = require('next-intl/plugin');

const withNextIntl = createNextIntlPlugin('./i18n.ts');

/**
 * Configuración de Next.js
 * 
 * La URL de la API se obtiene de:
 * 1. Variable de entorno NEXT_PUBLIC_API_URL (tiene prioridad)
 * 2. Valor por defecto según el entorno (development: localhost:5000, production: desde env)
 */
const getDefaultApiUrl = () => {
  if (process.env.NODE_ENV === 'production') {
    return process.env.NEXT_PUBLIC_API_URL || 'https://api.gesfer.com';
  }
  return process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001';
};

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NEXT_PUBLIC_API_URL: getDefaultApiUrl(),
  },
  // Deshabilitar la optimización de vendor chunks que está causando problemas
  experimental: {
    optimizePackageImports: ['@tanstack/react-query'],
  },
  output: "standalone",
  webpack: (config) => {
    // Permitir que los archivos en Shared encuentren las dependencias instaladas en Product
    config.resolve.modules.push(__dirname + "/node_modules");
    return config;
  },
};

module.exports = withNextIntl(nextConfig);

