/**
 * Configuración de Next.js para Admin
 * 
 * La URL de la API se obtiene de:
 * 1. Variable de entorno NEXT_PUBLIC_API_URL (tiene prioridad)
 * 2. Valor por defecto según el entorno (development: localhost:5001, production: desde env)
 */
const getDefaultApiUrl = () => {
  if (process.env.NODE_ENV === 'production') {
    return process.env.NEXT_PUBLIC_API_URL || 'https://admin-api.gesfer.com';
  }
  return process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';
};

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NEXT_PUBLIC_API_URL: getDefaultApiUrl(),
  },
  experimental: {
    optimizePackageImports: ['@tanstack/react-query'],
  },
};

module.exports = nextConfig;
