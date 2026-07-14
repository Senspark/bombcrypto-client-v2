import {defineConfig, loadEnv} from 'vite'
import react from '@vitejs/plugin-react'
import path from "path";
import obfuscatorPlugin from "vite-plugin-javascript-obfuscator";
import {nodePolyfills} from 'vite-plugin-node-polyfills';

// https://vite.dev/config/
export default defineConfig(({mode, command}) => {
        let isProduction = mode === 'production';
        // command === 'build' khi `vite build`; 'serve' khi dev server (`npm start` = vite --mode production).
        const isBuild = command === 'build';
        const env = loadEnv(mode, process.cwd(), '');
        const apiTarget = new URL(env.VITE_API_HOST);
        const checkIpTarget = new URL(env.VITE_API_CHECK_IP_HOST);
        const stripTrailingSlash = (p: string) => p.replace(/\/$/, '');
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
                // 'variable' thay vì 'function': function-wrapper dùng `arguments`, khi obfuscator chèn
                // vào class field initializer (vd Settings.ts `getSessionKey = () => {...}`) sẽ sinh
                // "'arguments' is not allowed in class field initializer" -> SyntaxError lúc parse.
                stringArrayWrappersType: 'variable',
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
                // Chỉ obfuscate khi BUILD production, KHÔNG chạy ở dev server (npm start) -> dev nhanh + không
                // dính lỗi obfuscator (vd 'arguments' in class field initializer). Build prod vẫn obfuscate.
                isProduction && isBuild && obfuscatorPluginConfig,
            ],
            build: {
                sourcemap: false, // Disable source maps in production
            },
            base: '',
            resolve: {
                alias: {
                    "@": path.resolve(__dirname, "./src"),
                    "@assets": path.resolve(__dirname, "./src/assets"),
                }
            },
            server: {
                proxy: {
                    // /api/login/web/* -> web API (variant ron/bas/... nằm sau 'web' nên giữ nguyên).
                    '/api/login/web': {
                        target: apiTarget.origin,
                        changeOrigin: true,
                        secure: false,
                        rewrite: (p) => p.replace(/^\/api\/login\/web/, stripTrailingSlash(apiTarget.pathname)),
                    },
                    // /api/login/{ip,check_server} -> check-ip API. Đặt SAU /api/login/web (match cụ thể trước).
                    '/api/login': {
                        target: checkIpTarget.origin,
                        changeOrigin: true,
                        secure: false,
                        rewrite: (p) => p.replace(/^\/api\/login/, stripTrailingSlash(checkIpTarget.pathname)),
                    },
                },
            },
        }
    }
)


