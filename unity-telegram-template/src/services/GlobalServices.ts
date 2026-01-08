import {EnvConfig} from "./EnvConfig.ts";
import Logger from "./Logger.ts";
import TonService from "./TonService.ts";
import SECRET from "../consts/Secret.ts";
import PermutationObfuscate from "../encrypt/PermutationObfuscate.ts";
import {AppendBytesObfuscate} from "../encrypt/AppendBytesObfuscate.ts";
import UnityCommunicator from "./UnityCommunicator.ts";
import {UnityBridge} from "./UnityBridge.ts";
import {EventEmitter} from "events";
import BackendAuthApiService from "../apis/BackendAuthApi.ts";
import {getCookieSettings} from "../consts/Settings.ts";
import BasicApi from "../apis/BasicApi.ts";
import CheckIpService from "./CheckIpService.ts";
import MerchantApiService from "../apis/MerchantApiService.ts";
import NotificationService from "./NotificationService.ts";
import EncryptedStorageService from "../encrypt/EncryptedStorageService.ts";
import CheckServerService from "./CheckServerService.ts";
import UnityService from "./UnityService.ts";

const isProd = EnvConfig.isProduction();
const apiHost = EnvConfig.apiHost();
const merchantApiHost = EnvConfig.apiMerchantHost();
const ipApiHost = EnvConfig.apiCheckIpHost();
const ignoreIpCheck = EnvConfig.ignoreIpCheck();

const logger = new Logger('[D]', !isProd);
const notificationService = new NotificationService();
const secret = new SECRET();
const unityReactObfuscate = new PermutationObfuscate(secret.PERMUTATION_ORDER_32);
const reactApiObfuscate = new AppendBytesObfuscate(secret.APPEND_BYTES);
const encryptedStorage = new EncryptedStorageService(secret.LOCAL_SECRET, secret.LOCAL_IV, logger);

const basicApi = new BasicApi(logger);
const authApi = new BackendAuthApiService(isProd, logger, basicApi, apiHost, reactApiObfuscate, getCookieSettings(isProd, apiHost), encryptedStorage);
const merchantApi = new MerchantApiService(logger, merchantApiHost, basicApi);
const ipService = new CheckIpService(ipApiHost, basicApi);
const serverService = new CheckServerService(apiHost, basicApi, ignoreIpCheck);
const tonService = new TonService(logger, isProd, merchantApi, notificationService);
const unityCom = new UnityCommunicator(logger, tonService, authApi, unityReactObfuscate, serverService);
const unityService = new UnityService();

UnityBridge.initialize(logger, new EventEmitter(), unityCom);

if (!isProd) {
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    globalThis.auth = authApi;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    globalThis.com = unityCom;
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    globalThis.ton = tonService;
}

export {
    logger,
    tonService,
    authApi,
    merchantApi,
    ipService,
    unityCom,
    serverService,
    unityService,
};