using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace App {
    public interface IPveServerBridge : IServerManagerDelegate {
        Task<IMapDetails> GetMapDetails();
        Task<bool> GetActiveBomber();
        Task<bool> ActiveBomber(HeroId id, int value);
        Task<bool> ActiveBomberHouse(string genId, int houseId);
        void GoHome(HeroId id);
        void GoWork(HeroId id);
        void GoSleep(HeroId id);
        Task ChangeBomberManStage(HeroId[] id, HeroStage stage);
        Task<IInvestedDetail> StopPvE();
        Task<IStartPveResponse> StartPvE(GameModeType type);
        void StartExplode(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation,
            List<Vector2Int> brokenList);
        Task<bool> CheckBomberStake(HeroId id);
    }
}