using System.Collections.Generic;

using Sfs2X.Entities.Data;

namespace App {
    public class TokenPrice : ITokenPrice {
        public int RewardType { get; }
        public double Price { get; }

        public TokenPrice(int rewardType, double price) {
            RewardType = rewardType;
            Price = price;
        }

        public TokenPrice(ISFSObject data) {
            RewardType = data.GetInt("reward_type");
            Price = data.GetDouble("price");
        }
    }

    public static class TokenPrices {
        public static List<ITokenPrice> Parse(ISFSObject data, string key = "prices") {
            var result = new List<ITokenPrice>();
            if (!data.ContainsKey(key)) {
                return result;
            }
            var arr = data.GetSFSArray(key);
            if (arr == null) {
                return result;
            }
            for (var i = 0; i < arr.Size(); i++) {
                result.Add(new TokenPrice(arr.GetSFSObject(i)));
            }
            return result;
        }

        public static ITokenPrice Find(this List<ITokenPrice> prices, int wireRewardType) {
            if (prices == null) {
                return null;
            }
            foreach (var price in prices) {
                if (price.RewardType == wireRewardType) {
                    return price;
                }
            }
            return null;
        }

        public static ITokenPrice FindNative(this List<ITokenPrice> prices) {
            if (prices == null) {
                return null;
            }
            foreach (var price in prices) {
                if (ServerRewardTypes.IsNative(price.RewardType)) {
                    return price;
                }
            }
            return null;
        }

        // The list currency is whatever a chain prices its items in: BCOIN on BSC / POLYGON, the chain's
        // own deposited token on an airdrop chain. Picking "the entry that is not native" keeps callers
        // from branching per network.
        public static ITokenPrice FindListed(this List<ITokenPrice> prices) {
            if (prices == null) {
                return null;
            }
            foreach (var price in prices) {
                if (!ServerRewardTypes.IsNative(price.RewardType)) {
                    return price;
                }
            }
            return null;
        }
    }
}
