using App;
using Senspark;
using Scenes.FarmingScene.Scripts;
using UnityEngine;

namespace Game.UI {
    public class ManageAllHeroesButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        
        [SerializeField]
        private LevelScene levelScene;
        
        private ISoundManager _soundManager;
        private IOnBoardingManager _onBoardingManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogInventoryCreator.Create();
            dialog.ShowLockHero();
            dialog.Show(canvasDialog);
            dialog.OnDidHide(() => {
                _onBoardingManager.DispatchEvent(e => e.refreshOnBoarding?.Invoke());
                if (levelScene) {
                    levelScene.ResetButtonEvents();
                }
            });
        }
    }
}