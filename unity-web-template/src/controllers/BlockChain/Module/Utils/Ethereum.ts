import { WindowWithEthereum } from "@solflare-wallet/metamask-sdk/lib/cjs/types";
import * as Storage from "./Storage.ts";
import {setEthereum} from "./Storage.ts";
import { ExternalProvider } from "@ethersproject/providers";
import Logger from "../../../Logger.ts";

class EthereumService {
    
    constructor(private readonly _logger: Logger) {
        this.initNetwork().then();
    }
    private _isInitialize: boolean = false;
    
    async initNetwork() {
        if(this._isInitialize)
        {
            this._logger.log('EthereumService already initialized');
            return;
        }
        const windowEthereum = (window as WindowWithEthereum);

        const ethereum = windowEthereum.ethereum;
        if (ethereum === undefined) {
            this._logger.error('Ethereum provider not found');
            return;
        }

        if (ethereum.providers === undefined) {
            const metaMask = ethereum as ExternalProvider;
            if (!metaMask || !metaMask.isMetaMask) {
                this._logger.error('MetaMask not found');
                return;
            }

            ethereum.addListener('chainChanged', this.handleChainChanged);
            ethereum.addListener('accountsChanged', this.handleAccountsChanged);
            setEthereum(metaMask);
            this._isInitialize = true;
        } else {
            for (let i = 0; i < ethereum.providers.length; i++) {
                const provider = ethereum.providers[i] as ExternalProvider;
                if (provider != null && provider.isMetaMask) {
                    ethereum.providers[i].addListener('chainChanged', this.handleChainChanged);
                    ethereum.providers[i].addListener('accountsChanged', this.handleAccountsChanged);

                    // Lưu lại provider
                    setEthereum(provider);
                    this._isInitialize = true;
                    return;
                }
            }
            this._logger.error('MetaMask not found');
            return;
        }
    }

    private async handleChainChanged(){
        // unityBridge.reloadClient();
    };

    private handleAccountsChanged (accounts: string[]) {
        const userAddress = accounts[0];
        Storage.setUserAddress(userAddress);
    };
}

export default EthereumService;