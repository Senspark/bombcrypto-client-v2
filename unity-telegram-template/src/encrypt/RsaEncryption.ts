import forge from "node-forge";
import {EnvConfig} from "../services/EnvConfig.ts";

const SIZE = 2048;

export default class RsaEncryption {
    private publicKey?: forge.pki.PublicKey;
    private privateKey?: forge.pki.PrivateKey;
    private readonly delimiter: string = EnvConfig.rsaDelimiter();

    private get maxLength(): number {
        return SIZE / 8 - 42; // OAEP padding
    }

    generateKeyPair(bits: number = SIZE): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            forge.pki.rsa.generateKeyPair({bits}, (err, keypair) => {
                if (err) {
                    reject(err);
                } else {
                    this.publicKey = keypair.publicKey;
                    this.privateKey = keypair.privateKey;
                    resolve();
                }
            });
        });
    }

    exportPublicKey(): string {
        if (!this.publicKey) {
            throw new Error("Public key not available. Generate or import keys first.");
        }
        const publicKeyAsn1 = forge.pki.publicKeyToAsn1(this.publicKey);
        const publicKeyDer = forge.asn1.toDer(publicKeyAsn1).getBytes();
        return forge.util.encode64(publicKeyDer);
    }

    exportPrivateKey(): string {
        if (!this.privateKey) {
            throw new Error("Private key not available. Generate or import keys first.");
        }
        const privateKeyAsn1 = forge.pki.privateKeyToAsn1(this.privateKey);
        const p = forge.pki.wrapRsaPrivateKey(privateKeyAsn1);
        const privateKeyDer = forge.asn1.toDer(p).getBytes();
        return forge.util.encode64(privateKeyDer);
    }

    importPublicKey(base64Key: string): void {
        const publicKeyDer = forge.util.decode64(base64Key);
        const publicKeyAsn1 = forge.asn1.fromDer(publicKeyDer);
        this.publicKey = forge.pki.publicKeyFromAsn1(publicKeyAsn1);
    }

    importPrivateKey(base64Key: string): void {
        const privateKeyDer = forge.util.decode64(base64Key);
        const privateKeyAsn1 = forge.asn1.fromDer(privateKeyDer);
        this.privateKey = forge.pki.privateKeyFromAsn1(privateKeyAsn1);
    }

    encrypt(data: string): string {
        const sb = [];
        for (let i = 0; i < data.length; i += this.maxLength) {
            const length = Math.min(this.maxLength, data.length - i);
            const part = data.substring(i, i + length);
            sb.push(this.encryptPart(part));
            sb.push(this.delimiter);
        }

        return sb.join("");
    }

    decrypt(data: string): string {
        const sb = [];
        const parts = data.split(this.delimiter);
        for (let i = 0; i < parts.length; i++) {
            const part = parts[i];
            if (part.length === 0) continue;
            sb.push(this.decryptPart(part));
        }

        return sb.join("");
    }

    private encryptPart(message: string): string {
        if (!this.publicKey) {
            throw new Error("Public key not available. Import or generate keys first.");
        }
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-expect-error
        const encrypted = this.publicKey.encrypt(message, "RSA-OAEP");
        return forge.util.encode64(encrypted);
    }

    private decryptPart(encryptedMessage: string): string {
        if (!this.privateKey) {
            throw new Error("Private key not available. Import or generate keys first.");
        }
        const encryptedBytes = forge.util.decode64(encryptedMessage);
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-expect-error
        return this.privateKey.decrypt(encryptedBytes, "RSA-OAEP");
    }
}
