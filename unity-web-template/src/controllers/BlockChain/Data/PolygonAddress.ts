import { IBlockchainAddress } from './IBlockchainAddress';

export default class PolygonAddress implements IBlockchainAddress {
    private _production: boolean;

    constructor(production: boolean) {
        this._production = production;
    }

    get CoinTokenAddress(): string {
        return this._production
            ? "0xB2C63830D4478cB331142FAc075A39671a5541dC"
            : "0xcF693b54F86c49bbBa54Ff887488Bbf84C5D05BF";
    }

    get SensparkTokenAddress(): string {
        return this._production
            ? "0xFe302B8666539d5046cd9aA0707bB327F5f94C22"
            : "0x93567522610828695F36178b180989996082404A";
    }

    get UsdtTokenAddress(): string {
        return this._production
            ? "0xc2132D05D31c914a87C6611C10748AEb04B58e8F"
            : "0x65F3c080Fe88dC4788d0fF04CFE00D8C499964b3";
    }

    get HeroTokenAddress(): string {
        return this._production
            ? "0xd8a06936506379dbBe6e2d8aB1D8C96426320854"
            : "0xF9f21032bcCCe8997bB29Ab9FBE19502191B7596";
    }

    get HeroSTokenAddress(): string {
        return this._production
            ? "0x27313635E6B7AA3CC8436E24BE2317D4A0e56BeB"
            : "0x5F2a8Aa67E11626AD9Dc5671f8cD29762D4532d4";
    }

    get HeroExtendedAddress(): string {
        return "";
    }

    get HouseTokenAddress(): string {
        return this._production
            ? "0x2d5F4Ba3E4a2D991bD72EdBf78F607C174636618"
            : "0x0fc7397017f1bebaf8ffe8220871af2b5b65509d";
    }

    get DepositAddress(): string {
        return this._production
            ? "0x14EDbb72bd3318F84345bbe816bDef37814AC568"
            : "0x48ce46d900105cf14ebf815c9980661c112b16b6";
    }

    get DepositBridgeAddress(): string {
        return this._production
            ? "0xD84E8aCAcE2Bddb2Da8975340E83A165dA51FFc3"
            : "0x97D80e2914bcBd6F957eE804c7B6fe844A0A1cd7";
    }

    get AirDropAddress(): string {
        return "";
    }

    get ClaimManagerAddress(): string {
        return this._production
            ? "0x83B5E78c10257bb4990Eba73E00BbC20c5581745"
            : "0x66e25f1de0a5b33e804be1a4e8c5e9376952b7c9";
    }

    get CoinExchangeAddress(): string {
        return this._production
            ? "0x700619afcC6400024dc7Cd3A96A5bFd80637c02D"
            : "";
    }

    get HeroStakeAddress(): string {
        return this._production
            ? "0x810570aa7e16cf14defd69d4c9796f3c1abe2d13"
            : "0x9b5d2671665d302d5011959236f5b395e753dccd";
    }

    get BirthdayEventAddress(): string{
        return "";
    }
}