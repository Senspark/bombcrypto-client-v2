using System;

using Scenes.TreasureModeScene.Scripts.Solana.Server_Response;

using Senspark;

using Services.Rewards;

using Sfs2X.Entities.Data;

namespace App {
    [Service(nameof(IChestRewardManager))]
    public interface IChestRewardManager : IService, IObserverManager<ChestRewardManagerObserver> {
        void InitNewChestReward(IChestReward rewards);

        /// <summary>
        /// Lấy Chest reward của Network mong muốn
        /// </summary>
        float GetChestRewardByNetwork(BlockRewardType type, NetworkType network);

        /// <summary>
        /// Lấy Chest reward của Network mong muốn
        /// </summary>
        float GetChestRewardByNetwork(IRewardType type, NetworkSymbol network);

        /// <summary>
        /// Lấy Chest reward của Network đang sử dụng
        /// </summary>
        float GetChestReward(BlockRewardType type);
        
        /// <summary>
        /// Lấy Chest reward của Network được truyền vào
        /// </summary>
        float GetChestReward(BlockRewardType type, string dataType);

        /// <summary>
        /// Lấy Chest reward của Network đang sử dụng
        /// </summary>
        float GetChestReward(IRewardType type);

        /// <summary>
        /// Lấy Chest reward của Network đang sử dụng
        /// </summary>
        float GetChestReward(string type);

        float GetBcoinRewardAndDeposit();
        float GetSenRewardAndDeposit();
        float GetBcoinRewardAndDeposit(string network);
        float GetSenRewardAndDeposit(string network);
        float GetRock();

        void SetChestReward(IRewardType type, float value);
        void SetChestReward(BlockRewardType type, float value);
        void SetChestReward(string type, float value);

        float AdjustChestReward(IRewardType type, float addValue);
        float AdjustChestReward(BlockRewardType type, float addValue);
        float AdjustChestReward(string type, float addValue);

        /// <summary>
        /// Lấy Pending Reward của Network mong muốn
        /// </summary>
        float GetClaimPendingRewardByNetwork(BlockRewardType type, NetworkType network);
        
        /// <summary>
        /// Lấy Pending Reward của Network mong muốn
        /// </summary>
        float GetClaimPendingRewardByNetwork(IRewardType type, NetworkSymbol network);

        /// <summary>
        /// Lấy Pending Reward của Network đang sử dụng
        /// </summary>
        float GetClaimPendingReward(IRewardType type);

        /// <summary>
        /// Lấy Pending Reward của Network đang sử dụng
        /// </summary>
        float GetClaimPendingReward(BlockRewardType type);

        /// <summary>
        /// Lấy Pending Reward của Network đang sử dụng
        /// </summary>
        float GetClaimPendingReward(string type);

        IChestReward ParseChestReward(ISFSObject data);
    }

    public class ChestRewardManagerObserver {
        public Action<BlockRewardType, double> OnRewardChanged;
        public Action<BlockRewardType, double, string> OnSameNetworkRewardChanged;
    }
}