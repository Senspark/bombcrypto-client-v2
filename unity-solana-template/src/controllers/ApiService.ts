import Logger from "./Logger.ts";
import IObfuscate from "./encrypt/IObfuscate.ts";
import {CookieSettings, getCookieSettings} from "../consts/Settings.ts";
import Cookies from "js-cookie";
import {v4 as uuidv4} from "uuid";
import {toNumberOrNull} from "../utils/Number.ts";

const TAG = '[API]';
const REQUIRED_HEADERS = {
    'accept': 'application/json',
    'Content-Type': 'application/json',
};
const CLIENT_TOKEN_EXPIRED_DAYS = 365; // 365 ngày
const VERSION_HEADER = 'X-Bc-Version';

export default class ApiService {
    constructor(
        isProd: boolean,
        private readonly _logger: Logger,
        private readonly _apiHost: string,
        private readonly _deobfuscate: IObfuscate,
        private readonly _currentVersion: number
    ) {
        generateClientToken(this._logger, getCookieSettings(isProd, this._apiHost));
    }

    public async getNonce(walletAddress: string): Promise<string | null> {
        try {
            const path = `${this._apiHost}/nonce`;
            const body = JSON.stringify({
                walletAddress: walletAddress
            });
            const resData = await this.sendPost<NonceResData>(path, body);
            return resData.nonce;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    public async checkProofAndGetJwtToken(walletAddress: string, signature: string): Promise<JwtData | null> {
        try {
            const path = `${this._apiHost}/check_proof`;
            const body = JSON.stringify({
                walletAddress: walletAddress,
                signature: signature
            });
            const resData = await this.sendPost<CheckProofResData>(path, body)
            return this.parseJwtData(resData);
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    public async refreshJwtToken(): Promise<JwtData | null> {
        try {
            const path = `${this._apiHost}/refresh`;
            const resData = await this.sendGet<CheckProofResData>(path);
            return this.parseJwtData(resData);
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    public async sendPost<T>(path: string, body: string): Promise<T> {
        const headers = {
            ...REQUIRED_HEADERS,
            [VERSION_HEADER]: this._currentVersion.toString()
        };
        const res = await fetch(path, {
            method: 'POST',
            headers: headers,
            body: body,
            credentials: 'include',
        });
        return await this.parseResponse(res);
    }

    public async sendGet<T>(path: string): Promise<T> {
        const headers = {
            ...REQUIRED_HEADERS,
            [VERSION_HEADER]: this._currentVersion.toString()
        };
        const res = await fetch(path, {
            method: 'GET',
            headers: headers,
            credentials: 'include',
        });
        return await this.parseResponse(res);
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

    private parseJwtData(resData: CheckProofResData): JwtData | null {
        if (!resData || !resData.auth || !resData.key) {
            this._logger.error(`Error: Cannot get jwt or server public key`);
            return null;
        }
        
        return {
            jwt: resData.auth,
            key: this._deobfuscate.deobfuscate(resData.key),
            version: toNumberOrNull(resData.version)
        }
    }
}

function generateClientToken(logger: Logger, settings: CookieSettings) {
    logger.log(`${TAG} generate client token ${JSON.stringify(settings)}`);
    if (!Cookies.get(settings.clientToken)) {
        const k = settings.clientToken;
        const v = uuidv4();
        Cookies.set(k, v, {
            secure: settings.secure,
            domain: settings.domain,
            sameSite: settings.sameSite,
            expires: CLIENT_TOKEN_EXPIRED_DAYS
        });

        const result = Cookies.get();
        if (!result[k]) {
            logger.error(`Cannot set cookie.`)
        }

        logger.log(JSON.stringify(result));
    }
}

export type JwtData = {
    jwt: string;
    key: string;
    version: number | null;
}

type GenericResData<T> = {
    success: boolean,
    error: string,
    message: T
};

type CheckProofResData = {
    auth: string; // jwt
    key: string; // rsa public key
    version: string;
}

type NonceResData = {
    nonce: string;
}