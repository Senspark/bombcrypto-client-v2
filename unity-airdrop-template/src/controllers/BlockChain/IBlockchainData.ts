import {IRpcToken} from "./BlockChainData.ts";

export interface IBlockchainData {
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

    coin_token_abi: JSON;
    hero_token_abi: JSON;
    hero_s_token_abi: JSON;
    hero_extended_abi: JSON;
    hero_design_abi: JSON;
    house_token_abi: JSON;
    house_design_abi: JSON;
    deposit_abi: JSON;
    air_drop_abi: JSON;
    claim_manager_abi: JSON;
    exchange_event_abi: JSON;
    hero_stake_abi: JSON;

    rpcTokens: IRpcToken[];
}