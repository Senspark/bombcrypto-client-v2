using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.UI;
using Senspark;
using Services.Rewards;

namespace App {
    [Service(nameof(ILaunchPadManager))]
    public interface ILaunchPadManager : IService {
        UniTask SyncRemoteData();
        bool CanShowInLaunchPad(IRewardType type, DataType symbol);
        bool CanShowInLaunchPad(ITokenReward type);
        bool CanClaim(IRewardType type, DataType symbol, float rewardValue);
        (float, string) GetClaimFee(IRewardType type, DataType symbol);

        TokenData GetData(IRewardType type, DataType symbol);
        TokenData GetData(ITokenReward type);

        /// <summary>
        /// Sẽ trả về danh sách Tokens cho tất cả các Network
        /// </summary>
        /// <returns></returns>
        List<TokenData> GetForceDisplayTokens();
        List<TokenData> GetForceDisplayTokensTelegram();
        List<TokenData> GetForceDisplayTokensSolana();
        List<TokenData> GetForceDisplayTokensRonin();
        List<TokenData> GetForceDisplayTokensBase();
        List<TokenData> GetForceDisplayTokensViction();
        
        IRewardType CreateRewardType(BlockRewardType type);
    }
}