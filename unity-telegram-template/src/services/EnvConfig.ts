export const EnvConfig = {
    isProduction(): boolean {
        return import.meta.env.VITE_IS_PROD == 'true';
    },

    isCloud(): boolean {
        return import.meta.env.VITE_IS_GCLOUD == 'true';
    },

    network(): string {
        return this.isProduction() ? 'mainnet' : 'testnet';
    },

    unityFolder(): string {
        return import.meta.env.VITE_UNITY_FOLDER
    },

    loaderExtension(): string {
        return import.meta.env.VITE_LOADER_URL_EXTENSION
    },

    dataExtension(): string {
        return import.meta.env.VITE_DATA_URL_EXTENSION
    },

    mobileDataExtension(): string {
        return import.meta.env.VITE_DATA_URL_MOBILE_EXTENSION
    },

    frameworkExtension(): string {
        return import.meta.env.VITE_FRAMEWORK_URL_EXTENSION
    },

    codeExtension(): string {
        return import.meta.env.VITE_CODE_URL_EXTENSION
    },

    apiHost(): string {
        return import.meta.env.VITE_API_HOST
    },

    apiCheckIpHost(): string {
        return import.meta.env.VITE_API_CHECK_IP_HOST
    },

    apiMerchantHost(): string {
        return import.meta.env.VITE_API_MERCHANT_HOST
    },

    ignoreIpCheck(): boolean {
        return import.meta.env.VITE_IGNORE_IP_CHECK == 'true'
    },
    signSecret(): string {
        return import.meta.env.VITE_SIGN_SECRET;
    },
    signPadding(): string {
        return import.meta.env.VITE_SIGN_PADDING;
    },
    localSecret(): string {
        return import.meta.env.VITE_LOCAL_SECRET;
    },
    localIv(): string {
        return import.meta.env.VITE_LOCAL_IV;
    },
    permutationOrder32(): number[] {
        return JSON.parse(import.meta.env.VITE_PERMUTATION_ORDER_32);
    },
    appendBytes(): number {
        return parseInt(import.meta.env.VITE_APPEND_BYTES);
    },
    rsaDelimiter(): string {
        return import.meta.env.VITE_RSA_DELIMITER;
    },
    walletProjectId(): string {
        return import.meta.env.VITE_WALLET_PROJECT_ID;
    },
}