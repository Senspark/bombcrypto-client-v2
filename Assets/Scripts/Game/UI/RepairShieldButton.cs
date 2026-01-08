using App;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class RepairShieldButton : MonoBehaviour {
        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            gameObject.SetActive(featureManager.EnableRepairShield);
        }
    }
}