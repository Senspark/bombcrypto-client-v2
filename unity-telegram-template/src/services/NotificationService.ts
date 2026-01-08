import {atom, getDefaultStore} from 'jotai'
import {IconType} from "antd/es/notification/interface";

export const notificationController = atom<NotificationController>({
    data: [],
    clear: () => {
    }
});
const store = getDefaultStore();

export default class NotificationService {
    constructor() {
        store.set(notificationController, {data: [], clear: this.clear.bind(this)});
    }

    show(title: string, description: string, duration: number = 5, type: string = 'info') {
        const normal = {title, description, duration, type};
        store.set(notificationController, (n) => this.createNewController(n, [normal]));
    }

    showError(description: string) {
        const normal = {title: 'Error', description, duration: 0, type: 'error'};
        store.set(notificationController, (n) => this.createNewController(n, [normal]));
    }

    showDepositSuccess(txLink: string) {
        const deposit = {txLink};
        store.set(notificationController, (n) => this.createNewController(n, [deposit]));
    }

    clear() {
        const v = store.get(notificationController);
        if (v.data.length > 0) {
            store.set(notificationController, this.createNewController(null, []));
        }
    }

    private createNewController(oldController: NotificationController | null, data: INotification[]) {
        if (!oldController) {
            return {
                data: data,
                clear: this.clear.bind(this)
            }
        } else {
            return {
                data: [...oldController.data, ...data],
                clear: this.clear.bind(this)
            }
        }
    }
}

export type NotificationController = {
    data: INotification[],
    clear: () => void,
}


export interface INotification {
}

export interface INormalNotification extends INotification {
    title: string | null;
    description: string | null;
    duration: number | null;
    type: IconType | null;
}

export interface IDepositNotification extends INotification {
    txLink: string | null;
}