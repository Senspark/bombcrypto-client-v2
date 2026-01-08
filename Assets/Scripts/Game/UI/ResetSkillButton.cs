using App;

using Senspark;

using UnityEngine;

public class ResetSkillButton : MonoBehaviour {
    private void Awake() {
        var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        gameObject.SetActive(featureManager.EnableResetSkill);
    }
}