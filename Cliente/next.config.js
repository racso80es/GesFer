const createNextIntlPlugin = require('next-intl/plugin');

const withNextIntl = createNextIntlPlugin('./i18n.ts');

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  },
  // Deshabilitar la optimización de vendor chunks que está causando problemas
  experimental: {
    optimizePackageImports: ['@tanstack/react-query'],
  },
};

module.exports = withNextIntl(nextConfig);

