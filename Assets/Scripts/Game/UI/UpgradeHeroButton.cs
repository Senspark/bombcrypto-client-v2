using App;

using Senspark;

using UnityEngine;

public class UpgradeHeroButton : MonoBehaviour {
    private void Awake() {
        var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        gameObject.SetActive(featureManager.EnableUpgrade);
    }
}