using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class ReloadBChainTextPro : ReloadBlockchainCurrency
{
    [SerializeField]
    private TMP_Text text;
    protected override string GetCurrentValue() {
        return text.text;
    }
    
    protected override void UpdateText(string value) {
        text.text = value;
    }
}
