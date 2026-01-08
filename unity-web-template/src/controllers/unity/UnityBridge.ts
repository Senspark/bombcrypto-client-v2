import {sleep} from "../../utils/Time.ts";
import UnityCommunicator from "./UnityCommunicator.ts";
import Logger from "../Logger.ts";
import UnityCommand from "../../consts/UnityCommand.ts";

type Callback = (arg: string | null) => void;
type Function = (arg: string) => Promise<string|null>;
const methods = new Map<string, Function>;
let _communicator : UnityCommunicator;
let _skipReloadClient = false;

declare global {
    interface Window {
        jsCall: (tag: string, callback: Callback, data: string) => void;
    }
}


function initialize(logger:Logger, communicator: UnityCommunicator) {
    logger.log('UnityBridge initialized');
    window.jsCall = jsCall;
    _communicator = communicator;

    methods.set('test', test);
    methods.set('INIT', communicator.handShakeFromUnity.bind(communicator));
    methods.set('GET_CONNECTION', communicator.getFirstDataConnection.bind(communicator));
    methods.set('ENABLE_VIDEO_THUMBNAIL', communicator.enableVideoThumbnail.bind(communicator));
    methods.set('GET_LOGIN_DATA', communicator.getLoginData.bind(communicator));
    methods.set('GET_JWT_WALLET', communicator.getJwtForWallet.bind(communicator));
    methods.set('GET_JWT_ACCOUNT', communicator.getJwtForAccount.bind(communicator));
    methods.set('INIT_BLOCK_CHAIN_CONFIG', communicator.initBlockChain.bind(communicator));
    methods.set('CALL_BLOCK_CHAIN_METHOD', communicator.callBlockChainMethod.bind(communicator));
    methods.set('CHANGE_NICK_NAME', communicator.changeNickName.bind(communicator));
    methods.set('LOGOUT', communicator.logout.bind(communicator));
    methods.set('DEPOSIT_AIRDROP', communicator.payment.bind(communicator));
}


// Unity call React
function jsCall(tag: string, callback: Callback, data: string) {
    console.log(`UnityBridge.jsCall called with tag=${tag}, data=${data}`);
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
                console.error(e)
            }
        })();
    }
}

function callUnity(cmd: string, data: string) {
    if (window.unityInstance) {
        _communicator?.sendMessageToUnity(cmd, data);
    } else {
        // console.error('Unity instance is not initialized');
    }
}

function callUnityNoEncrypt(cmd: string, data: string) {
    if (window.unityInstance) {
        _communicator?.sendMessageToUnityNoEncrypt(cmd, data);
    } else {
        // console.error('Unity instance is not initialized');
    }
}

function reloadClient() {
    if (_skipReloadClient) {
        _skipReloadClient = false;
        return;
    }
    if (window.unityInstance) {
        _communicator?.sendMessageToUnityNoEncrypt(UnityCommand.RELOAD, "no data");
        window.enableBgVideo(false);
    } else {
        // console.error('Unity instance is not initialized');
    }
}

function skipReloadClient(value: boolean = true) {
    _skipReloadClient = value;
}

async function test(data: string): Promise<string> {
    console.log(data);
    await sleep(1000);
    return Math.floor(Math.random() * 100).toString();
}

const unityBridge = {
    initialize,
    callUnity,
    callUnityNoEncrypt,
    reloadClient,
    skipReloadClient
};

export default unityBridge;