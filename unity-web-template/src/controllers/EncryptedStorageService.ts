import Logger from "./Logger.ts";
import AesEncryption from "./encrypt/AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, stringToByteArray} from "../utils/String.ts";

export interface StorageProvider {
    setItem(key: string, value: string): void;
    getItem(key: string): string | null;
    removeItem(key: string): void;
}

export default class EncryptedStorageService {
    constructor(
        localSecret: () => string,
        private readonly _localIv: () => string,
        private readonly _logger: Logger,
        private readonly _storage: StorageProvider,
    ) {
        this._encryptor = new AesEncryption();
        this._encryptor.importKey(localSecret());
    }

    private readonly _encryptor: AesEncryption;

    async set(key: string, plainText: string): Promise<boolean> {
        try {
            const encrypted = this._encryptor.encrypt(plainText, this.getIv(key));
            this._storage.setItem(key, encrypted);
            return true;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return false;
        }
    }

    async get(key: string): Promise<string | null> {
        try {
            const encrypted = this._storage.getItem(key);
            if (!encrypted) {
                return null;
            }
            return this._encryptor.decrypt(encrypted, this.getIv(key));
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    remove(key: string): void {
        this._storage.removeItem(key);
    }

    /**
     * get 3 bytes from key + 13 bytes from IV
     */
    private getIv(key: string): string {
        const keyBytes = stringToByteArray(key);
        if (keyBytes.length < 3) {
            throw new Error('Key is too short');
        }
        const ivBytes = base64ToByteArray(this._localIv());
        const mergedBytes = new Uint8Array(16);
        mergedBytes.set(keyBytes.slice(0, 3), 0);
        mergedBytes.set(ivBytes.slice(0, 13), 3);
        return byteArrayToBase64(mergedBytes);
    }
}