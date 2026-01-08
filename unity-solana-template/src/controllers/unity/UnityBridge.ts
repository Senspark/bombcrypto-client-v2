import {sleep} from "../../utils/Time.ts";
import UnityCommunicator from "./UnityCommunicator.ts";
import Logger from "../Logger.ts";
import unityCommand from "../../consts/UnityCommand.ts";

type Callback = (arg: string | null) => void;
type Function = (arg: string) => Promise<string|null>;
const methods = new Map<string, Function>;
let _communicator : UnityCommunicator;

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
    methods.set('GET_JWT_WALLET', communicator.getJwtForUnity.bind(communicator));
    methods.set('DEPOSIT', communicator.depositSol.bind(communicator));
    methods.set('DEPOSIT_BCOIN_SOL', communicator.depositBcoin.bind(communicator));
    methods.set('LOGOUT', communicator.logout.bind(communicator));
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
                console.error(e)
            }
        })();
    }
}

function callUnity(cmd: string, data: string) {
    if (window.unityInstance) {
        _communicator?.sendMessageToUnity(cmd, data);
    } else {
        console.error('Unity instance is not initialized');
    }
}

function reloadClient() {
    if (window.unityInstance) {
        _communicator?.sendMessageToUnityNoEncrypt(unityCommand.RELOAD, "no data");
    } else {
        console.error('Unity instance is not initialized');
    }
}

async function test(data: string): Promise<string> {
    console.log(data);
    await sleep(1000);
    return Math.floor(Math.random() * 100).toString();
}

const unityBridge = {
    initialize,
    callUnity,
    reloadClient
};

export default unityBridge;