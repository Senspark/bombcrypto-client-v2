import {BrowserProvider} from "ethers";
import {Provider} from "@reown/appkit-adapter-ethers";
import CoinToken from "../CoinToken.ts";
import {ExternalProvider} from "@ethersproject/providers";

const tokenData: { [key: string]: CoinToken } = {};
let _browserProvider: BrowserProvider;
let _provider: Provider;
let _userAddress: string;
let _windowEthereum: ExternalProvider | undefined;

function setProvider(provider: Provider): void {
    _provider = provider;
}

function getProvider(): Provider {
    return _provider;
}

function setBrowserProvider(provider: BrowserProvider): void {
    _browserProvider = provider;
}

function getBrowserProvider(): BrowserProvider {
    return _browserProvider;
}

function setToken(key: string, data: CoinToken): void {
    tokenData[key] = data;
}
function getToken(key: string): CoinToken {
    return tokenData[key];
}

function setUserAddress(userAddress: string): void {
    _userAddress = userAddress;
}

function getUserAddress(): string {
    return _userAddress;
}

function setEthereum(ethereum: ExternalProvider): void {
    _windowEthereum = ethereum;
}

function getEthereum(): ExternalProvider | undefined {
    return _windowEthereum;
}

export {
    setProvider,
    getProvider,
    setBrowserProvider,
    setToken,
    getToken,
    setUserAddress,
    getBrowserProvider,
    getUserAddress,
    setEthereum,
    getEthereum
}