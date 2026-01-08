using App;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class BLTreasureHuntButton : MonoBehaviour {
        [SerializeField]
        private GameObject lockImg;
        
        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            SetLock(!featureManager.EnableTreasureHunt);
        }
        
        private void SetLock(bool locked) {
            lockImg.SetActive(locked);
        }
        
    }
}