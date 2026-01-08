using System;
using Game.UI.Custom;
using UnityEngine;
using UnityEngine.UI;

public class BLItemDesc : MonoBehaviour {
    [SerializeField]
    private CustomContentSizeFitter contentSizeFitter;
    
    [SerializeField]
    private Text descText;

    public void SetText(string text) {
        if (String.IsNullOrEmpty(text)) {
            contentSizeFitter.gameObject.SetActive(false);
        } else {
            contentSizeFitter.gameObject.SetActive(true);
            descText.text = text;
            contentSizeFitter.AutoLayoutVertical();
        }
    }
}
