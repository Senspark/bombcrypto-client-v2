import {base64ToByteArray, byteArrayToBase64} from "../../utils/String.ts";
import IObfuscate from "./IObfuscate.ts";

export default class PermutationObfuscate implements IObfuscate {
    constructor(
        private readonly _permutationOrder: () => number[]
    ) {
    }

    obfuscate(base64: string): string {
        return this.obfuscate2(base64ToByteArray(base64));
    }

    obfuscate2(bytes: Uint8Array): string {
        const obfuscatedBytes = this.permuteKey(bytes, this._permutationOrder());
        return byteArrayToBase64(obfuscatedBytes);
    }

    deobfuscate(base64: string): string {
        return byteArrayToBase64(this.deobfuscate2(base64));
    }

    deobfuscate2(base64: string): Uint8Array {
        const bytes = base64ToByteArray(base64);
        return this.reversePermutation(bytes, this._permutationOrder());
    }

    private permuteKey(key: Uint8Array, permutationOrder: number[]): Uint8Array {
        if (key.length < permutationOrder.length) {
            throw new Error("Invalid key length.");
        }

        const permutedKey = new Uint8Array(key.length);
        for (let i = 0; i < permutationOrder.length; i++) {
            permutedKey[i] = key[permutationOrder[i]];
        }
        for (let i = permutationOrder.length; i < key.length; i++) {
            permutedKey[i] = key[i];
        }
        return permutedKey;
    }

    private reversePermutation(key: Uint8Array, permutationOrder: number[]): Uint8Array {
        if (key.length < permutationOrder.length) {
            throw new Error("Invalid key length.");
        }

        const originalKey = new Uint8Array(key.length);
        for (let i = 0; i < permutationOrder.length; i++) {
            originalKey[permutationOrder[i]] = key[i];
        }
        for (let i = permutationOrder.length; i < key.length; i++) {
            originalKey[i] = key[i];
        }
        return originalKey;
    }
}

