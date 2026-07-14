using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace App {
    public interface IPveServerBridge : IServerManagerDelegate {
        Task<IMapDetails> GetMapDetails();
        Task<bool> GetActiveBomber();
        Task<bool> ActiveBomber(HeroId id, int value);
        Task<bool> ActiveBombers(HeroId[] ids, int value);
        Task<bool> ActiveBomberHouse(string genId, int houseId);
        void GoHome(HeroId id);
        void GoWork(HeroId id);
        void GoSleep(HeroId id);
        Task ChangeBomberManStage(HeroId[] id, HeroStage stage);
        Task<IInvestedDetail> StopPvE();
        Task<IStartPveResponse> StartPvE(GameModeType type);
        void StartExplode(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation,
            List<Vector2Int> brokenList);
        /// <summary>
        /// Fire-and-forget: yêu cầu server kiểm tra on-chain stake và đẩy push BHERO_STAKE_PUSH.
        /// Client không chờ response — UI cập nhật khi push tới qua observer.
        /// Editor build pass debug_fake_push=true để server skip HTTP check (Editor không stake
        /// được, gọi blockchain cũng chỉ trả về state cũ).
        /// </summary>
        void RequestFakeStakePush(HeroId id);

        /// <summary>
        /// Production stake/unstake hook. Sau khi tx on-chain success, gọi method này để server
        /// fetch fresh stake từ ap-blockchain bằng txHash → push BHERO_STAKE_PUSH cho client.
        /// Fire-and-forget — không chờ response.
        /// </summary>
        void RefreshHeroStake(HeroId id, string txHash);
    }
}