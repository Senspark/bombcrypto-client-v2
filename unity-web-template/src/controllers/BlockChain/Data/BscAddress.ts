import { IBlockchainAddress } from './IBlockchainAddress';

export default class BscAddress implements IBlockchainAddress {
    private _production: boolean;

    constructor(production: boolean) {
        this._production = production;
    }

    get CoinTokenAddress(): string {
        return this._production
            ? "0x00e1656e45f18ec6747F5a8496Fd39B50b38396D"
            : "";
    }

    get SensparkTokenAddress(): string {
        return this._production
            ? "0xb43Ac9a81eDA5a5b36839d5b6FC65606815361b0"
            : "";
    }

    get UsdtTokenAddress(): string {
        return "";
    }

    get HeroTokenAddress(): string {
        return this._production
            ? "0x30cc0553f6fa1faf6d7847891b9b36eb559dc618"
            : "";
    }

    get HeroSTokenAddress(): string {
        return this._production
            ? "0x9fb9b7349279266c85c0C9dd264D71d2a4B79AB4"
            : "";
    }

    get HeroStakeAddress(): string {
        return this._production
            ? "0x053282c295419E67655a5032A4DA4e3f92D11F17"
            : "";
    }

    get HeroExtendedAddress(): string {
        return this._production
            ? "0x1f3EE5a5a153e5a30C65a82Efd7598Fd32bBF507"
            : "";
    }

    get HouseTokenAddress(): string {
        return this._production
            ? "0xea3516fEB8F3e387eeC3004330Fd30Aff615496A"
            : "";
    }

    get DepositAddress(): string {
        return this._production
            ? "0xad5669fD304aF930C04B5bc7541e5285b638169d"
            : "";
    }

    get AirDropAddress(): string {
        return this._production
            ? "0x4b70D3Cd925b21363DB045F9a8B0cf4B16937CeA"
            : "";
    }

    get ClaimManagerAddress(): string {
        return this._production
            ? "0x39328612EC8A6C45b490D524b1C103ACC32f6b6d"
            : "";
    }

    get CoinExchangeAddress(): string {
        return "";
    }

    get BirthdayEventAddress(): string {
        return this._production
            ? "0x65FDF6550C422a80222E9343a0D12C223c3EE4c5"
            : "";
    }
}