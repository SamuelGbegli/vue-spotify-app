import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

import { quasar, transformAssetUrls } from '@quasar/vite-plugin'

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "vue-spotify-app.client";
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
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7143';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin({
      template: {transformAssetUrls}
    }),
  quasar({
    autoImportComponentCase: "combined",
    sassVariables: fileURLToPath(
      new URL("./src/quasar-variables.sass", import.meta.url)
    )
  })],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
      proxy: {
          '^/weatherforecast': {
            target,
            secure: false
          },

        '^/auth': {
          target,
          secure: false
        },

          '^/track': {
            target,
            secure: false
          },
          '^/playlist': {
            target,
            secure: false
          },

          '^/playbackrecord': {
            target,
            secure: false
        },
          '^/playbackqueue': {
          target,
          secure: false
        }
        },
        port: parseInt(env.DEV_SERVER_PORT || '51630'),
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
