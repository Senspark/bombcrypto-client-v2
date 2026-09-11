import GeneralContract from './Utils/GeneralContract.js';
import { getDoubleGasFeeOptionV2, waitForReceipt } from './Utils/NetworkUtils.js';
import { Contract } from 'ethers';
import CoinToken from './CoinToken.ts';
import BHeroToken from './BHero.ts';
import { HeroActionResult } from './BHeroS.ts';

export interface UpgradePrice {
    /// wei. Zero nos niveis legados (2-5), onde so o nativo e cobrado.
    bcoin: string;
    /// wei. Zero nos niveis legados.
    sen: string;
    /// wei. Cobrado em todos os niveis.
    native: string;
}

/**
 * Contrato de TESTE dos niveis 6-10, que cobra BCOIN + SEN + nativo.
 *
 * O BHeroS em producao cobra somente nativo e tem teto no nivel 5. Este contrato roda por fora,
 * com MINTER_ROLE e BURNER_ROLE no BHeroToken, e cobre os niveis novos.
 *
 * Duas diferencas que o chamador precisa respeitar:
 * - O preco vem em TRES valores, nao um. `getUpgradePriceForHero` devolve os tres de uma vez para
 *   o hero concreto, evitando que a UI tenha que saber a raridade e o indice de nivel.
 * - `upgradeHero` e payable com `require(msg.value == nativeCost)` e sem devolucao de troco, igual
 *   ao BHeroS. O preco precisa ser lido imediatamente antes de assinar.
 */
export default class BHeroUpgradeV2 extends GeneralContract {
    private readonly _bcoinToken: CoinToken;
    private readonly _senToken: CoinToken;
    private readonly _bheroToken: BHeroToken;

    constructor(bcoinToken: CoinToken, senToken: CoinToken, bheroToken: BHeroToken, address: string, abi: JSON) {
        super(address, abi);
        this._bcoinToken = bcoinToken;
        this._senToken = senToken;
        this._bheroToken = bheroToken;
    }

    async getUpgradePrice(baseId: number): Promise<UpgradePrice> {
        const contract = await this.getContract();
        const [bcoin, sen, native] = await contract.getUpgradePriceForHero(baseId);
        return { bcoin: bcoin.toString(), sen: sen.toString(), native: native.toString() };
    }

    async getMaxLevel(): Promise<number> {
        const contract = await this.getContract();
        return Number(await contract.getMaxLevel());
    }

    async requiredMaterialLevel(baseLevel: number): Promise<number> {
        const contract = await this.getContract();
        return Number(await contract.requiredMaterialLevel(baseLevel));
    }

    /**
     * Aprova BCOIN e SEN se preciso e envia o upgrade.
     *
     * A aprovacao vem antes da leitura de gas de proposito: sem allowance o `estimateGas` reverte
     * dentro do `safeTransferFrom` e o erro nao diz nada util.
     */
    async upgradeHero(
        userAddress: string,
        baseId: number,
        materialId: number,
        price: UpgradePrice,
    ): Promise<HeroActionResult> {
        try {
            const bcoinCost = BigInt(price.bcoin);
            const senCost = BigInt(price.sen);
            if (bcoinCost > 0n) {
                await this._bcoinToken.checkAllowance(userAddress, this._address, bcoinCost);
            }
            if (senCost > 0n) {
                await this._senToken.checkAllowance(userAddress, this._address, senCost);
            }

            const value = BigInt(price.native);
            const contract: Contract = await this.getContract();
            const estimateGas = await contract.upgradeHero.estimateGas(baseId, materialId, { value });
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.upgradeHero(baseId, materialId, { ...options, value });
            const txHash = transaction.hash ?? '';
            await waitForReceipt(transaction);
            const details = await this._bheroToken.getTokenDetail(baseId);
            return { success: true, txHash, details };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: '', details: '' };
        }
    }
}
