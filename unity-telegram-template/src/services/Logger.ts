export default class Logger {
    constructor(
        private readonly _prefix: string,
        private readonly _enableLog: boolean
    ) {
        this.log(`[LOGGER] Logger created`);
    }

    log(message: string) {
        if (this._enableLog) {
            console.log(this._prefix, this.getCurrentTime(), message);
        }
    }

    error(message: string | unknown) {
        if (this._enableLog) {
            console.error(this._prefix, this.getCurrentTime(), message);
        }
    }

    clone(prefix: string, enableLog?: boolean): Logger {
        return new Logger(prefix, enableLog ?? this._enableLog);
    }

    private getCurrentTime() {
        return new Date().toLocaleTimeString();
    }
}