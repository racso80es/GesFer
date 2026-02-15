/// <reference types="vitest" />
import { defineConfig } from 'vite'
import path from 'path'

// Vitest Configuration for Node.js Environment (Backend/Electron Main/Core)
export default defineConfig({
  resolve: {
    alias: {
      'inversify': path.resolve(__dirname, './node_modules/inversify'),
      'reflect-metadata/lite': path.resolve(__dirname, './node_modules/reflect-metadata/ReflectLite.js'),
      'reflect-metadata': path.resolve(__dirname, './node_modules/reflect-metadata/Reflect.js'),
    }
  },
  esbuild: {
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
    globals: true,
    environment: 'node',
    // We don't use src/setupTests.ts because it mocks browser globals
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      all: true,
      // Key Change: Include Electron Main Process and Shared Core Logic
      include: [
        'electron/**/*.{ts,tsx}',
        '../../Core/**/*.{ts,tsx}'
      ],
      exclude: [
        'node_modules/**',
        'dist/**',
        '**/*.d.ts',
        '**/*.test.{ts,tsx}',
        '**/*.spec.{ts,tsx}'
      ]
    },
    // We look for specific *.node.test.ts files or tests inside electron/
    include: [
        'src/__tests__/**/*.node.test.{ts,tsx}',
        'electron/**/*.test.{ts,tsx}'
    ],
  }
})
