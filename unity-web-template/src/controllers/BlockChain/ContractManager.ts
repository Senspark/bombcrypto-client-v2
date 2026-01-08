/* eslint-disable @typescript-eslint/no-unused-vars,@typescript-eslint/ban-ts-comment */
import BlockChainData from "./BlockChainData.ts";
import Logger from "../Logger.ts";
import CoinToken from "./Module/CoinToken.ts";
import BHeroToken from "./Module/BHero.ts";
import BHeroSToken from "./Module/BHeroS.ts";
import BHeroExtended from "./Module/BHeroExtended.ts";
import BHouseToken from "./Module/BHouse.ts";
import Deposit from "./Module/Deposit.ts";
import AirDrop from "./Module/AirDrop.ts";
import ClaimManager from "./Module/ClaimManager.ts";
import CoinExchange from "./Module/CoinExchange.ts";
import BHeroStake from "./Module/BHeroStake.ts";
import {createRpcTokens} from "./Module/Utils/RpcTokens.ts";
import {setToken} from "./Module/Utils/Storage.ts";
import {getBalance} from "./Module/Utils/RpcTokens.ts";
const TAG = "[CTM]";

export class ContractManager{
    constructor(chainId: string, private readonly _logger: Logger, private readonly _isProd: boolean) {
        const config = new BlockChainData(chainId, this._isProd);
        this._logger.log(`${TAG} BlockChainConfig created: ${config}`);

        this._bcoinToken = new CoinToken(config.coin_token_address, config.coin_token_abi);
        this._sensparkToken = new CoinToken(config.senspark_token_address, config.coin_token_abi);
        this._usdtToken = new CoinToken(config.usdt_token_address, config.coin_token_abi);
        this._bheroToken = new BHeroToken(
            this._bcoinToken,
            this._sensparkToken,
            config.hero_token_address,
            config.hero_token_abi,
            config.hero_design_abi
        );
        this._bHeroSToken = new BHeroSToken(
            this._bcoinToken,
            this._sensparkToken,
            this._bheroToken,
            config.hero_s_token_address,
            config.hero_s_token_abi
        );
        this._heroExtended = new BHeroExtended(
            config.hero_extended_address,
            config.hero_extended_abi
        );
        this._houseToken = new BHouseToken(
            this._bcoinToken,
            config.house_token_address,
            config.house_token_abi,
            config.house_design_abi
        );
        this._deposit = new Deposit(
            config.deposit_address,
            config.deposit_abi
        );
        this._airdrop = new AirDrop(
            config.air_drop_address,
            config.air_drop_abi
        );
        this._claimManager = new ClaimManager(
            config.claim_manager_address,
            config.claim_manager_abi
        );
        this._exchange = new CoinExchange(
            config.exchange_event_address,
            config.exchange_event_abi,
            this._bcoinToken,
            this._usdtToken
        );
        this._heroStake = new BHeroStake(
            this._bcoinToken,
            this._sensparkToken,
            config.hero_stake_address,
            config.hero_stake_abi
        );
        
        setToken("bcoin", this._bcoinToken);
        setToken(this._bcoinToken._address, this._bcoinToken);
        setToken("sen", this._sensparkToken);
        setToken(this._sensparkToken._address, this._sensparkToken);
        setToken("usdt", this._usdtToken);

        createRpcTokens(config.rpcTokens, config.coin_token_abi);
    }

    private readonly _bcoinToken: CoinToken;
    private readonly _sensparkToken: CoinToken;
    private readonly _usdtToken: CoinToken;
    private readonly _bheroToken: BHeroToken;
    private readonly _bHeroSToken: BHeroSToken;
    private readonly _heroExtended: BHeroExtended;
    private readonly _houseToken: BHouseToken;
    private readonly _deposit: Deposit;
    private readonly _airdrop: AirDrop;
    private readonly _claimManager: ClaimManager;
    private readonly _exchange: CoinExchange;
    private readonly _heroStake: BHeroStake;
    
    async getBalance(param: string): Promise<string> {
        const data = JSON.parse(param) as {category: number, walletAddress: string};
        return await getBalance(data.category, data.walletAddress);
    }

    async getCoinBalance(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return await this._bcoinToken.getBalance(data.walletAddress);
    }

    async getSensparkBalance(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return await this._sensparkToken.getBalance(data.walletAddress);
    }

    async getUsdtBalance(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return await this._usdtToken.getBalance(data.walletAddress);
    }

    // @ts-ignore
    async getHeroIdCounter(args: string): Promise<string> {
        return await this._bheroToken.getIdCounter();
    }

    // @ts-ignore
    async getHeroLimit(args: string): Promise<string> {
        return await this._bheroToken.getTokenLimit();
    }

    async getHeroPrice(args: string): Promise<string> {
        const data = JSON.parse(args) as {category: number};
        return await this._bheroToken.getHeroPrice(data.category);
    }
    // @ts-ignore
    async getHeroUpgradeCost(args: string): Promise<string> {
        return JSON.stringify(await this._bheroToken.getUpgradeCosts());
    }
    // @ts-ignore
    async getHeroAbilityDesigns(args: string): Promise<string> {
        return JSON.stringify(await this._bheroToken.getAbilityDesigns());
    }

    async getClaimableHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return await this._bheroToken.getClaimableTokens(data.walletAddress);
    }

    async getPendingHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bheroToken.getPendingTokens(data.walletAddress));
    }

    async getPendingHeroV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bheroToken.getPendingTokensV2(data.walletAddress));
    }

    async buyHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, amount: number, category: number};
        return JSON.stringify(await this._bheroToken.mint(data.walletAddress, data.amount, data.category));
    }

    async upgradeHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, baseId: number, materialId: number};
        return JSON.stringify(await this._bheroToken.upgrade(data.walletAddress, data.baseId, data.materialId));
    }

    async claimHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bheroToken.claim(data.walletAddress));
    }

    async processTokenRequests(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bheroToken.processTokenRequests(data.walletAddress));
    }

    async processTokenRequestsV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bheroToken.processTokenRequestsV2(data.walletAddress));
    }

    async hasPendingHeroRandomization(args: string): Promise<string> {
        const data = JSON.parse(args) as {heroId: number};
        return JSON.stringify(await this._bheroToken.hasPendingRandomization(data.heroId));
    }

    async randomizeHeroAbilities(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, heroId: number};
        return JSON.stringify(await this._bheroToken.randomizeAbilities(data.walletAddress, data.heroId));
    }

    async processHeroRandomizeAbilities(args: string): Promise<string> {
        const data = JSON.parse(args) as {heroId: number};
        return JSON.stringify(await this._bheroToken.processRandomizeAbilities(data.heroId));
    }
    // @ts-ignore
    async isSuperBoxEnabled(args: string): Promise<string> {
        return JSON.stringify(await this._bheroToken.isSuperBoxEnabled());
    }

    async getHeroSPrice(): Promise<string> {
        return JSON.stringify(await this._bHeroSToken.getHeroPriceFormatted());
    }

    async buyHeroS(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, count: number};
        return JSON.stringify(await this._bHeroSToken.mint(data.walletAddress, data.count));
    }

    async fusionHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {heroIds: number[]};
        return JSON.stringify(await this._bHeroSToken.burnFusion(data.heroIds));
    }

    async fusion(args: string): Promise<string> {
        const data = JSON.parse(args) as {mainHeroIds: number[], secondHeroIds: number[]};
        return JSON.stringify(await this._bHeroSToken.fusion(data.mainHeroIds, data.secondHeroIds));
    }

    async repairShield(args: string): Promise<string> {
        const data = JSON.parse(args) as {idHeroS: number, listHeroIds: number[]};
        return JSON.stringify(await this._bHeroSToken.burnRepairShield(data.idHeroS, data.listHeroIds));
    }

    async getRockAmount(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return JSON.stringify(await this._bHeroSToken.getAmountRock(data.walletAddress));
    }

    async createRock(args: string): Promise<string> {
        const data = JSON.parse(args) as {idHeroesBurn: number[]};
        return await this._bHeroSToken.createRock(data.idHeroesBurn);
    }

    async repairShieldWithRock(args: string): Promise<string> {
        const data = JSON.parse(args) as {idHero: number, amountRock: number};
        return JSON.stringify(await this._bHeroSToken.resetShieldHeroS(data.idHero, data.amountRock));
    }

    async upgradeShieldLevel(args: string): Promise<string> {
        const data = JSON.parse(args) as {idHero: number, amountRock: number};
        return JSON.stringify(await this._bHeroSToken.upgradeShieldLevel(data.idHero, data.amountRock));
    }

    async upgradeShieldLevelV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {idHero: number, nonce: number, signature: string};
        return JSON.stringify(await this._bHeroSToken.upgradeShieldLevelV2(data.idHero, data.nonce, data.signature));
    }
    // @ts-ignore
    async claimGiveAwayHero(args: string): Promise<string> {
        return JSON.stringify(await this._heroExtended.claim());
    }

    async getGiveAwayHero(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string};
        return await this._heroExtended.getClaimableTokens(data.walletAddress);
    }
    // @ts-ignore
    async getHouseLimit(args: string): Promise<string> {
        return await this._houseToken.getTokenLimit();
    }
    // @ts-ignore
    async getHousePrice(args: string): Promise<string> {
        return JSON.stringify(await this._houseToken.getMintCosts());
    }
    // @ts-ignore
    async getAvailableHouse(args: string): Promise<string> {
        return JSON.stringify(await this._houseToken.getMintAvailable());
    }
    // @ts-ignore
    async getHouseMintLimits(args: string): Promise<string> {
        return JSON.stringify(await this._houseToken.getMintLimits());
    }
    // @ts-ignore
    async getHouseStats(args: string): Promise<string> {
        return JSON.stringify(await this._houseToken.getRarityStats());
    }

    async buyHouse(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, rarity: number};
        return JSON.stringify(await this._houseToken.mint(data.walletAddress, data.rarity));
    }

    async getNFT(args: string): Promise<string> {
        const data = JSON.parse(args) as {amount: number, eventId: string, nonce: string, signature: string};
        return JSON.stringify(await this._airdrop.getNFT(data.amount, data.eventId, data.nonce, data.signature));
    }

    async depositV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, amount: number, category: number};
        return JSON.stringify(await this._deposit.depositV2(data.walletAddress, data.amount, data.category));
    }

    async claimToken(args: string): Promise<string> {
        const data = JSON.parse(args) as {
            tokenType: string,
            amount: number,
            nonce: number,
            details: string,
            signature: string,
            formatType: string,
            waitConfirmations: number};
        return await this._claimManager.claimTokens(
            data.tokenType,
            data.amount,
            data.nonce,
            data.details,
            data.signature,
            data.formatType,
            data.waitConfirmations
        );
    }

    async canUseVoucher(args: string): Promise<string> {
        const data = JSON.parse(args) as {voucherType: string, walletAddress: string};
        return JSON.stringify(await this._claimManager.userCanUseVoucher(data.voucherType, data.walletAddress));
    }
    // @ts-ignore
    async buyHeroUseVoucher(args: string): Promise<string> {
        return "Not implemented";
        //return JSON.stringify(await this._claimManager.buyHeroUseVoucher(...JSON.parse(args)));
    }

    async exchangeBuyBcoin(args: string): Promise<string> {
        const data = JSON.parse(args) as {amount: number, category: number, walletAddress: string};
        return JSON.stringify(await this._exchange.buyTokens(data.amount, data.category, data.walletAddress));
    }
    // @ts-ignore
    async exchangeGetInfo(args: string): Promise<string> {
        return JSON.stringify(await this._exchange.getInfoBuy());
    }

    async withdrawFromHeroIdV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {id: number, amount: number, tokenAddress: string};
        return JSON.stringify(await this._heroStake.withdrawV2(data.id, data.amount, data.tokenAddress));
    }

    async stakeToHeroV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {walletAddress: string, id: number, amount: number, tokenAddress: string, category: number};
        return JSON.stringify(await this._heroStake.depositV2(data.walletAddress, data.id, data.amount, data.tokenAddress, data.category));
    }

    async getStakeFromHeroIdV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {id: number, tokenAddress: string};
        return await this._heroStake.getCoinBalanceV2(data.id, data.tokenAddress);
    }

    async getFeeFromHeroIdV2(args: string): Promise<string> {
        const data = JSON.parse(args) as {id: number, tokenAddress: string};
        return await this._heroStake.getWithdrawFeeV2(data.id, data.tokenAddress);
    }
    

    
    // Test method
    async test(param: string): Promise<string> {
        console.log(`Call method block chain success: ${param}`);
        return "OK";
    }
}