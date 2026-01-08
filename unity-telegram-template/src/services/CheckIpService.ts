import BasicApi from "../apis/BasicApi.ts";

export default class CheckIpService {
    constructor(
        private readonly _apiHost: string,
        private readonly _api: BasicApi,
    ) {
    }

    public async checkIp(): Promise<boolean> {
        const path = `${this._apiHost}/ip/allow`;
        const res = await this._api.sendGet<boolean>(path);
        return res ?? false;
    }
}