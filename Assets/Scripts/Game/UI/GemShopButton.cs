using App;
using Scenes.FarmingScene.Scripts;
using Senspark;
using UnityEngine;

namespace Game.UI {
    public class GemShopButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            gameObject.SetActive(featureManager.EnableBuyGemByNativeToken);
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogGemShop.Create();
            dialog.Show(canvasDialog);
        }
    }
}
