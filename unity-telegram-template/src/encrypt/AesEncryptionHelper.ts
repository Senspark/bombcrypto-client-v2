import IObfuscate from "./IObfuscate.ts";
import AesEncryption from "./AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, mergeByteArray} from "../utils/String.ts";

export default class AesEncryptionHelper {
    constructor(
        private readonly _aes: AesEncryption,
        private readonly _obfuscate: IObfuscate,
    ) {
    }

    encrypt(plainText: string): string {
        const iv = this._aes.generateIV();
        const encrypted = this._aes.encrypt(plainText, iv);
        const mergedBytes = mergeByteArray(base64ToByteArray(iv), (base64ToByteArray(encrypted)));
        return this._obfuscate.obfuscate2(mergedBytes);
    }

    decrypt(encryptedData: string): string | null {
        const bytes = this._obfuscate.deobfuscate2(encryptedData);
        const iv = byteArrayToBase64(bytes.slice(0, this._aes.ivSize));
        const data = byteArrayToBase64(bytes.slice(this._aes.ivSize));
        return this._aes.decrypt(data, iv);
    }
}