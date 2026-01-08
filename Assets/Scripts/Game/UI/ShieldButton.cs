using App;
using Cysharp.Threading.Tasks;
using Senspark;
using UnityEngine;

namespace Game.UI {
    public class ShieldButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        private IRepairShieldManager _repairShieldManager;
        private ISoundManager _soundManager;
        private IOnBoardingManager _onBoardingManager;

        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
            _repairShieldManager = ServiceLocator.Instance.Resolve<IRepairShieldManager>();
            gameObject.SetActive(featureManager.EnableRepairShield);
        }

        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _repairShieldManager.CreateDialog().ContinueWith(dialog => {
                dialog.Init(default);
                dialog.Show(canvasDialog);
                dialog.OnDidHide(() => {
                    _onBoardingManager.DispatchEvent(e => e.refreshOnBoarding?.Invoke());
                });
            });
        }
    }
}