using System.Collections.Generic;
using System.Threading.Tasks;
using Senspark;
using Services.Rewards;

namespace App {
    [Service(nameof(ILaunchPadManager))]
    public interface ILaunchPadManager : IService {
        Task SyncRemoteData();
        bool CanShowInLaunchPad(IRewardType type, NetworkSymbol symbol);
        bool CanShowInLaunchPad(ITokenReward type);
        bool CanClaim(IRewardType type, NetworkSymbol symbol, float rewardValue);
        (float, string) GetClaimFee(IRewardType type, NetworkSymbol symbol);

        TokenData GetData(IRewardType type, NetworkSymbol symbol);
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
        
        IRewardType CreateRewardType(string tokenType);
    }
}