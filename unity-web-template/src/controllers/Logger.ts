import unityBridge from "./unity/UnityBridge.ts";
import unityCommand from "../consts/UnityCommand.ts";

enum LogType {
    INFO = "INFO",
    ERROR = "ERROR",
}

type ReactLogEntry = {
    type: LogType;
    message: string;
};

declare global {
    interface Window {
        onReactSendLog?: () => void;
    }
}

export default class Logger {
    private readonly _logQueue: ReactLogEntry[] = [];
    private allowSendLog = false;

    constructor(private readonly _enableLog: boolean) {
        // Start sending logs every 0.1 second
        setInterval(() => this.flushOneLog(), 100);

        // Listen for Unity readiness from the global context
        window.onReactSendLog = () => {
            this.allowSendLog = true;
        };

        this.log(`[LOGGER] Logger created`);
    }

    private flushOneLog(): void {
        if (!this.allowSendLog) return; // Wait until Unity is ready
        if (this._logQueue.length === 0) return;

        const {type, message} = this._logQueue.shift()!; // Remove first log
        const formattedMessage = this.formatMessage(type, message);
        unityBridge.callUnityNoEncrypt(unityCommand.REACT_SEND_LOG, formattedMessage);
    }

    private formatMessage(type: LogType, message: string): string {
        return `[${type}] ${message}`;
    }

    log(message: string) {
        this._logQueue.push({type: LogType.INFO, message});
        if (this._enableLog) {
            console.log(message);
        }
    }

    error(message: string) {
        this._logQueue.push({type: LogType.ERROR, message});
        if (this._enableLog) {
            console.error(message);
        }
    }

    errors(...errors: unknown[]) {
        for (const err of errors) {
            if (typeof err === 'string') {
                this._logQueue.push({type: LogType.ERROR, message: err});
            } else if (err instanceof Error) {
                this._logQueue.push({type: LogType.ERROR, message: `${err.message}\n${err.stack?.substring(0, 200)}`});
            } else {
                this._logQueue.push({type: LogType.ERROR, message: JSON.stringify(err)});
            }
        }
        if (this._enableLog) {
            console.error(errors);
        }
    }
}