import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';
import tailwindcss from '@tailwindcss/vite'

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "io_panel.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7280';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        tailwindcss(),
        plugin()
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '/hubs': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false,
                ws: true
            },
            '/auth': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            },
            '/device': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            },
            '/room': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            },
            '/scene': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            },
            '/automation': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            },
            '/time': {
                target: 'https://localhost:7280',
                changeOrigin: true,
                secure: false
            }
        },
        port: parseInt(env.DEV_SERVER_PORT || '52795'),
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
