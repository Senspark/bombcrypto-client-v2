export class Message {
    message: string;
    code: boolean;

    constructor(message: string, result: boolean) {
        this.message = message;
        this.code = result;
    }

    toString(): string {
        return JSON.stringify(this);
    }
}

function Info(message: string): Message {
    return new Message(message, true);
}

function Error(error: string): Message {
    return new Message(error, false);
}

export { Info, Error };