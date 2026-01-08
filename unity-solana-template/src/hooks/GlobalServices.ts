import {EnvConfig} from "../configs/EnvConfig.ts";
import {SolanaAdapter} from "@reown/appkit-adapter-solana/react";
import {PhantomWalletAdapter, SolflareWalletAdapter} from "@solana/wallet-adapter-wallets";
import {createAppKit} from "@reown/appkit/react";
import {solana, solanaDevnet} from "@reown/appkit/networks";
import Logger from "../controllers/Logger.ts";
import NotificationService from "../controllers/NotificationService.ts";
import EncryptedStorageService from "../controllers/EncryptedStorageService.ts";
import SECRET from "../configs/Secret.ts";
import WalletService from "../controllers/WalletService.ts";
import AuthService from "../controllers/AuthService.ts";
import PermutationObfuscate from "../controllers/encrypt/PermutationObfuscate.ts";
import UnityCommunicator from "../controllers/unity/UnityCommunicator.ts";
import unityBridge from "../controllers/unity/UnityBridge.ts";
import {AppendBytesObfuscate} from "../controllers/encrypt/AppendBytesObfuscate.ts";
import VersionManager from "../controllers/VersionManager.ts";
import UnityService from "../controllers/UnityService.ts";
import CheckIpService from "../controllers/CheckIpService.ts";
import CheckServerService from "../controllers/CheckServerService.ts";
import ApiService from "../controllers/ApiService.ts";

const isProd = EnvConfig.isProduction();
const apiHost = EnvConfig.apiHost();
const checkIpHost = EnvConfig.apiCheckIpHost();
const ignoreIpCheck = EnvConfig.ignoreIpCheck();


function InitSolana() {
    const solanaWeb3JsAdapter = new SolanaAdapter({
        wallets: [new PhantomWalletAdapter(), new SolflareWalletAdapter()]
    })
    const projectId = EnvConfig.walletProjectId()
    const iconUrl = `${window.location.origin}/favicon.ico`;
    const metadata = {
        name: 'Bombcrypto',
        description: 'Bombcrypto game',
        url: 'https://bombcrypto.io',
        icons: [iconUrl]
    }
    createAppKit({
        adapters: [solanaWeb3JsAdapter],
        networks: [isProd ? solana : solanaDevnet],
        metadata,
        projectId,
        features: {
            analytics: false
        }
    });
}

export const logger = new Logger(!isProd);
export const notificationService = new NotificationService();
const secret = new SECRET();
const encryptedStorage = new EncryptedStorageService(secret.LOCAL_SECRET, secret.LOCAL_IV, logger);
const unityReactObfuscate = new PermutationObfuscate(secret.PERMUTATION_ORDER_32);
const reactApiObfuscate = new AppendBytesObfuscate(secret.APPEND_BYTES);
const versionManager = new VersionManager(logger, notificationService)

export const apiService = new ApiService(isProd, logger, apiHost, reactApiObfuscate, versionManager.getCurrentVersion());
export const walletService = new WalletService(isProd, secret.SIGN_SECRET, secret.SIGN_PADDING, logger, notificationService);
export const authService = new AuthService(isProd, encryptedStorage, walletService, logger, versionManager, apiService);
export const unityCommunicator = new UnityCommunicator(logger, authService, walletService, notificationService, unityReactObfuscate, versionManager);

export const serverService = new CheckServerService(apiHost, apiService, ignoreIpCheck);
export const checkIpService = new CheckIpService(logger, checkIpHost);
export const unityService = new UnityService();unityBridge.initialize(logger, unityCommunicator);

InitSolana();

if (!isProd) {
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.wallet = walletService;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.auth = authService;
}