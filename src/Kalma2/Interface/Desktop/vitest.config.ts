import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    // Exclude electron main process code from UI unit tests
    exclude: ['**/node_modules/**', '**/dist/**', '**/electron/**'],
  },
});
