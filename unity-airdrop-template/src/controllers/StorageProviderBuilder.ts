import { StorageProvider } from "./EncryptedStorageService";

export class StorageProviderBuilder {
    static localStorage(): StorageProvider {
        return {
            setItem: (key: string, value: string) => localStorage.setItem(key, value),
            getItem: (key: string) => localStorage.getItem(key),
            removeItem: (key: string) => localStorage.removeItem(key),
        };
    }

    static sessionStorage(): StorageProvider {
        return {
            setItem: (key: string, value: string) => sessionStorage.setItem(key, value),
            getItem: (key: string) => sessionStorage.getItem(key),
            removeItem: (key: string) => sessionStorage.removeItem(key),
        };
    }
}

