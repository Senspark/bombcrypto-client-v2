import Logger from "./Logger.ts";

const REQUIRED_HEADERS = {
    'accept': 'application/json',
    'Content-Type': 'application/json',
};
export default class CheckIpService {
    constructor(
        private readonly _logger: Logger,
        private readonly _apiHost: string,
    ) {
    }
    
    public async checkIp() : Promise<boolean> {
        try {
            const path = `${this._apiHost}/ip/allow`;
            const res = await fetch(path, {
                method: 'GET',
                headers: REQUIRED_HEADERS,
                credentials: 'include',
            });
            if(res.status !== 200){
                return false;
            }
            const response = await res.json() as ResData;
            if(!response.success){
                return false;
            }
            return response.message.result;
        }
        catch (e){
            this._logger.error(`Error: ${(e as Error).message}`);
            return false;
        }
    }
}

type ResData= {
    success: boolean,
    error: string,
    message: {result: boolean}
};