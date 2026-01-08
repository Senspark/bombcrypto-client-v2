using System.Globalization;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MinePoolUi : MonoBehaviour {
    [SerializeField]
    private TMP_Text amountText;
    
    [SerializeField]
    private Image fill;
    
    [SerializeField]
    private RectTransform poolPosition;
    
    private readonly Color32 _colorGreen = new (0, 202, 13,255);
    private readonly Color32 _colorRed = new (202, 0, 13,255);
    
    public RectTransform PoolPosition => poolPosition;
    
    /// <summary>
    /// Update ui cho từng pool
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxPool"></param>
    public void UpdatePool(float value, float maxPool) {
        amountText.text = value.ToString("0.########",CultureInfo.CurrentCulture);
        var amount = Mathf.Clamp01(value / maxPool);
        fill.fillAmount = amount;
        
        fill.color = amount <= 0.25f ? _colorRed : _colorGreen;
    }
}
