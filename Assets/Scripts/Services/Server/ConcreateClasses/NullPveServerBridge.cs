using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using UnityEngine;

namespace Services.Server.ConcreateClasses {
    public class NullPveServerBridge : IPveServerBridge {
        public Task<IMapDetails> GetMapDetails() {
            throw new System.NotImplementedException();
        }
        
        public Task<bool> GetActiveBomber() {
            throw new System.NotImplementedException();
        }
        
        public Task<bool> ActiveBomber(HeroId id, int value) {
            throw new System.NotImplementedException();
        }
        
        public Task<bool> ActiveBomberHouse(string genId, int houseId) {
            throw new System.NotImplementedException();
        }
        
        public void GoHome(HeroId id) {
            throw new System.NotImplementedException();
        }
        
        public void GoWork(HeroId id) {
            throw new System.NotImplementedException();
        }
        
        public void GoSleep(HeroId id) {
            throw new System.NotImplementedException();
        }
        
        public Task ChangeBomberManStage(HeroId[] id, HeroStage stage) {
            throw new System.NotImplementedException();
        }
        
        public Task<IInvestedDetail> StopPvE() {
            throw new System.NotImplementedException();
        }
        
        public Task<IStartPveResponse> StartPvE(GameModeType type) {
            throw new System.NotImplementedException();
        }
        
        public void StartExplode(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation, List<Vector2Int> brokenList) {
            throw new System.NotImplementedException();
        }
        
        public Task<bool> CheckBomberStake(HeroId id) {
            throw new System.NotImplementedException();
        }
    }
}