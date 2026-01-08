
import {base64ToByteArray, byteArrayToBase64, stringToByteArray} from "../utils/String.ts";
import Logger from "../services/Logger.ts";
import AesEncryption from "./AesEncryption.ts";

export default class EncryptedStorageService {
    constructor(
        localSecret: () => string,
        private readonly _localIv: () => string,
        private readonly _logger: Logger,
    ) {
        this._encryptor = new AesEncryption();
        this._encryptor.importKey(localSecret());
    }

    private readonly _encryptor: AesEncryption;

     set(key: string, plainText: string): boolean {
        try {
            const encrypted = this._encryptor.encrypt(plainText, this.getIv(key));
            localStorage.setItem(key, encrypted);
            return true;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return false;
        }
    }

     get(key: string): string | null {
        try {
            const encrypted = localStorage.getItem(key);
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
        localStorage.removeItem(key);
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