import { atom, getDefaultStore } from 'jotai';

export const confirmationController = atom<ConfirmationController>({
    isIdle: true,
    title: '',
    content: '',
    resolve: null,
});

const store = getDefaultStore();

export default class ConfirmationService {
    constructor() {
        store.set(confirmationController, {
            isIdle: true,
            title: '',
            content: '',
            resolve: null,
        });
    }

    async waitUntilConfirmModalReady(): Promise<void> {
        return new Promise((resolve) => {
            const checkIdle = () => {
                const state = store.get(confirmationController);
                if (state.isIdle) {
                    resolve();
                } else {
                    setTimeout(checkIdle, 100);
                }
            };
            checkIdle();
        });
    }

    async showConfirm(title: string, content: string): Promise<boolean> {
        return new Promise((resolve) => {
            store.set(confirmationController, {
                isIdle: false,
                title,
                content,
                resolve,
            });
        });
    }

    handleResponse(result: boolean) {
        const state = store.get(confirmationController);
        if (state.resolve) {
            state.resolve(result);
        }
        store.set(confirmationController, {
            isIdle: true,
            title: '',
            content: '',
            resolve: null,
        });
    }
}

export type ConfirmationController = {
    isIdle: boolean;
    title: string;
    content: string;
    resolve: ((value: boolean) => void) | null;
};