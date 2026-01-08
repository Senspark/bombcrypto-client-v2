using App;
using Cysharp.Threading.Tasks;
using Scenes.FarmingScene.Scripts;
using Senspark;
using UnityEngine;

namespace Game.UI {
    public class HeroStakeButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        private ISoundManager _soundManager;
        private IFeatureManager _featureManager;
        private IOnBoardingManager _onBoardingManager;

        private void Awake() {
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
            gameObject.SetActive(_featureManager.CanStakeHero);
        }

        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogSelectStaking.Create().ContinueWith(dialog => {
                dialog.Show(canvasDialog);    
                dialog.OnDidHide(() => {
                    _onBoardingManager.DispatchEvent(e => e.refreshOnBoarding?.Invoke());
                });
            });
        }
    }
}