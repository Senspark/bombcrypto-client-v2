import {IBlockchainAddress} from "./Data/IBlockchainAddress.ts";
import BscAddress from "./Data/BscAddress.ts";
import PolygonAddress from "./Data/PolygonAddress.ts";
import RoninAddress from "./Data/RoninAddress.ts";
import BaseAddress from "./Data/BaseAddress.ts";
import VictionAddress from "./Data/VictionAddress.ts";
import {IBlockchainData} from "./IBlockchainData.ts";
import CoinTokenAbi from "./Data/Abi/CoinTokenAbi.json";
import HeroTokenAbi from './Data/Abi/HeroTokenAbi.json';
import HeroSTokenAbi from './Data/Abi/HeroSTokenAbi.json';
import HeroExtendedAbi from './Data/Abi/HeroExtendedAbi.json';
import HeroDesignAbi from './Data/Abi/HeroDesignAbi.json';
import HouseTokenAbi from './Data/Abi/HouseTokenAbi.json';
import HouseDesignAbi from './Data/Abi/HouseDesignAbi.json';
import DepositAbi from './Data/Abi/DepositAbi.json';
import AirDropAbi from './Data/Abi/AirDropAbi.json';
import ClaimManagerAbi from './Data/Abi/ClaimManagerAbi.json';
import CoinExchangeAbi from './Data/Abi/CoinExchangeAbi.json';
import HeroStakeAbi from './Data/Abi/HeroStakeAbi.json';

export interface IRpcToken {
    index: number;
    chainId: number;
    address: string;
    digit: number;
}

export default class BlockChainData implements IBlockchainData {
    coin_token_address: string;
    senspark_token_address: string;
    usdt_token_address: string;
    hero_token_address: string;
    hero_s_token_address: string;
    hero_extended_address: string;
    house_token_address: string;
    deposit_address: string;
    air_drop_address: string;
    claim_manager_address: string;
    exchange_event_address: string;
    hero_stake_address: string;

    coin_token_abi: JSON = {} as JSON;
    hero_token_abi: JSON = {} as JSON;
    hero_s_token_abi: JSON = {} as JSON;
    hero_extended_abi: JSON = {} as JSON;
    hero_design_abi: JSON = {} as JSON;
    house_token_abi: JSON = {} as JSON;
    house_design_abi: JSON = {} as JSON;
    deposit_abi: JSON = {} as JSON;
    air_drop_abi: JSON = {} as JSON;
    claim_manager_abi: JSON = {} as JSON;
    exchange_event_abi: JSON = {} as JSON;
    hero_stake_abi: JSON = {} as JSON;

    rpcTokens: IRpcToken[];
    
    private readonly _bscAddress: IBlockchainAddress;
    private readonly _polygonAddress: IBlockchainAddress;
    private readonly _roninAddress: IBlockchainAddress;
    private readonly _baseAddress: IBlockchainAddress;
    private readonly _victionAddress: IBlockchainAddress;

    constructor(chainId: string, isProd: boolean) {
        
        this._bscAddress = new BscAddress(isProd);
        this._polygonAddress = new PolygonAddress(isProd);
        this._roninAddress = new RoninAddress(isProd);
        this._baseAddress = new BaseAddress(isProd);
        this._victionAddress = new VictionAddress(isProd);
        const address = this.getAddress(isProd, chainId);

        this.coin_token_address = address.CoinTokenAddress;
        this.senspark_token_address = address.SensparkTokenAddress;
        this.usdt_token_address = address.UsdtTokenAddress;
        this.hero_token_address = address.HeroTokenAddress;
        this.hero_s_token_address = address.HeroSTokenAddress;
        this.hero_extended_address = address.HeroExtendedAddress;
        this.house_token_address = address.HouseTokenAddress;
        this.deposit_address = address.DepositAddress;
        this.air_drop_address = address.AirDropAddress;
        this.claim_manager_address = address.ClaimManagerAddress;
        this.exchange_event_address = address.CoinExchangeAddress;
        this.hero_stake_address = address.HeroStakeAddress;

        this.loadAbi();

        this.rpcTokens = this.getRpc(isProd);
    }

    loadAbi() {
        this.coin_token_abi = JSON.parse(JSON.stringify(CoinTokenAbi));
        this.coin_token_abi = JSON.parse(JSON.stringify(CoinTokenAbi));
        this.hero_token_abi = JSON.parse(JSON.stringify(HeroTokenAbi));
        this.hero_s_token_abi = JSON.parse(JSON.stringify(HeroSTokenAbi));
        this.hero_extended_abi = JSON.parse(JSON.stringify(HeroExtendedAbi));
        this.hero_design_abi = JSON.parse(JSON.stringify(HeroDesignAbi));
        this.house_token_abi = JSON.parse(JSON.stringify(HouseTokenAbi));
        this.house_design_abi = JSON.parse(JSON.stringify(HouseDesignAbi));
        this.deposit_abi = JSON.parse(JSON.stringify(DepositAbi));
        this.air_drop_abi = JSON.parse(JSON.stringify(AirDropAbi));
        this.claim_manager_abi = JSON.parse(JSON.stringify(ClaimManagerAbi));
        this.exchange_event_abi = JSON.parse(JSON.stringify(CoinExchangeAbi));
        this.hero_stake_abi = JSON.parse(JSON.stringify(HeroStakeAbi));
    }

    private getRpc(isProd: boolean): IRpcToken[] {
        return [
            {
                index: 0,
                chainId: isProd ? 56 : 97,
                address: this._bscAddress.CoinTokenAddress,
                digit: 18,
            },
            {
                index: 1,
                chainId: isProd ? 137 : 80002,
                address: this._polygonAddress.CoinTokenAddress,
                digit: 18,
            },
            {
                index: 2,
                chainId: isProd ? 56 : 97,
                address:  this._bscAddress.SensparkTokenAddress,
                digit: 18,
            },
            {
                index: 4,
                chainId: isProd ? 137 : 80002,
                address: this._polygonAddress.SensparkTokenAddress,
                digit: 18,
            },
            {
                index: 3,
                chainId: isProd ? 137 : 80002,
                address: this._polygonAddress.UsdtTokenAddress,
                digit: 6,
            }
        ];
    }


    private getAddress(isProd: boolean, chainId: string): IBlockchainAddress {
        if (isProd) {
            if (chainId === '56') {
                return this._bscAddress;
            } else if (chainId === '137') {
                return this._polygonAddress;
            } else if (chainId === '2020') {
                return this._roninAddress;
            } else if (chainId === '8453') {
                return this._baseAddress;
            } else if (chainId === '88') {
                return this._victionAddress;
            }
            throw new Error(`Invalid chainId ${chainId}`);
        } else {
            if (chainId === '56' || chainId === '97') {
                return this._bscAddress;
            } else if (chainId === '137' || chainId === '80002') {
                return this._polygonAddress;
            } else if (chainId === '2020' || chainId === '2021') {
                return this._roninAddress;
            } else if (chainId === '8453' || chainId === '84532') {
                return this._baseAddress;
            } else if (chainId === '88' || chainId === '89') {
                return this._victionAddress;
            }
            throw new Error(`Invalid chainId ${chainId}`);
        }
    }
}