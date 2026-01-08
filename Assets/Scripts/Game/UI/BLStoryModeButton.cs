using App;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class BLStoryModeButton : MonoBehaviour {
        [SerializeField]
        private GameObject lockImg;
        
        private void Awake() {
            if (!AppConfig.IsProduction) {
                SetLock(false);
                return;
            }
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            SetLock(!featureManager.EnableStoryMode);
        }
        
        private void SetLock(bool locked) {
            lockImg.SetActive(locked);
        }
        
    }
}