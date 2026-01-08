using App;
using Scenes.FarmingScene.Scripts;
using Senspark;
using UnityEngine;

namespace Game.UI {
    public class ShopButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        
        [SerializeField]
        private LevelScene levelScene;

        private ISoundManager _soundManager;
        private IOnBoardingManager _onBoardingManager;

        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
            var canShow = featureManager.EnableShopForUserFi || AppConfig.IsSolana();
            gameObject.SetActive(canShow);
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (levelScene != null) {
                levelScene.EnableDialogBackground(true);
            }
            var dialog = await DialogShop.Create();
            dialog.Show(canvasDialog);
            dialog.OnDidHide(() => {
                _onBoardingManager.DispatchEvent(e => e.refreshOnBoarding?.Invoke());
                if (levelScene) {
                    levelScene.ResetButtonEvents();
                    levelScene.EnableDialogBackground(false);
                }
            });
        }
    }
}