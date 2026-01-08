using System;

using App;

using Senspark;

namespace Utils {
    public static class GemUtils {
        public static (float gemLock, float gemUnlock) GetGemSpending(float price) {
            var chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            return GetGemSpending(
                chestRewardManager.GetChestReward(BlockRewardType.Gem),
                chestRewardManager.GetChestReward(BlockRewardType.LockedGem),
                price
            );
        }

        public static (float gemLock, float gemUnlock) GetGemSpending(float gemLock, float gemUnlock, float price) {
            float gemLockSpending;
            float gemUnlockSpending;
            if (price <= gemLock) {
                gemLockSpending = price;
                gemUnlockSpending = 0;
            } else {
                gemLockSpending = gemLock;
                gemUnlockSpending = price - gemLock;
            }
            return (gemLockSpending, gemUnlockSpending);
        }

        public static (int gemLock, int gemUnlock) GetGemSpending(int gemLock, int gemUnlock, int price) {
            int gemLockSpending;
            int gemUnlockSpending;
            if (price <= gemLock) {
                gemLockSpending = price;
                gemUnlockSpending = 0;
            } else {
                gemLockSpending = gemLock;
                gemUnlockSpending = price - gemLock;
            }
            return (gemLockSpending, gemUnlockSpending);
        }
    }
}