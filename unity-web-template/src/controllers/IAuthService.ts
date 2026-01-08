import {Account, HandshakeType} from "./WalletService.ts";

export interface IAuthService {
    /**
     * Nếu return null thì show error message
     */
    getJwt(type: HandshakeType): Promise<JwtDataWallet | null>;

    getJwtForAccount(account: Account): Promise<JwtDataSenspark | null>;

    changeNickName(userName: string, newNickName: string): Promise<boolean>;

    getServerPublicKey(): Promise<string | null>;

    logout(): Promise<void>;

    getAccountData(): Promise<Account | null>;
}


export type ScheduleBy = 'wallet' | 'account' | 'none';

export enum JwtValidation {
    Error, Expired, NoAddress, Valid
}

export type JwtDataWallet = {
    walletAddress: string,
    jwt: string,
    version: number | null,
}

export type JwtDataSenspark = {
    jwt: string,
    version: number | null,
    isUserFi: boolean,
    address: string | null
}
