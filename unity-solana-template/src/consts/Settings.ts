export type CookieSettings = {
    clientToken: string;
    domain: string;
    secure: boolean,
    sameSite: SameSite,
}

type SameSite = "strict" | "Strict" | "lax" | "Lax" | "none" | "None" | undefined;

export type StorageSettings = {
    keyJwt: string;
    keyWalletAddress: string;
    currentVersion: string;
}

export const getCookieSettings = (
    isProd: boolean,
    apiHost: string,
): CookieSettings => {
    const url = new URL(apiHost);
    const isSecure = url.protocol === 'https:';

    const domain = isSecure ? '.bombcrypto.io' : 'localhost';
    const secure = isSecure;
    const sameSite: SameSite = isSecure ? 'strict' : undefined;
    const obj = {domain, secure, sameSite};

    if (isProd) {
        return {
            clientToken: '_acl_au_1',
            ...obj,
        }
    }
    // test
    return {
        clientToken: '_bcl_au_1',
        ...obj,
    }
}

export const getStorageSettings = (isProd: boolean): StorageSettings => {
    if (isProd) {
        return {
            keyJwt: 'app/connected_key',
            keyWalletAddress: 'app/connected_address',
            currentVersion: 'app/version'
        };
    }
    // test
    return {
        keyJwt: 'test/connected_id',
        keyWalletAddress: 'test/connected_address',
        currentVersion: 'test/version'
    };
}
