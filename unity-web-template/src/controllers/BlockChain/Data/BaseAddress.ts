import { IBlockchainAddress } from './IBlockchainAddress';

export default class BaseAddress implements IBlockchainAddress {
    private _production: boolean;

    constructor(production: boolean) {
        this._production = production;
    }

    get CoinTokenAddress(): string {
        return this._production
            ? ""
            : "";
    }

    get SensparkTokenAddress(): string {
        return "";
    }

    get UsdtTokenAddress(): string {
        return "";
    }

    get HeroTokenAddress(): string {
        return "";
    }

    get HeroSTokenAddress(): string {
        return "";
    }

    get HeroExtendedAddress(): string {
        return "";
    }

    get HouseTokenAddress(): string {
        return "";
    }

    get DepositAddress(): string {
        return "";
    }

    get AirDropAddress(): string {
        return "";
    }

    get ClaimManagerAddress(): string {
        return "";
    }

    get CoinExchangeAddress(): string {
        return "";
    }

    get HeroStakeAddress(): string {
        return "";
    }

    get BirthdayEventAddress(): string{
        return "";
    }
}