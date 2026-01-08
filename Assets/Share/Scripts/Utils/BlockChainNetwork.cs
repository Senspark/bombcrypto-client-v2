using App;

namespace Share.Scripts.Utils {
    public static class BlockChainNetwork {
        
        public static NetworkType GetNetworkType(string chainId) {
            //DevHoang: Add new airdrop
            switch (chainId) {
                case "56":
                    return NetworkType.Binance;
                case "137":
                    return NetworkType.Polygon;
                case "2020":
                    return NetworkType.Ronin;
                case "8453":
                    return NetworkType.Base;
                case "88":
                    return NetworkType.Viction;
                case "97":
                    return NetworkType.Binance; // bsc testnet
                case "80002":
                    return NetworkType.Polygon; // polygon testnet
                case "2021":
                    return NetworkType.Ronin; // Ronin testnet
                case "84532":
                    return NetworkType.Base; // Base testnet
                case "89":
                    return NetworkType.Viction; // Viction testnet
                default:
                    return NetworkType.Binance;
            }
        }
    }
}