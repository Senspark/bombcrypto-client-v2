import forge from "node-forge";
/**
 * @param text can be: ascii, utf-8, utf-16, utf-32
 */
export function stringToBase64(text: string): string {
    const utf8Bytes = new TextEncoder().encode(text);
    let binary = '';
    utf8Bytes.forEach(byte => binary += String.fromCharCode(byte));
    return btoa(binary);
}

export function base64ToString(base64: string): string {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return new TextDecoder().decode(bytes);
}

/**
 * @param base64 must be a valid base64 string
 */
export function base64ToByteArray(base64: string): Uint8Array {
    const binary = forge.util.decode64(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
}

/**
 * @param text can be: ascii, utf-8, utf-16, utf-32
 */
export function stringToByteArray(text: string): Uint8Array {
    return new TextEncoder().encode(text);
}

/**
 * @return could be: ascii, utf-8, utf-16, utf-32
 */
export function byteArrayToString(bytes: Uint8Array): string {
    return new TextDecoder().decode(bytes);
}

/**
 * @return only base64 string
 */
export function byteArrayToBase64(bytes: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return forge.util.encode64(binary);
}

/**
 * @param uint phải là số nguyên dương (uint, ko phải ulong)
 */
export function uint32ToByteArray(uint: number): Uint8Array {
    if (uint < 0) {
        throw new Error('uint must be positive');
    }
    if (!Number.isInteger(uint)) {
        throw new Error('uint must be an integer');
    }
    if (uint > 0xFFFFFFFF) {
        throw new Error('uint must be less than 2^32');
    }
    const buffer = new ArrayBuffer(4);
    new DataView(buffer).setUint32(0, uint, true);
    return new Uint8Array(buffer);
}

/**
 * @return uint 4 bytes (0-2^32) (ko phải ulong)
 */
export function byteArrayToUint32(bytes: Uint8Array): number {
    if (bytes.length !== 4) {
        throw new Error('bytes must be 4 bytes');
    }
    return new DataView(bytes.buffer).getUint32(0, true);
}

/**
 * Gộp các byte array lại với nhau theo thứ tự
 */
export function mergeByteArray(...args: Uint8Array[]): Uint8Array {
    const totalLength = args.reduce((acc, curr) => acc + curr.length, 0);
    const result = new Uint8Array(totalLength);
    let offset = 0;
    args.forEach((arr) => {
        result.set(arr, offset);
        offset += arr.length;
    });
    return result;
}