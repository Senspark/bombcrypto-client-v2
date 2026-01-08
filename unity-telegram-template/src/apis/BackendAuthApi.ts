import {Account, CHAIN, ConnectAdditionalRequest, TonProofItemReplySuccess} from "@tonconnect/ui-react";
import {Address} from "@ton/core";
import IObfuscate from "../encrypt/IObfuscate.ts";
import {AsyncTask, SimpleIntervalJob, ToadScheduler} from "toad-scheduler";
import {CookieSettings, getStorageSettings, StorageSettings} from "../consts/Settings.ts";
import Logger from "../services/Logger.ts";
import Cookies from "js-cookie";
import {v4 as uuidv4} from "uuid";
const REQUIRED_HEADERS = {
    'accept': 'application/json',
    'Content-Type': 'application/json',
};

import BasicApi from "./BasicApi.ts";
import EncryptedStorageService from "../encrypt/EncryptedStorageService.ts";

const CLIENT_TOKEN_EXPIRED_DAYS = 365; // 365 ngày
const TAG = '[API]';
const K_SINGLE_REFRESH_TOKEN_ID_JOB = "refreshJwt";
const REFRESH_INTERVAL_SECONDS_TEST = 5 * 60;
const REFRESH_INTERVAL_SECONDS_PROD = 5 * 60;

export default class BackendAuthApiService {
    constructor(
        private readonly _isProd: boolean,
        logger: Logger,
        private readonly _api: BasicApi,
        private readonly _apiHost: string,
        obfuscator: IObfuscate,
        cookieSettings: CookieSettings,
        encryptedStorage: EncryptedStorageService,
    ) {
        this._logger = logger.clone('AUTH');
        this._refreshTokenIntervalSeconds = _isProd ? REFRESH_INTERVAL_SECONDS_PROD : REFRESH_INTERVAL_SECONDS_TEST;
        
        this._encryptedStorage = encryptedStorage
        this._obfuscate = obfuscator;
        this._storageSettings = getStorageSettings(_isProd);

        this._logger = logger;

        try {
            this.updateAccountTon();
            this.generateClientToken(cookieSettings);
            
        } catch {
            this._logger.error(`Initialize Auth Api failed`);
        }
    }
    

    private readonly _logger;
    private readonly _scheduler = new ToadScheduler();
    private readonly _refreshTokenIntervalSeconds: number;
    private readonly _storageSettings: StorageSettings;
    private readonly _encryptedStorage: EncryptedStorageService;
    private readonly _obfuscate: IObfuscate;
    private readonly _accountTon: AccountTon = {
        jwt: null,
        address: null,
        chain: null,
        publicKey: null,
    }
    
    private updateAccountTon(){
        try {
            this._accountTon.jwt = this._encryptedStorage.get(this._storageSettings.keyJwt);
            this._accountTon.publicKey = this._encryptedStorage.get(this._storageSettings.publicKey);
            this._accountTon.chain = this._encryptedStorage.get(this._storageSettings.chain);
            this._accountTon.address = this._encryptedStorage.get(this._storageSettings.keyWalletAddress);
        }
        catch (e){
            this._logger.error(`Get from storage failed: ${(e as Error).message}`);
        }
    }
    
    public async getNonce(): Promise<ConnectAdditionalRequest | null> {
        const reqBody = {
            address: this._accountTon.address
        };

        try {
            const response = await (
                await fetch(`${(this._apiHost)}/ton/nonce`, {
                    method: 'POST',
                    headers: REQUIRED_HEADERS,
                    body: JSON.stringify(reqBody),
                    credentials: 'include',
                })
            ).json();
            const nonce = response.message.payload as string

            return {tonProof: nonce as string};
        } catch {
            return null;
        }
    }

    private _scheduleJob: SimpleIntervalJob | null = null;

    public async generatePayload(): Promise<ConnectAdditionalRequest | null> {

        const path = `${this._apiHost}/ton/generate_payload`;
        const reqBody = JSON.stringify({
            address: this._accountTon.address,
        });
        const res = await this._api.sendPost<GeneratePayloadRes>(path, reqBody);
        if (res) {
            return {tonProof: res.payload};
        }
        return null;
    }

    public async refreshJwt(): Promise<IProofResponseData | null> {
        this._logger.log(`${TAG} start refresh jwt`);
        const jwtRes = await this.refresh();
        if (!jwtRes) {
            this._logger.error(`Error: Cannot refresh jwt`);
            return null;
        }
        this.setLoginToken(jwtRes.auth);
        this.setPublicKey(jwtRes.key);
        this._logger.log(`${TAG} refresh jwt success`);
        return jwtRes;
    }

    private async refresh(): Promise<IProofResponseData | null> {
        try {
            const reqBody = {
                address: this._accountTon.address,
                network: this._accountTon.chain,
            };
            const res = await 
                 fetch(`${this._apiHost}/ton/refresh`, {
                    method: 'POST',
                    headers: REQUIRED_HEADERS,
                    body: JSON.stringify(reqBody),
                    credentials: 'include',
                });
            
            const  response = await res.json();
            const data = response.message as IProofResponseData;
            this.scheduleRefreshJwt();

            return {
                auth: data.auth,
                key: this._obfuscate.deobfuscate(data.key)
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    /**
     * Refresh every 5 minutes
     */
    public scheduleRefreshJwt(): void {
        this.unScheduleRefreshJwt();
        this._logger.log(`${TAG} schedule refresh jwt`);

        const task = new AsyncTask('refreshJwt', () => this.refreshJwt().then(), (e: Error) => {
            this._logger.error(`Error: ${e.message}`);
        });
        this._scheduleJob = new SimpleIntervalJob({seconds: this._refreshTokenIntervalSeconds}, task, {
            id: K_SINGLE_REFRESH_TOKEN_ID_JOB,
            preventOverrun: true,
        });
        this._scheduler.addSimpleIntervalJob(this._scheduleJob);
    }

    public unScheduleRefreshJwt(): void {
        if (this._scheduleJob) {
            this._logger.log(`${TAG} unschedule refresh jwt`);
            const id = this._scheduleJob.id;
            if (id) {
                this._scheduler.stopById(id);
                this._scheduler.removeById(id);
            }
            this._scheduleJob.stop();
        }
        this._scheduleJob = null;
    }

    public async checkProof(proof: TonProofItemReplySuccess['proof'], account: Account, timeOutSeconds?: number): Promise<void> {
        if (this._isProd && account.chain != CHAIN.MAINNET) {
            throw new Error('Invalid network chain');
        }
        const path = `${this._apiHost}/ton/check_proof`;
        const reqBody = JSON.stringify({
            address: account.address,
            network: account.chain,
            public_key: account.publicKey,
            proof: {
                ...proof,
                state_init: account.walletStateInit,
            },
        });

        const res = await this._api.sendPost<IProofResponseData>(path, reqBody, timeOutSeconds);
        if (!res) {
            throw new Error('Authorization failed');
        }

        if (!res.auth || !res.key) {
            throw new Error(`Error: Cannot get jwt or server public key`);
        }


        try{
            if (res) {
                // Directly set the account values instead of reading them back from storage
                // This ensures the values are immediately available
                const publicKey = this._obfuscate.deobfuscate(res.key);
                this._accountTon.publicKey = publicKey;
                this._accountTon.jwt = res.auth;
                this._accountTon.address = account.address;
                this._accountTon.chain = account.chain;
                
                // Still save to storage for persistence
                this._encryptedStorage.set(this._storageSettings.publicKey, publicKey);
                this._encryptedStorage.set(this._storageSettings.keyJwt, res.auth);
                this._encryptedStorage.set(this._storageSettings.keyWalletAddress, account.address);
                this._encryptedStorage.set(this._storageSettings.chain, account.chain);
                
                // Log the values to help debug
                this._logger.log(`${TAG} Account updated: JWT exists: ${!!this._accountTon.jwt}, Address: ${this._accountTon.address}, PublicKey exists: ${!!this._accountTon.publicKey}`);
            }
        }
         catch (e) {
            this._logger.error(e);
            throw new Error('Post Authorization failed');
        }
    }

    public getFriendlyWalletAddress() {
        return this.parseFriendlyAddress();
    }

    public getAccountTon() {
        return this._accountTon;
    }
    
    private setChainNetwork(chain: string){
        this._accountTon.chain = chain;
        return this._encryptedStorage.set(this._storageSettings.chain, chain);
    }
    
    private setPublicKey(publicKey: string){
        this._accountTon.publicKey = publicKey;
        return this._encryptedStorage.set(this._storageSettings.publicKey, publicKey);
    }
    
    private setWalletAddress(walletAddress: string) {
        this._accountTon.address = walletAddress;
        this._encryptedStorage.set(this._storageSettings.keyWalletAddress, walletAddress);
    }

    private setLoginToken(jwt: string) {
        this._accountTon.jwt = jwt;
        this._encryptedStorage.set(this._storageSettings.keyJwt, jwt);
    }
    
    public clear() {
        try {
            this.unScheduleRefreshJwt();
            this._encryptedStorage.remove(this._storageSettings.keyJwt);
            this._encryptedStorage.remove(this._storageSettings.publicKey);
            this._encryptedStorage.remove(this._storageSettings.chain);
            this._encryptedStorage.remove(this._storageSettings.keyWalletAddress);
            // this._encryptedStorage.remove(this._storageSettings.userData);
        }
       catch (e) {
            this._logger.error(e);
        }
        
    }

    private parseFriendlyAddress(): string | null {
        const address = this._accountTon.address;
        if (address == null || this._isProd == null) {
            return null;
        }
        try {
            return Address.parse(address).toString({
                bounceable: false,
                testOnly: !this._isProd
            });
        } catch (e) {
            this._logger.error(e);
            return null;
        }
    }

    private generateClientToken(settings: CookieSettings) {
        this._logger.log(`${TAG} generate client token ${JSON.stringify(settings)}`);

        try {
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
                    this._logger.error(`Cannot set cookie.`)
                }

                this._logger.log(JSON.stringify(result));
            }
        } catch (e) {
            this._logger.error(e);
        }
    }
}

interface IProofResponseData {
    auth: string;
    key: string;
}

type AccountTon = {
    jwt: string | null;
    address: string | null;
    chain: string | null;
    publicKey: string | null;
}

type GeneratePayloadRes = {
    payload: string;
};

export type JwtData = {
    walletAddress: string,
    jwt: string,
    version: number | null,
}

export type JwtDataRes = {
    jwt: string;
    key: string;
    version: number | null;
}

