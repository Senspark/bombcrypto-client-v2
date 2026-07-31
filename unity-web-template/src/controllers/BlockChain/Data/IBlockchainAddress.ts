export interface IBlockchainAddress {
    CoinTokenAddress: string;
    SensparkTokenAddress: string;
    UsdtTokenAddress: string;
    HeroTokenAddress: string;
    HeroSTokenAddress: string;
    HeroStakeAddress: string;
    HeroExtendedAddress: string;
    HouseTokenAddress: string;
    DepositAddress: string;
    DepositBridgeAddress: string;
    // Native (BNB / POL) vault. Only BSC / Polygon deploy one; other chains omit it.
    DepositNativeAddress?: string;
    AirDropAddress: string;
    ClaimManagerAddress: string;
    CoinExchangeAddress: string;
    BirthdayEventAddress: string;
}