using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingReward : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI compeletedTxt;
    
    [SerializeField]
    private Image rewardIcon;
    
    [SerializeField]
    private TextMeshProUGUI rewardQuantity;
    
    [SerializeField]
    private UnityEngine.Animation onBoardingCompleted;
    
    [SerializeField]
    private GameObject[] onBoardingCompletedObjs;

    public void InitReward(string text, float quantity) {
        foreach (var obj in onBoardingCompletedObjs) {
            obj.SetActive(false);
        }
        compeletedTxt.SetText($"completed: {text}");
        rewardQuantity.SetText($"x{quantity}");
        this.gameObject.SetActive(true);
        onBoardingCompleted.Play();
    }
}
