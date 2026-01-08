import forge from "node-forge";

const KEY_SIZE = 32; // AES-256 requires a 32-byte key
const IV_SIZE = 16; // AES requires a 16-byte IV

export default class AesEncryption {
    private key: string | null = null;
    public readonly ivSize: number = IV_SIZE;

    generateKey(): void {
        const randomBytes = forge.random.getBytesSync(KEY_SIZE);
        this.key = forge.util.encode64(randomBytes);
    }

    generateIV(): string {
        const iv = forge.random.getBytesSync(this.ivSize);
        return forge.util.encode64(iv);
    }

    getKeyBase64(): string {
        if (!this.key) {
            throw new Error("AES key not available.");
        }
        return this.key;
    }

    importKey(base64Key: string): void {
        this.key = base64Key;
    }

    encrypt(data: string, base64Iv: string): string {
        if (!this.key) {
            throw new Error("AES key not available.");
        }

        const iv = forge.util.decode64(base64Iv);
        const key = forge.util.decode64(this.key);

        const cipher = forge.cipher.createCipher("AES-CBC", key);
        cipher.start({iv});
        cipher.update(forge.util.createBuffer(data, "utf8"));
        cipher.finish();

        return forge.util.encode64(cipher.output.getBytes());
    }

    /**
     * @returns string|null Return null if decrypt failed
     */
    decrypt(base64Data: string, base64Iv: string): string | null {
        if (!this.key) {
            throw new Error("AES key not available.");
        }

        const iv = forge.util.decode64(base64Iv);
        const key = forge.util.decode64(this.key);
        const encryptedData = forge.util.decode64(base64Data);

        const decipher = forge.cipher.createDecipher("AES-CBC", key);
        decipher.start({iv});
        decipher.update(forge.util.createBuffer(encryptedData));
        const success = decipher.finish();

        if (!success) {
            return null;
        }
        return decipher.output.toString();
    }
}
