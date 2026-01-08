using System.Linq;
using App;
using Senspark;
using Engine.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.UI {
    public class AutoGoHome : MonoBehaviour {
        [SerializeField]
        private LevelScene levelScene;

        [SerializeField]
        private float autoHomeSeconds = 60;

        private IPveHeroStateManager _pveHeroStateManager;
        private IServerManager _serverManager;
        private ILogManager _logManager;
        private IStorageManager _storeManager;
        private IPlayerStorageManager _playerStore;
        private IHouseStorageManager _houseStorageManager;
        private ObserverHandle _handle;
        
        private Timer _timer;

        private void Awake() {
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_storeManager, new StoreManagerObserver {
                OnAutoMineChanged = OnAutoMineChanged,
            });
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnHeroChangeState = OnHeroChangeState
            });
            InitAutoHome();
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void Update() {
            if (levelScene.PauseStatus.IsPausing) {
                return;
            }
            _timer?.Update(Time.deltaTime);
        }
        
        private void InitAutoHome() {
            _timer = null;
            var enable = _storeManager.EnableAutoMine;
            if (enable) {
                _timer = new Timer(autoHomeSeconds, AutoHome, true);
            }
            AutoHome();
        }
        
        private void AutoHome() {
            if (!_storeManager.EnableAutoMine || levelScene.PauseStatus.IsPausing) {
                return;
            }
            _logManager.Log();
            
            // Cách hoạt động mới:
            /*
             * Mỗi 60 giây: Duyệt từng con hero đang sleep
             * - Nếu chưa full slot nhà thì cho vào nhà
             * - Nếu đã full slot thì kiểm tra có con nào yếu hơn con đang duyệt, nếu có: swap 2 con
             *
             * Khi 1 hero go sleep thì sẽ ngay lập tức kiểm tra nó có vào nhà được ko:
             * - Nếu chưa full slot thì cho vào nhà
             * - Nếu đã full slot thì kiểm tra có con nào yếu hơn con đang duyệt, nếu có: swap 2 con
             */
            
            var activePlayers = _playerStore.GetActivePlayerData();
            var sleeping =
                from p in activePlayers
                where p.stage == HeroStage.Sleep && _playerStore.SimulatePlayerHpOverTime(p, _houseStorageManager.GetHouseChargeFromId(p.heroId.Id)) <=
                    p.maxHp * 0.7f
                orderby p.rare descending
                select p;
            if (sleeping != null && !sleeping.Any()) {
                return;
            }
            
            var totalSlot = _houseStorageManager.GetHouseSlot();
            if (totalSlot == 0) {
                return;
            }
            var slotLeft = totalSlot - _playerStore.GetHomePlayerCount();
            foreach (var player in sleeping) {
                if (slotLeft > 0) {
                    _pveHeroStateManager.ChangeHeroState(player.heroId, HeroStage.Home);
                    slotLeft--;
                } else {
                    var weaker = FindWeakerHeroInHomeThanThis(player.heroId);
                    if (weaker != null) {
                        _pveHeroStateManager.ChangeHeroState(weaker.heroId, HeroStage.Sleep);
                        return;
                    }
                }
            }
        }

        [CanBeNull]
        private PlayerData FindWeakerHeroInHomeThanThis(HeroId heroId) {
            var mainHero = _playerStore.GetPlayerDataFromId(heroId);
            var inHome = _playerStore.GetInHomePlayers().OrderBy(e => e.rare);
            var weakerHero = inHome.FirstOrDefault(e => e.hp >= e.maxHp || e.rare < mainHero.rare);
            return weakerHero;
        }

        private void OnAutoHomeChanged() {
            var number = _playerStore.GetActivePlayersAmount();
            if (number == 0) {
                return;
            }
            InitAutoHome();
        }
        
        private void OnAutoMineChanged(bool _) {
            OnAutoHomeChanged();
        }
        
        private void OnHeroChangeState(IPveHeroDangerous data) {
            // Nếu Data có sự thay đổi thì Process
            AutoHome();
        }
    }
}
