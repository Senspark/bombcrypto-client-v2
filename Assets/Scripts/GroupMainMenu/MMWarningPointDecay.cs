using System;
using System.Collections;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.MainMenuScene.Scripts;

using Senspark;
using Services;
using TMPro;
using UnityEngine;

public class MMWarningPointDecay : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI amountMatchesText;
    
    [SerializeField]
    private TextMeshProUGUI remainingText;
    
    private Canvas _canvasDialog;

    private void Awake() {
        var manager = ServiceLocator.Instance.Resolve<IPvPBombRankManager>();
        var remainingMatches = Mathf.Max(manager.GetMinMatchesConfig() - manager.GetAmountMatches(), 0);
        amountMatchesText.SetText($"Play <color=#F9CD00>{remainingMatches}</color> more 1v1 matches to avoid BR Point decay.");
        var localNow = DateTime.UtcNow.ToLocalTime();
        var targetTime = localNow.Date.AddDays(1);
        var remainingTime = targetTime - DateTime.UtcNow.ToLocalTime();
        StartCoroutine(CountdownToTargetTime(remainingTime));
    }
    
    private IEnumerator CountdownToTargetTime(TimeSpan remainingTime) {
        while (remainingTime.TotalSeconds >= 0) {
            remainingText.SetText($"Remaining: <color=#F9CD00>{remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s</color>");
            yield return new WaitForSeconds(1);
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
        }
        Destroy(this.gameObject);
    }
    
    public void SetCanvasDialog(Canvas canvas) {
        _canvasDialog = canvas;
    }
    
    public void OnButtonInfoClicked() {
        DialogPointDecay.Create().ContinueWith(dialog => {
            dialog.Show(_canvasDialog);
        }); 
    }
}
