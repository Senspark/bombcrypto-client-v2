import Logger from "../services/Logger.ts";
import BasicApi from "./BasicApi.ts";

export default class MerchantApiService {
    constructor(
        private readonly _logger: Logger,
        private readonly _apiHost: string,
        private readonly _api: BasicApi,
    ) {
    }

    private _address: Addresses | null = null;

    public async getAddresses(): Promise<Addresses | null> {
        if (this._address) {
            return this._address;
        }
        try {
            const url = `${this._apiHost}/addresses`;
            const res = await this._api.sendGet<Addresses>(url);
            if (res) {
                this._address = res;
                return res;
            }
            return null;
        } catch (e) {
            this._logger.error(e);
            return null;
        }
    }
}

export type Addresses = {
    transferTonTo: string,
    transferBcoinTo: string,
    bcoinAddress: string,
}