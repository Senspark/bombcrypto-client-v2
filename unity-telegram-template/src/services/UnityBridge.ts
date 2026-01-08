import {EventEmitter} from 'events';
import UnityCommunicator from "./UnityCommunicator.ts";
import Logger from "./Logger.ts";

type Callback = (arg: unknown) => void;
type Function = (arg: string) => Promise<unknown>;
let _eventEmitter: EventEmitter;
// eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
const methods = new Map<string, Function>;
let _logger: Logger = new Logger('[Unity]', false);

declare global {
    interface Window {
        BlockchainManager_Initialize: (config: unknown) => void;
        jsCall: (tag: string, callback: Callback, data: string) => void;
        Javascript_Subscribe: (tag: string, callback: Callback) => void;
        Javascript_Unsubscribe: (tag: string) => void;
    }
}


function initialize(
    logger: Logger,
    eventEmitter: EventEmitter,
    com: UnityCommunicator
) {
    _logger = logger.clone('[Unity]');

    _eventEmitter = eventEmitter;
    window.BlockchainManager_Initialize = BlockchainManager_Initialize;
    window.jsCall = jsCall;
    window.Javascript_Subscribe = Javascript_Subscribe;
    window.Javascript_Unsubscribe = Javascript_Unsubscribe;


    methods.set('INIT', com.handShakeFromUnity.bind(com));
    methods.set('IS_SERVER_MAINTENANCE', com.isMaintenance.bind(com));
    methods.set('GET_JWT_WALLET', com.getDataTelegram.bind(com));
    methods.set('DEPOSIT', com.payment.bind(com));
    methods.set('LOGOUT', com.logout.bind(com));
    methods.set('GET_FRIENDLY_WALLET', com.getFriendlyWalletAddress.bind(com));
    methods.set('COPY_TO_CLIP_BOARD', com.copyToClipboard.bind(com));
    methods.set('OPEN_URL', com.openUrl.bind(com));
    methods.set('GET_START_PARAM', com.getStartParams.bind(com));
    methods.set('IS_IOS_BROWSER', com.isIOSBrowser.bind(com));
    methods.set('IS_ANDROID_BROWSER', com.isAndroidBrowser.bind(com));
}

async function BlockchainManager_Initialize() {
    // do nothing
}

function jsCall(tag: string, callback: Callback, data: string) {
    if (!methods.has(tag)) {
        return;
    }

    const method = methods.get(tag);
    if (method) {
        (async () => {
            try {
                const result = await method(data);
                callback(result);
            } catch (e) {
                _logger.error(e);
            }
        })();
    }
}

async function Javascript_Subscribe(tag: string, callback: Callback) {
    _eventEmitter.on(tag, callback);
}

async function Javascript_Unsubscribe(tag: string) {
    _eventEmitter.removeAllListeners(tag);
}

export const UnityBridge = {
    initialize,
};