using App;
using Game.Manager;
using Senspark;
using UnityEngine;

namespace Game.UI {
    public class FusionButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
    
        private ISoundManager _soundManager;
        private IFusionManager _fusionManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _fusionManager = ServiceLocator.Instance.Resolve<IFusionManager>();
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            gameObject.SetActive(featureManager.EnableFusion);
        }
    
        public async void OnFusionBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await _fusionManager.CreateDialog();
            dialog.Show(canvasDialog);
        }
    }
}