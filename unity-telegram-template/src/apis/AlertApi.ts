import WebApp from '@twa-dev/sdk'

class Alert {
    public show(message: string) {
        if (!WebApp.version) {
            WebApp.showAlert(message);
        } else {
            alert(message);
        }
    }
}

export const AlertApi = new Alert();