using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBChainText : ReloadBlockchainCurrency {
    [SerializeField] 
    private Text text;
    protected override string GetCurrentValue() {
        return text.text;
    }
    
    protected override void UpdateText(string value) {
        text.text = value;
    }
}
