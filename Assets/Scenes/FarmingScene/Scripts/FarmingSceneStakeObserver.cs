using App;

using Engine.Manager;

using Senspark;

using Share.Scripts.Utils;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    /// <summary>
    /// Lắng nghe push BHERO_STAKE_PUSH ở FarmingScene để hiển thị
    /// <see cref="DialogStakingResult"/> và bắn <see cref="StakeEvent.AfterStake"/>
    /// cho các UI subscribe (shield, fusion, …).
    ///
    /// Data hero đã được <c>ForceUpdateHero</c> ở bridge trước khi event này bắn,
    /// nên các scene khác chỉ cần đọc <c>PlayerStorage</c> là thấy state mới.
    /// </summary>
    public class FarmingSceneStakeObserver : MonoBehaviour {
        [SerializeField]
        private Canvas dialogCanvas;

        private ObserverHandle _handle;
        private IBHeroManager _bHeroManager;

        private void Awake() {
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(serverManager, new ServerObserver {
                OnHeroStakePush = OnHeroStakePush,
            });
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void OnHeroStakePush(IHeroDetails hero) {
            var heroId = new HeroId(hero.Id, hero.AccountType);
            var player = _bHeroManager.GetPlayerDataFromId(heroId);
            if (player == null) {
                return;
            }

            // Bắn event cho các UI listener cũ (ví dụ UpgradeShieldPolygon) refresh state.
            EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);

            ShowResult(player);
        }

        private async void ShowResult(PlayerData player) {
            var heroData = new HeroDataSuccess {
                PlayerData = player,
                Level = player.level.ToString(),
                HeroId = player.heroId.Id.ToString(),
                CurrentShield = player.Shield != null ? player.Shield.CurrentAmount.ToString() : "0",
                TotalShield = player.Shield != null ? player.Shield.TotalAmount.ToString() : "0",
            };

            var dialog = await DialogStakingResult.Create();
            dialog.Show(true, heroData, dialogCanvas);
        }
    }
}
