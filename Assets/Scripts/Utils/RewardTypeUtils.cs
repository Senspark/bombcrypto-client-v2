using System;
using System.Collections.Generic;

using Constant;

namespace Utils {
    public static class RewardTypeUtils {
        private static readonly Dictionary<int, string> Names = new() {
            [(int) RewardType.Senspark] = "SENSPARK", 
            [(int) RewardType.BCOIN] = "BCOIN"
        };

        public static string GetName(this RewardType rewardType) {
            return GetName((int) rewardType);
        }

        public static string GetName(int rewardType) {
            return Names.TryGetValue(rewardType, out var value)
                ? value
                : throw new Exception($"Could not find reward type: {rewardType}");
        }
    }
}