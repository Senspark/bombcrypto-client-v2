import {defineConfig, loadEnv} from 'vite'
import react from '@vitejs/plugin-react'
import path from "path";
import obfuscatorPlugin from "vite-plugin-javascript-obfuscator";
import {nodePolyfills} from 'vite-plugin-node-polyfills';

// https://vite.dev/config/
export default defineConfig(({mode}) => {
        const isProduction = mode === 'production';
        const env = loadEnv(mode, process.cwd(), '');
        const obfuscatorPluginConfig = obfuscatorPlugin({
            options: {
                // ...  [See more options](https://github.com/javascript-obfuscator/javascript-obfuscator)
                compact: true,
                controlFlowFlattening: true,
                controlFlowFlatteningThreshold: 1,
                deadCodeInjection: true,
                deadCodeInjectionThreshold: 1,
                // debugProtection: true,
                // debugProtectionInterval: 4000,
                disableConsoleOutput: true,
                identifierNamesGenerator: 'hexadecimal',
                log: false,
                numbersToExpressions: true,
                renameGlobals: false,
                selfDefending: true,
                simplify: true,
                splitStrings: true,
                splitStringsChunkLength: 5,
                stringArray: true,
                stringArrayCallsTransform: true,
                stringArrayEncoding: ['rc4'],
                stringArrayIndexShift: true,
                stringArrayRotate: true,
                stringArrayShuffle: true,
                stringArrayWrappersCount: 5,
                stringArrayWrappersChainedCalls: true,
                stringArrayWrappersParametersMaxCount: 5,
                stringArrayWrappersType: 'function',
                stringArrayThreshold: 1,
                transformObjectKeys: true,
                unicodeEscapeSequence: false
            },
        });

        return {
            define: {
                'process.env.SOME_KEY': JSON.stringify(env.SOME_KEY)
            },
            plugins: [
                react(),
                nodePolyfills({
                    include: ['buffer']
                }),
                isProduction && obfuscatorPluginConfig
            ].filter(Boolean), // Filter out falsy values (e.g., plugins not added in development)
            build: {
                sourcemap: false, // Disable source maps in production
            },
            base: '',
            resolve: {
                alias: {
                    "@": path.resolve(__dirname, "./src"),
                    "@assets": path.resolve(__dirname, "./src/assets"),
                }
            }
        }
    }
)


