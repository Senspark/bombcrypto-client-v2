export default class Logger {
    constructor(private readonly _enableLog: boolean) {
        this.log(`[LOGGER] Logger created`);
    }

    log(message: string) {
        if (this._enableLog) {
            console.log(message);
        }
    }

    error(message: string) {
        if (this._enableLog) {
            console.error(message);
        }
    }
}