using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLPvpMode.Manager.Api;
using CustomSmartFox.SolCommands;using Senspark;
using Server.Models;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace App {
    public partial class DefaultPveServerBridge : IPveServerBridge {
        private readonly IPlayerStorageManager _playerStorageManager;
        private readonly IHouseStorageManager _houseStorageManager;
        private readonly IStorageManager _storageManager;
        private readonly IServerDispatcher _serverDispatcher;
        private readonly IUserAccountManager _userAccountManager;

        public DefaultPveServerBridge(
            IPlayerStorageManager playerStorageManager,
            IHouseStorageManager houseStorageManager,
            IStorageManager storageManager,
            IServerDispatcher serverDispatcher
        ) {
            _playerStorageManager = playerStorageManager;
            _houseStorageManager = houseStorageManager;
            _storageManager = storageManager;
            _serverDispatcher = serverDispatcher;
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
        }
        
        public async Task<IMapDetails> GetMapDetails() {
            var data = new SFSObject();
            
            var response = await _serverDispatcher.SendCmd(new CmdGetBlockMap(data));
            return OnGetMapDetails(response);
        }
        
        private IMapDetails OnGetMapDetails(ISFSObject data) {
            var tileSet = data.GetInt("tileset_pve_v2");
            var blockData = data.GetUtfString("datas_pve_v2");
            var result = new MapDetails(tileSet, blockData);
            _playerStorageManager.SetMapDetails(result);
            return result;
        }
        
        public async Task<bool> GetActiveBomber() {
            var data = new SFSObject();
            
            var response = await _serverDispatcher.SendCmd(new CmdGetActiveBomber(data));
            return OnGetActiveHero(response);
        }
        
        private bool OnGetActiveHero(ISFSObject data) {
            var result = HeroDetails.ParseArray(data);
            foreach (var hero in result) {
                var heroId = new HeroId(hero.Id, hero.AccountType);
                _playerStorageManager.UpdatePlayerHpFromServer(hero);
                _playerStorageManager.UpdateHeroSShield(heroId, hero.HeroSAbilities);
                _playerStorageManager.UpdateHeroState(heroId, hero.Stage);
            }
            _serverDispatcher.DispatchEvent(e => e.OnSyncHero?.Invoke(new SyncHeroResponse(result)));
            return true;
        }
        
        public async Task<bool> ActiveBomber(HeroId id, int value) {
            var data = new SFSObject().Apply(it => {
                it.PutLong(SFSDefine.SFSField.Id, id.Id);
                it.PutInt(SFSDefine.SFSField.active, value);
                it.PutInt(SFSDefine.SFSField.AccountType, (int)id.Type);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)id.Type);
            });

            var response = await _serverDispatcher.SendCmd(new CmdActiveBomber(data));
            return OnActiveHero(response);
        }
        
        private bool OnActiveHero(ISFSObject data) {
            var result = HeroDetails.Parse(data);
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.UpdatePlayerHpFromServer(result);
            _playerStorageManager.UpdateHeroSShield(heroId, result.HeroSAbilities);
            _playerStorageManager.UpdateHeroState(heroId, result.Stage);
            _playerStorageManager.UpdateHeroActiveState(heroId, result.IsActive);

            // Trick: Nếu deactive hero thì xem như nó vào Home để biến mất khỏi map
            var stage = result.IsActive ? result.Stage : HeroStage.Home;
            var evData = new PveHeroDangerous(heroId, stage, PveDangerousType.NoDanger);
            _serverDispatcher.DispatchEvent(d => d.OnActiveHero?.Invoke(evData, heroId, result.IsActive));
            return true;
        }
        
        public async Task<bool> ActiveBomberHouse(string genId, int houseId) {
            var data = new SFSObject().Apply(it => {
                it.PutInt(SFSDefine.SFSField.HouseId, houseId);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdActiveHouse(data));
            return OnActiveHouse(response);
        }
        
        private bool OnActiveHouse(ISFSObject data) {
            var houseGenId = data.GetUtfString(SFSDefine.SFSField.HouseGenId);

            _houseStorageManager.SetActiveHouse(houseGenId);
            var bombers = HeroDetails.ParseArray(data);
            foreach (var h in bombers) {
                var heroId = new HeroId(h.Id, h.AccountType);
                _playerStorageManager.UpdatePlayerHpFromServer(h);
                if (h.Stage != HeroStage.Sleep)
                    continue;

                _playerStorageManager.UpdateHeroState(heroId, h.Stage);
                var evData = new PveHeroDangerous(heroId, h.Stage, PveDangerousType.NoDanger);
                _serverDispatcher.DispatchEvent(d => d.OnHeroChangeState?.Invoke(evData));
            }
            return true;
        }
        
        public async void GoHome(HeroId id) {
            var data = new SFSObject().Apply(it => {
                it.PutLong(SFSDefine.SFSField.Id, id.Id);
                it.PutInt(SFSDefine.SFSField.AccountType, (int)id.Type);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)id.Type);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGoHome(data));
            OnGoHomeResponse(response);
        }
        
        private bool OnGoHomeResponse(ISFSObject data) {
            var result = HeroDetails.Parse(data);
            var state = HeroStage.Home;
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.UpdatePlayerHpFromServer(result);
            _playerStorageManager.UpdateHeroState(heroId, state);

            _serverDispatcher.DispatchEvent(observer =>
                observer.OnHeroChangeState?.Invoke(new PveHeroDangerous(heroId, state, PveDangerousType.NoDanger)));
            return true;
        }
        
        public async void GoWork(HeroId id) {
            var data = new SFSObject().Apply(it => {
                it.PutLong(SFSDefine.SFSField.Id, id.Id);
                it.PutInt(SFSDefine.SFSField.AccountType, (int)id.Type);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)id.Type);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGoWork(data));
            OnGoWorkResponse(response);
        }
        
        private bool OnGoWorkResponse(ISFSObject data) {
            var result = HeroDetails.Parse(data);
            var isDangerous = (PveDangerousType)data.GetInt("is_dangerous");
            var state = HeroStage.Working;
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.UpdatePlayerHpFromServer(result);
            _playerStorageManager.UpdateHeroState(heroId, state);

            _serverDispatcher.DispatchEvent(observer =>
                observer.OnHeroChangeState?.Invoke(new PveHeroDangerous(heroId, state, isDangerous)));
            return true;
        }
        
        public async void GoSleep(HeroId id) {
            var data = new SFSObject().Apply(it => {
                it.PutLong(SFSDefine.SFSField.Id, id.Id);
                it.PutInt(SFSDefine.SFSField.AccountType, (int)id.Type);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)id.Type);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGoSleep(data));
            OnGoSleepResponse(response);
        }
        
        private bool OnGoSleepResponse(ISFSObject data) {
            var result = HeroDetails.Parse(data);
            var state = HeroStage.Sleep;
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.UpdatePlayerHpFromServer(result);
            _playerStorageManager.UpdateHeroState(heroId, state);

            _serverDispatcher.DispatchEvent(observer =>
                observer.OnHeroChangeState?.Invoke(new PveHeroDangerous(heroId, state, PveDangerousType.NoDanger)));
            return true;
        }
        public async Task ChangeBomberManStage(HeroId[] ids, HeroStage stage) {
            if (AppConfig.IsTon()) {
                await ChangeBomberManStageForTon(ids, stage);
            } else {
                await ChangeBomberManStageWeb(ids, stage);
            }
        }
        //Dùng cho web và sol
        private async Task ChangeBomberManStageWeb(HeroId[] ids, HeroStage stage) {
            var data = new SFSObject();
            var array = new SFSArray();

            foreach (var id in ids) {
                var d = new SFSObject();
                d.PutLong("id", id.Id);
                d.PutInt("stage", (int) stage);
                d.PutInt(SFSDefine.SFSField.AccountType, (int) id.Type);
                d.PutInt(SFSDefine.SFSField.HeroType, (int) id.Type);
                array.AddSFSObject(d);
            }
            data.PutSFSArray("datas", array);

            var response = await _serverDispatcher.SendCmd(new CmdChangeStage(data));
            OnChangeBomberManStageResponse(response);
        }
        
        private bool OnChangeBomberManStageResponse(ISFSObject data) {
            var array = data.GetSFSArray("datas");
            if(array.Size() == 0) {
                return true;
            }   

            for (var i = 0; i < array.Size(); i++) {
                var d = array.GetSFSObject(i);
                var result = HeroDetails.Parse(d);
                var heroId = new HeroId(result.Id, result.AccountType);
                _playerStorageManager.UpdatePlayerHpFromServer(result);
                _playerStorageManager.UpdateHeroState(heroId, result.Stage);

                var isDangerous = (PveDangerousType)d.GetInt("is_dangerous");
                var isSleep = result.Stage == HeroStage.Sleep || result.Stage == HeroStage.Home;
                var dangerous = isSleep ? PveDangerousType.NoDanger : isDangerous;

                _serverDispatcher.DispatchEvent(observer =>
                    observer.OnHeroChangeState?.Invoke(new PveHeroDangerous(heroId, result.Stage, dangerous)));
            }
            return true;
        }
        
        private async Task ChangeBomberManStageForTon(HeroId[] ids, HeroStage stage) {
            var data = new SFSObject();
            var array = new SFSArray();
            data.PutInt("stage", (int) stage);
            data.PutInt(SFSDefine.SFSField.AccountType, (int) ids[0].Type);
            data.PutInt(SFSDefine.SFSField.HeroType, (int) ids[0].Type);
            foreach (var id in ids) {
                array.AddInt(id.Id);
            }
            data.PutSFSArray("datas", array);

            var response = await _serverDispatcher.SendCmd(new CmdChangeStageV3(data));
            OnChangeBomberManStageResponseTon(response);
        }
        
        private bool OnChangeBomberManStageResponseTon(ISFSObject data) {
            var array = data.GetSFSArray("datas");
            if(array.Size() == 0) {
                return true;
            }   
            var isDangerous = (PveDangerousType)data.GetInt("is_dangerous");
            var heroType = data.GetInt(SFSDefine.SFSField.HeroType);
            var stage = data.GetInt("stage");
            for (var i = 0; i < array.Size(); i++) {
                var d = array.GetSFSObject(i);
                d.PutInt("stage",  stage);
                d.PutInt(SFSDefine.SFSField.HeroType, heroType);
                var result = HeroDetails.Parse(d);
                var heroId = new HeroId(result.Id, result.AccountType);
                _playerStorageManager.UpdatePlayerHpFromServer(result);
                _playerStorageManager.UpdateHeroState(heroId, result.Stage);

                var isSleep = result.Stage == HeroStage.Sleep || result.Stage == HeroStage.Home;
                var dangerous = isSleep ? PveDangerousType.NoDanger : isDangerous;

                _serverDispatcher.DispatchEvent(observer =>
                    observer.OnHeroChangeState?.Invoke(new PveHeroDangerous(heroId, result.Stage, dangerous)));
            }
            return true;
        }
        
        public async Task<IInvestedDetail> StopPvE() {
            var data = new SFSObject().Apply(it => {
                var s = SFSDefine.GetSlogans(EntryPoint.StopPvE);
                it.PutUtfString(SFSDefine.SFSField.SLOGAN, s);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdStopPve(data));
            return OnStopPvE(response);
        }
        
        private IInvestedDetail OnStopPvE(ISFSObject data) {
            var result = ParseInvestedDetail(data);
            return result;
        }
        
        private InvestedDetail ParseInvestedDetail(ISFSObject data) {
            var detail = new InvestedDetail(data);
            _storageManager.InvestedDetail = detail;
            _serverDispatcher.DispatchEvent(observer => observer.OnInvestedDetail?.Invoke(detail));
            return detail;
        }
        
        public async Task<IStartPveResponse> StartPvE(GameModeType type) {
            var data = new SFSObject().Apply(it => {
                var s = SFSDefine.GetSlogans(EntryPoint.StartPvE);
                var mode = type switch {
                    GameModeType.TreasureHunt => 1,
                    GameModeType.TreasureHuntV2 => 3,
                    _ => throw new Exception("Invalid Data")
                };
                it.PutUtfString(SFSDefine.SFSField.SLOGAN, s);
                it.PutInt("mode", mode);
            });

            var response = await _serverDispatcher.SendCmd(new CmdStartPve(data));
            return OnStartPvE(response);
        }
        
        private IStartPveResponse OnStartPvE(ISFSObject data) {
            ParseCurrentMiningToken(data);
            var response = new StartPveResponse(data);
            return response;
        }
        
        private void ParseCurrentMiningToken(ISFSObject data) {
            var token = new MiningTokenData(data);
            if (token.IsValid) {
                _storageManager.MiningTokenType = token.TokenType;
            }
        }
        
        public async void StartExplode(GameModeType type, HeroId heroId, int bombId, Vector2Int tileLocation,
            List<Vector2Int> brokenList) {
            var hero = _playerStorageManager.GetPlayerDataFromId(heroId);
            if (hero == null) {
                return;
            }
            
            ISFSArray blocks = new SFSArray();
            foreach (var t in brokenList) {
                ISFSObject block = new SFSObject();
                block.PutInt("i", t.x);
                block.PutInt("j", t.y);
                blocks.AddSFSObject(block);
            }

            var data = new SFSObject().Apply(it => {
                var accountType = hero.AccountType;
                it.PutLong("id", heroId.Id);
                it.PutInt("num", bombId);
                it.PutInt("i", tileLocation.x);
                it.PutInt("j", tileLocation.y);
                it.PutInt("heroId", heroId.Id);
                it.PutSFSArray("blocks", blocks);
                it.PutInt(SFSDefine.SFSField.AccountType, (int)accountType);
                it.PutInt(SFSDefine.SFSField.HeroType, (int)accountType);
            });

            var response = await _serverDispatcher.SendCmd(new CmdStartExplode(data));
            OnStartExplode(response);
        }
        
        private bool OnStartExplode(ISFSObject data) {
            var result = new PveExplodeResponse(data);
            _serverDispatcher.DispatchEvent(e => e.OnPveExploded?.Invoke(result));
            return true;
        }
        
        public async Task<bool> CheckBomberStake(HeroId id) {
            var data = new SFSObject().Apply(it => {
                it.PutLong(SFSDefine.SFSField.Id, id.Id);
            });
            
            var response = await _serverDispatcher.SendCmd(new CmdCheckBomberStake(data));
            return OnStakeBomber(response);
        }
        
        private bool OnStakeBomber(ISFSObject data) {
            var result = HeroDetails.Parse(data);
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.ForceUpdateHero(heroId, result);
            return true;
        }
    }
}