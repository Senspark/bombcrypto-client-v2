import NFTToken from './Utils/NFTToken.js';
import {convertBN2DArrayToNumber2DArray, createTaskCompleteSource, sleep} from './Utils/Utils.js';
import {getDoubleGasFeeOption, waitForReceipt} from './Utils/NetworkUtils.js';
import {Contract, ethers, TransactionResponse} from "ethers";
import CoinToken from "./CoinToken.ts";
import {toNumberOrNull, toNumberOrZero} from "../../../utils/Number.ts";

const HERO_S_PRICE_MULTIPLIER = 5;

export default class BHeroToken extends NFTToken {
    private readonly _bcoinToken: CoinToken;
    private readonly _sensparkToken: CoinToken;

    constructor(bcoinToken: CoinToken, sensparkToken: CoinToken, address: string, abi: JSON, designAbi: JSON) {
        super(address, abi, designAbi);
        this._bcoinToken = bcoinToken;
        this._sensparkToken = sensparkToken;
    }

    async getRarityStats(): Promise<TransactionResponse> {
        const contract = await this.getDesignContract();
        return await contract.getRarityStats();
    }

    async getIdCounter(): Promise<string> {
        const contract = await this.getContract();
        const value = await contract.tokenIdCounter();
        return value.toString();
    }

    async getTokenLimit(): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getTokenLimit();
        return value.toString();
    }

    async getHeroPrice(category: number): Promise<string> {
        if (category === 0) {
            return this.getMintCost();
        } else if (category === 1) {
            return this.getSenMintCost();
        } else if (category === 2) {
            return this.getSuperBoxMintCost();
        } else {
            throw new Error('Invalid request');
        }
    }

    async getMintCost(): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getMintCost();
        return value.toString();
    }

    async getSenMintCost(): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getSenMintCost();
        return value.toString();
    }

    async getSuperBoxMintCost(): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getSuperBoxMintCost();
        return value.toString();
    }

    async getUpgradeCost(rarity: number, level: number): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getUpgradeCost(rarity, level);
        return value.toString();
    }

    async getUpgradeCosts(): Promise<string[][]> {
        const contract = await this.getDesignContract();
        const value = await contract.getUpgradeCosts();
        const result = convertBN2DArrayToNumber2DArray(value);
        return result;
    }

    async getRandomizeAbilityCost(rarity: number, times: number): Promise<string> {
        const contract = await this.getDesignContract();
        const value = await contract.getRandomizeAbilityCost(rarity, times);
        return value.toString();
    }

    async getAbilityDesigns(): Promise<string[][]> {
        const contract = await this.getDesignContract();
        const value = await contract.getAbilityDesigns();

        const result: string[][] = [];
        value.forEach((e: { toString: () => string; }[]) => {
            const arr: string[] = [];
            for (let i = 0; i < 3; i++) {
                arr.push(e[i].toString());
            }
            result.push(arr);
        });
        return result;
    }

    async getTokenDetails(userAddress: string): Promise<TransactionResponse> {
        const contract = await this.getContract();
        return await contract.getTokenDetailsByOwner(userAddress);
    }

    async isSuperBoxEnabled(): Promise<boolean> {
        const contract = await this.getContract();
        return await contract.isSuperBoxEnabled();
    }

    async claim(userAddress: string): Promise<boolean> {
        try {
            const contract = await this.getContract();
            const estimateGas = await contract.claim.estimateGas(userAddress);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.claim(options);

            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async mint(userAddress: string, count: number, category: number): Promise<boolean> {
        try {
            const cost = await this.getHeroPrice(category);
            let coinToken;
            if (category === 0) {
                coinToken = this._bcoinToken;
            } else if (category === 1) {
                coinToken = this._sensparkToken;
            } else if (category === 2) {
                coinToken = this._sensparkToken;
            } else {
                throw new Error('Invalid request');
            }
            const countBN = ethers.toBigInt(count);
            const costBN = ethers.toBigInt(cost);
            await coinToken.checkAllowance(userAddress, this._address, (costBN * countBN));

            const contract = await this.getContract();
            const estimateGas = await contract.mint.estimateGas(count, category);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.mint(count, category, options);

            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async getClaimableTokens(userAddress: string): Promise<string> {
        const contract = await this.getContract();
        const value = await contract.getClaimableTokens(userAddress);
        return value.toString();
    }

    async getPendingTokens(userAddress: string): Promise<{ pendingHeroes: number }> {
        const contract = await this.getContract();
        const pendingHeroes = await contract.getPendingTokens(userAddress);
        return { pendingHeroes: parseInt(pendingHeroes) };
    }

    async getPendingTokensV2(userAddress: string): Promise<{ pendingHeroes: number, pendingHeroesFusion: number }> {
        const contract = await this.getContract();
        const [pendingHeroes, pendingHeroesFusion] = await contract.getPendingTokensV2(userAddress);
        return { pendingHeroes: parseInt(pendingHeroes), pendingHeroesFusion: parseInt(pendingHeroesFusion) };
    }

    async getProcessableTokens(userAddress: string): Promise<string> {
        const contract = await this.getContract();
        const value = await contract.getProcessableTokens(userAddress);
        return value.toString();
    }

    async processTokenRequests(userAddress: string): Promise<{ result: boolean, fusionFailAmount: number, fusionSuccessHeroIds: number[] }> {
        const response = {
            result: false,
            fusionFailAmount: 0,
            fusionSuccessHeroIds: []
        };
        try {
            const tokens = parseInt(await this.getProcessableTokens(userAddress));
            while (true) {
                const pendingResult = await this.getPendingTokens(userAddress);
                const pendingTokens = pendingResult.pendingHeroes;
                if (pendingTokens === tokens) {
                    break;
                }
                await sleep(2000);
            }

            const contract = await this.getContract();
            const estimateGas = await contract.processTokenRequests.estimateGas();
            const options = getDoubleGasFeeOption(estimateGas);

            const transaction = await contract.processTokenRequests(options);
            await waitForReceipt(transaction);
            response.result = true;
        } catch (ex) {
            console.error(`exception ${ex}`);
        }
        return response;
    }

    async processTokenRequestsV2(userAddress: string): Promise<{ result: boolean, fusionFailAmount: number, fusionSuccessHeroIds: number[] }> {
        const response: { result: boolean, fusionFailAmount: number, fusionSuccessHeroIds: number[] } = {
            result: false,
            fusionFailAmount: 0,
            fusionSuccessHeroIds: []
        };
        try {
            const tokens = parseInt(await this.getProcessableTokens(userAddress));
            let pendingHeroesFusion = 0;
            while (true) {
                const pendingResult = await this.getPendingTokensV2(userAddress);
                const pendingTokens = pendingResult.pendingHeroes;
                pendingHeroesFusion = pendingResult.pendingHeroesFusion;
                if (pendingTokens === tokens) {
                    break;
                }
                await sleep(2000);
            }

            const contract = await this.getContract();
            const estimateGas = await contract.processTokenRequests.estimateGas();
            const options = getDoubleGasFeeOption(estimateGas);

            const fusionTask = this.listenToFusionEvents(pendingHeroesFusion, contract, userAddress);
            const transaction = await contract.processTokenRequests(options);
            await waitForReceipt(transaction);

            const fusionResult = await fusionTask;
            response.result = true;
            response.fusionFailAmount = fusionResult.fusionFailAmount;
            response.fusionSuccessHeroIds = fusionResult.fusionSuccessHeroIds;
        } catch (ex) {
            console.error(`exception ${ex}`);
        }
        return response;
    }

    listenToFusionEvents(totalFusionAmount: number, contract: Contract, walletAddress: string): Promise<{ fusionFailAmount: number, fusionSuccessAmount: number, fusionSuccessHeroIds: number[] }> {
        const fusionResult :{ fusionFailAmount: number, fusionSuccessAmount: number, fusionSuccessHeroIds: number[] } = {
            fusionFailAmount: 0,
            fusionSuccessAmount: 0,
            fusionSuccessHeroIds: []
        };

        const [task, completeTask] = createTaskCompleteSource<{ fusionFailAmount: number, fusionSuccessAmount: number, fusionSuccessHeroIds: number[] }>();
        const successFilter = contract.filters.FusionSuccess(walletAddress);
        const failedFilter = contract.filters.FusionFailed(walletAddress);

        function checkTask() {
            if (fusionResult.fusionFailAmount + fusionResult.fusionSuccessAmount === totalFusionAmount) {
                contract.off(successFilter, onFusionSuccess);
                contract.off(failedFilter, onFusionFailed);
                completeTask(fusionResult);
            }
        }

        function onFusionFailed(countFusionFailed: string) {
            const count = toNumberOrNull(countFusionFailed);
            if(count == null) {
                console.error(`Invalid countFusionFailed ${countFusionFailed}`);
                return;
            }
            fusionResult.fusionFailAmount = count;
            checkTask();
        }

        function onFusionSuccess(countFusionSuccess: string, idHeroFusionSuccess: string[]) {
            const count = toNumberOrNull(countFusionSuccess);
            if(count == null) {
                console.error(`Invalid countFusionFailed ${countFusionSuccess}`);
                return;
            }
            fusionResult.fusionSuccessAmount = count;
            fusionResult.fusionSuccessHeroIds = idHeroFusionSuccess.map((e: string) => toNumberOrZero(e)).filter((e: number) => e !== 0);
            checkTask();
        }

        if (totalFusionAmount > 0) {
            contract.once(successFilter, onFusionSuccess);
            contract.once(failedFilter, onFusionFailed);
        }
        checkTask();

        return task;
    }

    async upgrade(userAddress: string, baseId: number, materialId: number): Promise<boolean> {
        try {
            const cost = await this.getUpgradeCost(5, 3);
            const multiplier = ethers.toBigInt(HERO_S_PRICE_MULTIPLIER);
            const costBN = ethers.toBigInt(cost) * multiplier;
            await this._bcoinToken.checkAllowance(userAddress, this._address, costBN);

            const contract = await this.getContract();
            const estimateGas = await contract.upgrade.estimateGas(baseId, materialId);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.upgrade(baseId, materialId, options);

            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async hasPendingRandomization(heroId: number): Promise<boolean> {
        const contract = await this.getContract();
        return await contract.hasPendingRandomization(heroId);
    }

    async randomizeAbilities(userAddress: string, heroId: number): Promise<boolean> {
        try {
            const cost = await this.getRandomizeAbilityCost(5, 10);
            const multiplier = ethers.toBigInt(HERO_S_PRICE_MULTIPLIER);
            const costBN = ethers.toBigInt(cost) * multiplier;
            await this._bcoinToken.checkAllowance(userAddress, this._address, costBN);

            const contract = await this.getContract();
            const estimateGas = await contract.randomizeAbilities.estimateGas(heroId);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.randomizeAbilities(heroId, options);

            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async processRandomizeAbilities(heroId: number): Promise<boolean> {
        try {
            const contract = await this.getContract();
            const estimateGas = await contract.processRandomizeAbilities.estimateGas(heroId);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.processRandomizeAbilities(heroId, options);

            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }
}