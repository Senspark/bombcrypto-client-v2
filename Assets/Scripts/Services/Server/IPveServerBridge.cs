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

        // Server-driven treasure mode: one call starts (or fully resyncs) the server's game;
        // everything after arrives as TREASURE_EVENTS (ServerObserver.OnTreasureEvents).
        // paused: the client is paused, so a resync keeps the server's heroes still.
        // autoMine: the server sends a hero with no energy left home (when a house has room) instead of to sleep.
        Task<TreasureSnapshot> StartTreasureMode(bool paused = false, bool autoMine = false);
        Task StopTreasureMode();

        // Heroes halt until resumed; bombs already planted still explode.
        Task PauseTreasureMode(bool paused);

        // The auto mine switch changed while playing.
        Task SetTreasureAutoMine(bool enabled);

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