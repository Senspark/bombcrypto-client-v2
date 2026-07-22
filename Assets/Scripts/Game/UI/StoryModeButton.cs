using App;

using Senspark;

namespace Game.UI {
    public class StoryModeButton : GameModeButton {
        private ObserverHandle _handle;
        private IBHeroManager _bHeroManager;

        protected override void Init() {
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();

            if (!featureManager.EnableStoryMode) {
                SetLock(true, LocalizeKey.ui_unlock_pvp);
                return;
            }

            _handle = new ObserverHandle();
            _handle.AddObserver(serverManager, new ServerObserver() {
                OnSyncHero = OnSyncHero
            });
            OnSyncHero(null);
        }
        private void OnDestroy() {
            _handle?.Dispose();
        }
        
        private void OnSyncHero(ISyncHeroResponse _) {
            var playerCount = _bHeroManager.GetPlayerCount();
            SetLock(playerCount < 15, LocalizeKey.ui_story_require);
        }
    }
}