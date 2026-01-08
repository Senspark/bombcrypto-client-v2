import {EnvConfig} from "../configs/EnvConfig.ts";
import {EthersAdapter} from '@reown/appkit-adapter-ethers'
import {createAppKit} from "@reown/appkit/react";
import {
    AppKitNetwork,
    base,
    baseSepolia,
    saigon,
    ronin,
    viction,
    victionTestnet
} from "@reown/appkit/networks";
import Logger from "../controllers/Logger.ts";
import NotificationService from "../controllers/NotificationService.ts";
import ConfirmationService from "../controllers/ConfirmationService.ts";
import EncryptedStorageService from "../controllers/EncryptedStorageService.ts";
import SECRET from "../configs/Secret.ts";
import PermutationObfuscate from "../controllers/encrypt/PermutationObfuscate.ts";
import UnityCommunicator from "../controllers/unity/UnityCommunicator.ts";
import unityBridge from "../controllers/unity/UnityBridge.ts";
import {AppendBytesObfuscate} from "../controllers/encrypt/AppendBytesObfuscate.ts";
import ApiService from "../controllers/ApiService.ts";
import CheckServerService from "../controllers/CheckServerService.ts";
import UnityService from "../controllers/UnityService.ts";
import CheckIpService from "../controllers/CheckIpService.ts";
import VersionManager from "../controllers/VersionManager.ts";
import EthereumService from "../controllers/BlockChain/Module/Utils/Ethereum.ts";
import DepositServiceWithInvoice from "../controllers/DepositServiceWithInvoice.ts";
import {StorageProviderBuilder} from "../controllers/StorageProviderBuilder.ts";
import RonBaseAuthService from "../controllers/RonBaseAuthService.ts";
import RonBaseWalletService from "../controllers/RonBaseWalletService.ts";
import loginModal from "../components/LoginModal.tsx";

const isProd = EnvConfig.isProduction();
const apiHost = EnvConfig.apiHost();
const checkIpHost = EnvConfig.apiCheckIpHost();
const ignoreIpCheck = EnvConfig.ignoreIpCheck();

function InitWeb() {
    // const solanaWeb3JsAdapter = new EthersAdapter({
    //     wallets: [new PhantomWalletAdapter(), new SolflareWalletAdapter()]
    // })
    const projectId = EnvConfig.walletProjectId()
    const iconUrl = `${window.location.origin}/favicon.ico`;
    const metadata = {
        name: 'Bombcrypto (Bcoin)',
        description: 'Bombcrypto game',
        url: 'https://bombcrypto.io',
        icons: [iconUrl]
    }

    const networkForTest: [AppKitNetwork, ...AppKitNetwork[]] = [ronin, saigon, base, baseSepolia, viction, victionTestnet];
    const networkForProd: [AppKitNetwork, ...AppKitNetwork[]] = [ronin, base, viction];
    createAppKit({
        adapters: [new EthersAdapter()],
        networks: isProd ? networkForProd : networkForTest,
        metadata,
        projectId,
        features: {
            analytics: false
        }
    });
}

export const logger = new Logger(!isProd);
export const notificationService = new NotificationService();
export const confirmationService = new ConfirmationService();
const secret = new SECRET();
const encryptedStorage = new EncryptedStorageService(secret.LOCAL_SECRET, secret.LOCAL_IV, logger, StorageProviderBuilder.localStorage());
const unityReactObfuscate = new PermutationObfuscate(secret.PERMUTATION_ORDER_32);
const reactApiObfuscate = new AppendBytesObfuscate(secret.APPEND_BYTES);
const versionManager = new VersionManager(logger, notificationService)


export const apiService = new ApiService(isProd, logger, apiHost, reactApiObfuscate, versionManager.getCurrentVersion());
export const walletService = new RonBaseWalletService(isProd, secret.SIGN_SECRET, secret.SIGN_PADDING, logger, notificationService);
export const authService = new RonBaseAuthService(isProd, encryptedStorage, walletService, logger, versionManager, apiService);
export const unityCommunicator = new UnityCommunicator(logger, authService, walletService, unityReactObfuscate, isProd);
export const serverService = new CheckServerService(apiHost, apiService, ignoreIpCheck);
export const checkIpService = new CheckIpService(logger, checkIpHost);
export const ethereumService = new EthereumService(logger);
export const unityService = new UnityService();
export const depositServiceWithInvoice = new DepositServiceWithInvoice(logger, walletService, notificationService);

loginModal.setStatus(isProd)
unityBridge.initialize(logger, unityCommunicator);
InitWeb();

if (!isProd) {
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.wallet = walletService;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.auth = authService;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.deposit = depositServiceWithInvoice;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.com = unityCommunicator;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    window.confirm = confirmationService;
}