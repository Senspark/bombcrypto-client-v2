using App;

using Constant;

using Scenes.InventoryScene.Scripts;

using Senspark;

using Services;

using Share.Scripts.Dialog;

using UnityEngine;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace BomberLand.Button {
    public class BLInventoryButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        private ISoundManager _soundManager;
        private IStorageManager _storageManager;
        private IFeatureManager _featureManager;
        private IEarlyConfigManager _earlyConfigManager;
        private ILanguageManager _languageManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        }

        public void OnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_earlyConfigManager.IsDisableFeature(FeatureId.Inventory)) {
                DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_feature_maintenance));
                return;
            }
            SceneManager.LoadScene(nameof(InventoryScene));
        }
    }
}