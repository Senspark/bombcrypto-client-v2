import { IBlockchainAddress } from './IBlockchainAddress';

export default class BscAddress implements IBlockchainAddress {
    private _production: boolean;

    constructor(production: boolean) {
        this._production = production;
    }

    get CoinTokenAddress(): string {
        return this._production
            ? "0x00e1656e45f18ec6747F5a8496Fd39B50b38396D"
            : "0x648a9cf8e95c73110d28e7e2329b2d0910bd36b8";
    }

    get SensparkTokenAddress(): string {
        return this._production
            ? "0xb43Ac9a81eDA5a5b36839d5b6FC65606815361b0"
            : "0x4B5828F31550aFe15C61D7a765D9597ad4282325";
    }

    get UsdtTokenAddress(): string {
        return "";
    }

    get HeroTokenAddress(): string {
        return this._production
            ? "0x30cc0553f6fa1faf6d7847891b9b36eb559dc618"
            : "0xC1A4C06426B4Df799E455964A20FDe866E86fbd1";
    }

    get HeroSTokenAddress(): string {
        return this._production
            ? "0x9fb9b7349279266c85c0C9dd264D71d2a4B79AB4"
            : "0x2c5a4C5978b814105EDb7148F37Fe07157E03bAD";
    }

    get HeroStakeAddress(): string {
        return this._production
            ? "0x053282c295419E67655a5032A4DA4e3f92D11F17"
            : "0xe3D882b5FC1654782D6579c876975324Ab4D3d07";
    }

    get HeroExtendedAddress(): string {
        return this._production
            ? "0x1f3EE5a5a153e5a30C65a82Efd7598Fd32bBF507"
            : "";
    }

    get HouseTokenAddress(): string {
        return this._production
            ? "0xea3516fEB8F3e387eeC3004330Fd30Aff615496A"
            : "0xB901EE87a6321ea73532C7fDF772dC9790b38c3C";
    }

    get DepositAddress(): string {
        return this._production
            ? "0xad5669fD304aF930C04B5bc7541e5285b638169d"
            : "0x23094e46b74BF9352720a14CcbEf5C85496f65FC";
    }

    get DepositBridgeAddress(): string {
        // Mainnet DepositBridge is deployed in Phase 8; testnet proxy only for now.
        return this._production
            ? ""
            : "0xB3Ed4C979a957A14889b0Cda508963d22dA49832";
    }

    get AirDropAddress(): string {
        return this._production
            ? "0x4b70D3Cd925b21363DB045F9a8B0cf4B16937CeA"
            : "";
    }

    get ClaimManagerAddress(): string {
        return this._production
            ? "0x39328612EC8A6C45b490D524b1C103ACC32f6b6d"
            : "0xc3835d85059f6454433213Ff7A16FA2be40d9a0A";
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