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
    publicKey: string;
    chain: string;
}

export const getCookieSettings = (
    isProd: boolean,
    apiHost: string,
): CookieSettings => {
    const url = new URL(apiHost);
    const isSecure = url.protocol === 'https:';

    const domain = isSecure ? '.bombcrypto.io' : 'localhost';
    const secure = isSecure;
    const sameSite: SameSite = isSecure ? 'none' : undefined;
    //const sameSite: SameSite =  'none' ;

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
            publicKey: 'app/public_key',
            chain: 'app/chain'
        };
    }
    // test
    return {
        keyJwt: 'test/connected_key',
        keyWalletAddress: 'test/connected_address',
        publicKey: 'test/public_key',
        chain: 'test/chain'
    };
}
