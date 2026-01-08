export async function sleep(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

export function tonToNanoTo(ton: number) {
    return BigInt(Math.ceil(ton * 1e9));
}

export function getStorage(key: string) {
    return localStorage.getItem(key);
}

export async function copyToClipboard(text: string) {
    try {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'absolute';
        textarea.style.left = '-9999px';
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
        //await navigator.clipboard.writeText(text);
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (err) {
        //ignore
    }
}

export enum JwtValidation {
    Error, Expired, NoAddress, Valid
}

