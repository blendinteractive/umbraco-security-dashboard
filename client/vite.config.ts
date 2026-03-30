import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  build: {
    lib: {
      entry: resolve(__dirname, 'src/security-dashboard.element.ts'),
      formats: ['es'],
      fileName: 'security-dashboard',
    },
    outDir: '../src/Umbraco.SecurityDashboard/App_Plugins/SecurityDashboard/dist',
    emptyOutDir: true,
    rollupOptions: {
      external: [/^@umbraco-cms\//, /^@umbraco-ui\//],
    },
  },
});
