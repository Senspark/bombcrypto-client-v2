using System;

using Game.UI;

using Senspark;

namespace App {
    [Service(nameof(IChestRewardManager))]
    public interface IChestRewardManager : IService, IObserverManager<ChestRewardManagerObserver> {
        /// <summary>
        /// Set DataType (network / user kind) hiện tại của account — nguồn chuẩn là data_type từ login response.
        /// Mặc định TR cho tới khi được set.
        /// </summary>
        void SetCurrentNetwork(DataType dataType);

        void InitNewChestReward(IChestReward rewards);

        /// <summary>
        /// Lấy Chest reward của Network mong muốn
        /// </summary>
        float GetChestRewardByNetwork(IRewardType type, DataType network);

        /// <summary>
        /// Lấy Chest reward của Network đang sử dụng
        /// </summary>
        float GetChestReward(BlockRewardType type);

        /// <summary>
        /// Lấy Chest reward của Network được truyền vào
        /// </summary>
        float GetChestReward(BlockRewardType type, DataType dataType);

        /// <summary>
        /// Lấy Chest reward của Network đang sử dụng
        /// </summary>
        float GetChestReward(IRewardType type);

        float GetBcoinRewardAndDeposit();
        float GetSenRewardAndDeposit();
        float GetBcoinRewardAndDeposit(DataType network);
        float GetSenRewardAndDeposit(DataType network);
        float GetRock();

        /// <summary>
        /// Số dư bridge (scope BP, chain-agnostic). Hook cho spending phase sau.
        /// </summary>
        float GetBcoinBridge();
        float GetSenBridge();

        void SetChestReward(BlockRewardType type, float value);

        float AdjustChestReward(IRewardType type, float addValue);
        float AdjustChestReward(BlockRewardType type, float addValue);

        /// <summary>
        /// Lấy Pending Reward của Network mong muốn
        /// </summary>
        float GetClaimPendingRewardByNetwork(IRewardType type, DataType network);
    }

    public class ChestRewardManagerObserver {
        /// <summary>
        /// Bắn mỗi khi balance của (token, scope) thay đổi. scope = DataType của entry (TR/BSC/POLYGON/…).
        /// </summary>
        public Action<BlockRewardType, DataType, double> OnRewardBalanceChanged;
    }
}