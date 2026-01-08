import Logger from "../Logger.ts";
import AesEncryptionHelper from "../encrypt/AesEncryptionHelper.ts";
import {ContractManager} from "./ContractManager.ts";
import BlockChainCommand from "../../consts/BlockChainCommand.ts";

const TAG = "[BCG]";

export class BlockChainConfig{
    private actions: Map<string, (param: string) => Promise<string>>;
    private readonly _contractManager: ContractManager;

    constructor(data: string,
                isProd: boolean,
                private readonly _logger: Logger,
                private readonly _aesHelper: AesEncryptionHelper)
    {
        this._contractManager = new ContractManager(data, this._logger, isProd);
        this.actions = new Map<string, (param: string) => Promise<string>>();

        this.actions.set(BlockChainCommand.GET_BALANCE, this._contractManager.getBalance.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_COIN_BALANCE, this._contractManager.getCoinBalance.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_SENSPARK_BALANCE, this._contractManager.getSensparkBalance.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_USDT_BALANCE, this._contractManager.getUsdtBalance.bind(this._contractManager));

        this.actions.set(BlockChainCommand.GET_HERO_ID_COUNTER, this._contractManager.getHeroIdCounter.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HERO_LIMIT, this._contractManager.getHeroLimit.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HERO_PRICE, this._contractManager.getHeroPrice.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HERO_UPGRADE_COST, this._contractManager.getHeroUpgradeCost.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HERO_ABILITY_DESIGNS, this._contractManager.getHeroAbilityDesigns.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_CLAIMABLE_HERO, this._contractManager.getClaimableHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_PENDING_HERO, this._contractManager.getPendingHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_PENDING_HERO_V2, this._contractManager.getPendingHeroV2.bind(this._contractManager));
        this.actions.set(BlockChainCommand.BUY_HERO, this._contractManager.buyHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.UPGRADE_HERO, this._contractManager.upgradeHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.CLAIM_HERO, this._contractManager.claimHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.PROCESS_TOKEN_REQUESTS, this._contractManager.processTokenRequests.bind(this._contractManager));
        this.actions.set(BlockChainCommand.PROCESS_TOKEN_REQUESTS_V2, this._contractManager.processTokenRequestsV2.bind(this._contractManager));
        this.actions.set(BlockChainCommand.HAS_PENDING_HERO_RANDOMIZE, this._contractManager.hasPendingHeroRandomization.bind(this._contractManager));
        this.actions.set(BlockChainCommand.RANDOMIZE_HERO_ABILITIES, this._contractManager.randomizeHeroAbilities.bind(this._contractManager));
        this.actions.set(BlockChainCommand.PROCESS_HERO_RANDOMIZE_AB, this._contractManager.processHeroRandomizeAbilities.bind(this._contractManager));
        this.actions.set(BlockChainCommand.IS_SUPER_BOX_ENABLED, this._contractManager.isSuperBoxEnabled.bind(this._contractManager));

        this.actions.set(BlockChainCommand.GET_HERO_S_PRICE, this._contractManager.getHeroSPrice.bind(this._contractManager));
        this.actions.set(BlockChainCommand.BUY_HERO_S, this._contractManager.buyHeroS.bind(this._contractManager));
        this.actions.set(BlockChainCommand.FUSION_HERO, this._contractManager.fusionHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.FUSION, this._contractManager.fusion.bind(this._contractManager));
        this.actions.set(BlockChainCommand.REPAIR_SHIELD, this._contractManager.repairShield.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_ROCK_AMOUNT, this._contractManager.getRockAmount.bind(this._contractManager));
        this.actions.set(BlockChainCommand.CREATE_ROCK, this._contractManager.createRock.bind(this._contractManager));
        this.actions.set(BlockChainCommand.REPAIR_SHIELD_WITH_ROCK, this._contractManager.repairShieldWithRock.bind(this._contractManager));
        this.actions.set(BlockChainCommand.UPGRADE_SHIELD_LEVEL, this._contractManager.upgradeShieldLevel.bind(this._contractManager));
        this.actions.set(BlockChainCommand.UPGRADE_SHIELD_LEVEL_V2, this._contractManager.upgradeShieldLevelV2.bind(this._contractManager));

        this.actions.set(BlockChainCommand.CLAIM_GIVE_AWAY_HERO, this._contractManager.claimGiveAwayHero.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_GIVE_AWAY_HERO, this._contractManager.getGiveAwayHero.bind(this._contractManager));

        this.actions.set(BlockChainCommand.GET_HOUSE_LIMIT, this._contractManager.getHouseLimit.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HOUSE_PRICE, this._contractManager.getHousePrice.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_AVAILABLE_HOUSE, this._contractManager.getAvailableHouse.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HOUSE_MINT_LIMITS, this._contractManager.getHouseMintLimits.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_HOUSE_STATS, this._contractManager.getHouseStats.bind(this._contractManager));
        this.actions.set(BlockChainCommand.BUY_HOUSE, this._contractManager.buyHouse.bind(this._contractManager));

        this.actions.set(BlockChainCommand.GET_NFT, this._contractManager.getNFT.bind(this._contractManager));
        this.actions.set(BlockChainCommand.DEPOSIT_V2, this._contractManager.depositV2.bind(this._contractManager));

        this.actions.set(BlockChainCommand.CLAIM_TOKEN, this._contractManager.claimToken.bind(this._contractManager));
        this.actions.set(BlockChainCommand.CAN_USE_VOUCHER, this._contractManager.canUseVoucher.bind(this._contractManager));
        this.actions.set(BlockChainCommand.BUY_HERO_USE_VOUCHER, this._contractManager.buyHeroUseVoucher.bind(this._contractManager));

        this.actions.set(BlockChainCommand.EXCHANGE_BUY_BCOIN, this._contractManager.exchangeBuyBcoin.bind(this._contractManager));
        this.actions.set(BlockChainCommand.EXCHANGE_GET_INFO, this._contractManager.exchangeGetInfo.bind(this._contractManager));

        this.actions.set(BlockChainCommand.WITHDRAW_FROM_HERO_ID_V2, this._contractManager.withdrawFromHeroIdV2.bind(this._contractManager));
        this.actions.set(BlockChainCommand.STAKE_TO_HERO_V2, this._contractManager.stakeToHeroV2.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_STAKE_FROM_HERO_ID_V2, this._contractManager.getStakeFromHeroIdV2.bind(this._contractManager));
        this.actions.set(BlockChainCommand.GET_FEE_FROM_HERO_ID_V2, this._contractManager.getFeeFromHeroIdV2.bind(this._contractManager));
    }
    
    public async callAction(actionName: string, param: string): Promise<string | null> {
        try {
            this._logger.log(`${TAG} call ${actionName} with param ${param}`);
            const action = this.actions.get(actionName);
            if (action) {
                const result = await action.call(this, param);
                return this._aesHelper.encrypt(result);
            } else {
                this._logger.error(`${TAG} Action ${actionName} not found`);
                return null;
            }
        }
        catch (e) {
            this._logger.error(`${TAG} ${e}`);
            return null;
        }
    }

}