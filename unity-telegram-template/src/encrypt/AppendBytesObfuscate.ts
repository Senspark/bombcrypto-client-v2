import IObfuscate from "./IObfuscate.ts";
import {base64ToByteArray, byteArrayToBase64} from "../utils/String.ts";


export class AppendBytesObfuscate implements IObfuscate {
    constructor(private readonly _numberOfBytes: number) {
    }

    obfuscate(base64: string): string {
        const bytes = base64ToByteArray(base64);
        return this.obfuscate2(bytes);
    }

    deobfuscate(base64: string): string {
        return byteArrayToBase64(this.deobfuscate2(base64));
    }

    obfuscate2(bytes: Uint8Array): string {
        const newBytes = new Uint8Array(bytes.length + this._numberOfBytes);
        const randomBytes = crypto.getRandomValues(new Uint8Array(this._numberOfBytes));
        newBytes.set(randomBytes, 0);
        newBytes.set(bytes, this._numberOfBytes);
        return byteArrayToBase64(newBytes);
    }

    deobfuscate2(base64: string): Uint8Array {
        const bytes = base64ToByteArray(base64);
        return bytes.slice(this._numberOfBytes);
    }
}