using System.Linq;
using App;
using Senspark;
using Engine.Utils;
using UnityEngine;

namespace Game.UI {
    public class AutoGoWork : MonoBehaviour {
        [SerializeField]
        private LevelScene levelScene;
        
        [SerializeField]
        private float autoWorkSeconds = 60 * 10;

        private IPveHeroStateManager _pveHeroStateManager;
        private ILogManager _logManager;
        private IStorageManager _storeManager;
        private IBHeroManager _playerStore;
        private IHouseStorageManager _houseStorageManager;
        private ObserverHandle _handle;
        
        private Timer _timer;
        private bool _releasingRent;
        private float _nextRentCheckTime;

        private void Awake() {
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_storeManager, new StoreManagerObserver() {
                OnAutoMineChanged = OnAutoMineChanged
            });
            InitAutoWork();
        }
        
        private void OnDestroy() {
            _handle.Dispose();
        }
        
        private void Update() {
            CheckHouseRentExpired();
            if (levelScene.PauseStatus.IsPausing) {
                return;
            }
            _timer?.Update(Time.deltaTime);
        }

        private void InitAutoWork() {
            _timer = null;
            var enableAutoWork = _storeManager.EnableAutoMine;
            if (enableAutoWork) {
                _timer = new Timer(autoWorkSeconds, AutoWork, true);
            }
            AutoWork();
        }

        private void AutoWork() {
            if (!_storeManager.EnableAutoMine || levelScene.PauseStatus.IsPausing) {
                return;
            }
            _logManager.Log();

            var activePlayers = _playerStore.GetActivePlayerData();
            var notWorking = 
                    from p in activePlayers
                    where p.stage != HeroStage.Working
                    select p;
            
            var needShield = levelScene.Mode == GameModeType.TreasureHuntV2;
            if (needShield && !(AppConfig.IsAirDrop())) {
                notWorking =
                    from p in notWorking
                    where !p.IsHeroS || p.Shield == null || p.Shield.CurrentAmount > 0
                    select p;
            }
            notWorking =
                from p in notWorking
                where _playerStore.SimulatePlayerHpOverTime(p, _houseStorageManager.GetHouseChargeFromId(p.heroId.Id)) >
                      p.maxHp * 0.7f
                select p;
            var goWork = notWorking.Select(e => e.heroId).ToArray();
            if (goWork.Length == 0) {
                return;
            }
            _pveHeroStateManager.ChangeHeroState(goWork, HeroStage.Working);
        }

        // Rent expired: the server puts the heroes to sleep, so fetch their state instead of deciding here
        private async void CheckHouseRentExpired() {
            if (_releasingRent || Time.realtimeSinceStartup < _nextRentCheckTime) {
                return;
            }
            var heroIds = _houseStorageManager.CheckHouseRentExpired();
            if (heroIds.Count == 0) {
                return;
            }
            _releasingRent = true;
            await levelScene.RefreshHeroesFromServer();
            _releasingRent = false;
            if (!this) {
                return;
            }
            var heroes = _playerStore.GetActivePlayerData();
            foreach (var id in heroIds) {
                var hero = heroes.FirstOrDefault(p => p.heroId.Id == id);
                if (hero == null || hero.stage != HeroStage.Home) {
                    _houseStorageManager.HeroLeaveHouse(id);
                }
            }
            // client clock ahead of the server: the server still keeps them home, retry shortly
            _nextRentCheckTime = Time.realtimeSinceStartup + 3;
        }

        private void OnAutoMineChanged(bool _) {
            InitAutoWork();
        }
    }
}