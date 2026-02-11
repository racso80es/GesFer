import { defineConfig } from 'vite'
import electron from 'vite-plugin-electron'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  esbuild: {
    supported: {
      'top-level-await': true
    },
    // Inversify needs this
    keepNames: true,
    tsconfigRaw: {
      compilerOptions: {
        experimentalDecorators: true
      }
    }
  },
  resolve: {
    alias: {
      'inversify': path.resolve(__dirname, './node_modules/inversify'),
      'reflect-metadata/lite': path.resolve(__dirname, './node_modules/reflect-metadata/ReflectLite.js'),
      'reflect-metadata': path.resolve(__dirname, './node_modules/reflect-metadata/Reflect.js'),
    }
  },
  plugins: [
    react(),
    electron([
      {
        // Main-Process entry file of the Electron App.
        entry: 'electron/main.ts',
      },
      {
        entry: 'electron/preload.ts',
        onstart(options) {
          options.reload()
        },
      },
    ]),
  ],
})
