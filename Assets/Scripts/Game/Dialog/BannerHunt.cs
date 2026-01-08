using System;

using App;

using Scenes.FarmingScene.Scripts;

using Senspark;

using UnityEngine;

namespace Game.Dialog {
    public class BannerHunt : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        private IPlayerStorageManager _playerStoreManager;
        private ISoundManager _soundManager;
        private ObserverHandle _handle;
        private IServerManager _serverManager;

        private void Awake() {
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnSyncHero = OnSyncHero,
            });
        }

        private void Start() {
            ActiveUI();
        }

        private void OnSyncHero(ISyncHeroResponse _) {
            ActiveUI();
        }

        private void ActiveUI() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            if (!featureManager.EnableBannerHunt) {
                gameObject.SetActive(false);
                return;
            }
            var players = _playerStoreManager.GetPlayerCount();
            gameObject.SetActive(players == 0);
        }

        public async void OnBuyHeroBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogShop.Create();
            dialog.Show(canvasDialog);
        }

        private void OnDestroy() {
            _handle.Dispose();
        }
    }
}