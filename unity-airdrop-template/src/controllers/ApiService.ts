import Logger from "./Logger.ts";
import IObfuscate from "./encrypt/IObfuscate.ts";
import {CookieSettings, getCookieSettings} from "../consts/Settings.ts";
import Cookies from "js-cookie";
import {v4 as uuidv4} from "uuid";
import {Account} from "./WalletService.ts";
import {SupportedNetwork} from "./RpcNetworkUtils.ts";

const TAG = '[API]';
const REQUIRED_HEADERS = {
    'accept': 'application/json',
    'Content-Type': 'application/json',
};
const VERSION_HEADER = 'X-Bc-Version';
const JWT_HEADER = 'authorization';
const CLIENT_TOKEN_EXPIRED_DAYS = 365; // 365 ngày

export default class ApiService {
    constructor(
        isProd: boolean,
        private readonly _logger: Logger,
        private _apiHost: string,
        private readonly _deobfuscate: IObfuscate,
        private readonly _currentVersion: number
    ) {
        generateClientToken(this._logger, getCookieSettings(isProd, this._apiHost));
    }

    private _currentJwt: string = '';
    private _curApiHost: string = '';

    public setApiHost(networkType: SupportedNetwork | undefined) {
        // Set default
        this._curApiHost = this._apiHost;

        // Set new ApiHost for ronin
        if (networkType === 'ronin') {
            this._curApiHost = this._apiHost.replace('web', 'web/ron');
        }

        // Set new ApiHost for base
        if (networkType === 'base') {
            this._curApiHost = this._apiHost.replace('web', 'web/bas');
        }

        // Set new ApiHost for vic
        if (networkType === 'vic') {
            this._curApiHost = this._apiHost.replace('web', 'web/vic');
        }
    }

    public async getNonce(walletAddress: string): Promise<string | null> {
        try {
            const path = `${this._curApiHost}/nonce`;
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
            const path = `${this._curApiHost}/check_proof`;
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

    public async changeNickName(userName: string, newNickName: string): Promise<boolean> {
        try {
            const path = `${this._apiHost}/change_nick_name`;
            const body = JSON.stringify({
                userName: userName,
                newNickName: newNickName
            });
            return await this.sendPost<boolean>(path, body);
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return false;
        }
    }

    public async checkProofForAccount(account: Account): Promise<JwtData | null> {
        try {
            const path = `${this._curApiHost}/check_proof_account`;
            const body = JSON.stringify({
                userName: account.userName,
                password: account.password
            });
            const resData = await this.sendPost<CheckProofResData>(path, body)
            return this.parseJwtData(resData);
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }


    public async refreshJwtToken(refreshToken: string): Promise<JwtData | null> {
        try {
            const path = `${this._curApiHost}/refresh/${refreshToken}`;
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
            [VERSION_HEADER]: this._currentVersion.toString(),
            [JWT_HEADER]: `Bearer ${this._currentJwt}`
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
            [VERSION_HEADER]: this._currentVersion.toString(),
            [JWT_HEADER]: `Bearer ${this._currentJwt}`
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

        this._currentJwt = resData.auth;
        return {
            jwt: resData.auth,
            refreshToken: resData.rf,
            key: this._deobfuscate.deobfuscate(resData.key),
            extraData: resData.extraData
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
    refreshToken: string;
    key: string;
    extraData: string;
}

type GenericResData<T> = {
    success: boolean,
    error: string,
    message: T
};

type CheckProofResData = {
    auth: string; // jwt
    rf: string; // refresh token
    key: string; // rsa public key
    extraData: string;
}

type NonceResData = {
    nonce: string;
}