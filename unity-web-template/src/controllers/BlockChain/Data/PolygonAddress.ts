import { IBlockchainAddress } from './IBlockchainAddress';

export default class PolygonAddress implements IBlockchainAddress {
    private _production: boolean;

    constructor(production: boolean) {
        this._production = production;
    }

    get CoinTokenAddress(): string {
        return this._production
            ? "0xB2C63830D4478cB331142FAc075A39671a5541dC"
            : "";
    }

    get SensparkTokenAddress(): string {
        return this._production
            ? "0xFe302B8666539d5046cd9aA0707bB327F5f94C22"
            : "";
    }

    get UsdtTokenAddress(): string {
        return this._production
            ? "0xc2132D05D31c914a87C6611C10748AEb04B58e8F"
            : "";
    }

    get HeroTokenAddress(): string {
        return this._production
            ? "0xd8a06936506379dbBe6e2d8aB1D8C96426320854"
            : "";
    }

    get HeroSTokenAddress(): string {
        return this._production
            ? "0x27313635E6B7AA3CC8436E24BE2317D4A0e56BeB"
            : "";
    }

    get HeroExtendedAddress(): string {
        return "";
    }

    get HouseTokenAddress(): string {
        return this._production
            ? "0x2d5F4Ba3E4a2D991bD72EdBf78F607C174636618"
            : "";
    }

    get DepositAddress(): string {
        return this._production
            ? "0x14EDbb72bd3318F84345bbe816bDef37814AC568"
            : "";
    }

    get AirDropAddress(): string {
        return "";
    }

    get ClaimManagerAddress(): string {
        return this._production
            ? "0x83B5E78c10257bb4990Eba73E00BbC20c5581745"
            : "";
    }

    get CoinExchangeAddress(): string {
        return this._production
            ? "0x700619afcC6400024dc7Cd3A96A5bFd80637c02D"
            : "";
    }

    get HeroStakeAddress(): string {
        return this._production
            ? "0x810570aa7e16cf14defd69d4c9796f3c1abe2d13"
            : "";
    }

    get BirthdayEventAddress(): string{
        return "";
    }
}