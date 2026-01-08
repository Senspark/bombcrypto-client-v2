import Logger from "./Logger.ts";
import {BrowserProvider, TransactionRequest} from 'ethers'
import {ChainId, NetworkRpc} from "./RpcNetworkUtils.ts";

const TAG = '[WS]';

export async function getWalletAddress(logger: Logger, provider: BrowserProvider): Promise<string | null> {
    try {
        const accounts = await provider.send('eth_accounts', []);
        const address = accounts[0];
        return address ? address.toLowerCase() : null;
    } catch (error) {
        logger.errors('Error getting wallet address:', error);
        return null;
    }
}

export async function getBalance(logger: Logger, provider: BrowserProvider): Promise<bigint | null> {
    try {
        const address = await getWalletAddress(logger, provider);
        if (!address) {
            logger.error(`${TAG} No wallet address found`);
            return null;
        }
        const balance = await provider.getBalance(address);
        if (balance == undefined) {
            logger.error(`${TAG} Failed to get balance for address: ${address}`);
            return null;
        }
        return balance;
    } catch (error) {
        logger.errors('Error getting balance:', error);
        return null;
    }
}

export async function getChainId(logger: Logger, provider: BrowserProvider): Promise<ChainId | null> {
    try {
        const network = await provider.getNetwork();
        const hex = network.chainId.toString(16);
        const dec = Number(network.chainId);
        return {
            dec: dec,
            hex: `0x${hex}`
        };
    } catch (error) {
        logger.errors('Error getting chain ID:', error);
        return null;
    }
}

export async function forceSwapChain(
    logger: Logger,
    provider: BrowserProvider,
    chainId: ChainId,
): Promise<boolean> {
    try {
        await provider.send('wallet_switchEthereumChain', [{chainId: chainId.hex}]);
        return true;
    } catch (error) {
        logger.errors('Error switching chain:', error);
        return false;
    }
}

export async function addNetwork(
    logger: Logger,
    provider: BrowserProvider,
    rpc: NetworkRpc,
) {
    try {
        await provider.send('wallet_addEthereumChain', [{
            chainId: rpc.chainIdHex,
            rpcUrls: [rpc.rpcUrl],
            chainName: rpc.chainName,
            nativeCurrency: {
                name: rpc.currencySymbol,
                symbol: rpc.currencySymbol,
                decimals: rpc.decimals
            },
            blockExplorerUrls: [rpc.blockExplorerUrl]
        }]);
        logger.log(`${TAG} Network added successfully`);
        return true;
    } catch (error) {
        logger.errors('Error adding network:', error);
        return false;
    }
}

export async function estimateGas(
    logger: Logger,
    provider: BrowserProvider,
    toAddress: string,
    amount: bigint,
    data: string | undefined
): Promise<{ gasLimit: bigint, gasPrice: bigint } | null> {
    try {
        const gasEstimate = await provider.estimateGas({
            to: toAddress,
            value: amount,
            data: data
        });
        const gasPrice = await provider.getFeeData();
        logger.log(`${TAG} Estimated gas: ${gasEstimate.toString()}, Gas Price: ${gasPrice.gasPrice?.toString()}`);
        if (!gasPrice.gasPrice) {
            logger.error(`${TAG} Failed to get gas price`);
            return null;
        }
        return {
            gasLimit: gasEstimate,
            gasPrice: gasPrice.gasPrice
        };
    } catch (error) {
        logger.errors('Error estimating gas:', error);
        return null;
    }
}

export async function sendTransaction(
    logger: Logger,
    provider: BrowserProvider,
    transactionRequest: TransactionRequest,
) {
    try {
        const signer = await provider.getSigner();
        const txResponse = await signer.sendTransaction(transactionRequest);
        logger.log(`Transaction sent with hash: ${txResponse.hash}`);
        const receipt = await txResponse.wait();
        if (receipt && receipt.status === 1) {
            logger.log(`Transaction confirmed in block ${receipt.blockNumber}`);
            return {success: true, txHash: txResponse.hash};
        } else {
            const error = 'Transaction failed';
            logger.error(error);
            return {success: false, error};
        }
    } catch (e) {
        const errMsg = (e as Error).message;
        if (errMsg.indexOf('denied') > 0) {
            logger.error(`${TAG} User denied transaction: ${errMsg}`);
            return {success: false, error: 'denied'};
        }
        const errorMessage = `Transfer failed: ${errMsg}`;
        logger.errors(errorMessage, errorMessage);
        return {success: false, error: errorMessage};
    }
}

export async function forceSwapWallet(
    logger: Logger,
    provider: BrowserProvider
): Promise<boolean> {
    try {
        await provider.send('eth_requestAccounts', []);
        return true;
    } catch (error) {
        logger.errors('Error switching wallet/account:', error);
        return false;
    }
}
