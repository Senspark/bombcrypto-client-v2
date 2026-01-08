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
        private IPlayerStorageManager _playerStore;
        private IHouseStorageManager _houseStorageManager;
        private ObserverHandle _handle;
        
        private Timer _timer;

        private void Awake() {
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
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
            CheckHouseRentExpired();
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

        private void CheckHouseRentExpired() {
            // nếu house rent hết hạn thì go work cho tất cả hero ở nhà đó
            var heroesInHouseExpired = _houseStorageManager.CheckHouseRentExpired();
            if (heroesInHouseExpired.Count != 0) {
                var allHeroes = _playerStore.GetActivePlayerData();
                var filterHeroes = from p in allHeroes
                    where heroesInHouseExpired.Contains(p.heroId.Id)
                    select p;
                var heroesGoWork = filterHeroes.Select(e => e.heroId).ToArray();
                _pveHeroStateManager.ChangeHeroState(heroesGoWork, HeroStage.Working);
            }
        }

        private void OnAutoMineChanged(bool _) {
            InitAutoWork();
        }
    }
}