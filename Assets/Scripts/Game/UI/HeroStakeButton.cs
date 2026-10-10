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
            // Bỏ qua DialogSelectStaking (chỉ còn 1 lựa chọn khả dụng),
            // mở thẳng danh sách hero để stake.
            var levelScene = LevelScene.Instance;
            levelScene?.PauseStatus.SetValue(this, true);
            DialogLegacyHeroes.Create().ContinueWith(dialog => {
                // WillHide để cũng chạy khi dialog bị HideImmediately.
                dialog.OnWillHide(() => levelScene?.PauseStatus.SetValue(this, false));
                dialog.OnDidHide(() => {
                    _onBoardingManager.DispatchEvent(e => e.refreshOnBoarding?.Invoke());
                });
                dialog.Show(canvasDialog);
            });
        }
    }
}
