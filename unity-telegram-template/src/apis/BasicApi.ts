import Logger from "../services/Logger.ts";

const REQUIRED_HEADERS = {
    'accept': 'application/json',
    'Content-Type': 'application/json',
};

export default class BasicApi {
    constructor(
        logger: Logger,
    ) {
        this._logger = logger.clone('[API]');
    }

    private readonly _logger: Logger;

    async sendGet<T>(path: string, timeOutSeconds?: number): Promise<T | null> {
        return await this.send(path, {
            method: 'GET',
            headers: REQUIRED_HEADERS,
            credentials: 'include'
        }, timeOutSeconds);
    }

    async sendPost<T>(path: string, body: string, timeOutSeconds?: number): Promise<T | null> {
        return await this.send(path, {
            method: 'POST',
            headers: REQUIRED_HEADERS,
            body: body,
            credentials: 'include',
        }, timeOutSeconds);
    }

    private async send<T>(path: string, data: object, timeOutSeconds?: number): Promise<T | null> {
        const abortController = new AbortController();
        timeOutSeconds ??= 30;

        const timeOutId = setTimeout(() => {
            abortController.abort();
        }, timeOutSeconds * 1000);

        try {
            const res = await fetch(path, {
                ...data,
                signal: abortController.signal,
            });
            return await this.parseResponse(res);
        } catch (e) {
            this._logger.error(`sendPost: Error: ${(e as Error).message}`);
            return null;
        } finally {
            clearTimeout(timeOutId);
        }
    }

    private async parseResponse<T>(res: Response) {
        if (res.status !== 200) {
            throw new Error(`Error: ${res.status}`);
        }
        const j = await res.json() as GenericResData<T>;
        if (!j || !j.success) {
            throw new Error(`Error: ${j.error}`);
        }
        return j.message as T;
    }
}

type GenericResData<T> = {
    success: boolean,
    error: string,
    message: T
};