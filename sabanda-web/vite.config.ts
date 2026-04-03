import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    https: {
      key: fs.readFileSync(path.resolve(__dirname, 'certs', 'local-cert-key.pem')),
      cert: fs.readFileSync(path.resolve(__dirname, 'certs', 'local-cert.pem')),
    },
  },
})
