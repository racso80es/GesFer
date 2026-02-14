/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// Vitest Configuration
// We replicate critical parts of vite.config.ts but exclude the electron plugin
// and add test-specific settings.
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      // Must match vite.config.ts to resolve dependencies correctly
      'inversify': path.resolve(__dirname, './node_modules/inversify'),
      'reflect-metadata/lite': path.resolve(__dirname, './node_modules/reflect-metadata/ReflectLite.js'),
      'reflect-metadata': path.resolve(__dirname, './node_modules/reflect-metadata/Reflect.js'),
      '@iota/sdk': path.resolve(__dirname, './node_modules/@iota/sdk'),
      '@iota/sdk-wasm/web/lib/index': path.resolve(__dirname, './node_modules/@iota/sdk-wasm/web/lib/index.js'),
    }
  },
  esbuild: {
    // Critical for Inversify (decorators)
    supported: {
      'top-level-await': true
    },
    keepNames: true,
    tsconfigRaw: {
      compilerOptions: {
        experimentalDecorators: true
      }
    }
  },
  test: {
    globals: true, // Describe, it, expect
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/**',
        'dist/**',
        'electron/**',
        '**/*.d.ts',
        '**/*.test.{ts,tsx}',
        '**/*.spec.{ts,tsx}',
        '**/setupTests.ts'
      ]
    },
    include: ['src/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
  }
})
