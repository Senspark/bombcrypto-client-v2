using App;

using UnityEngine;
using UnityEngine.UI;

public class CharacterSkillPointDisplay : MonoBehaviour
{
    [SerializeField]
    private Text basePointLbl;

    [SerializeField]
    private string additionFormat;
    
    [SerializeField]
    private string amountTotalFormat;
    
    [SerializeField]
    private Image shieldImg;
    
    [SerializeField]
    private Sprite[] shieldIcons;

    public void SetPoint(float basePoint, float additionPoint = 0) {
        basePointLbl.text = additionPoint > 0 
            ? string.Format(additionFormat, basePoint, additionPoint) 
            : basePoint.ToString("N0");
    }

    public void SetPoint(int amount, int total) {
        var t = amount / (float) total;
        var c = t > 0.3f ? Color.white : Color.red;
        basePointLbl.text = string.Format(amountTotalFormat, ColorTypeConverter.ToRGBHex(c), amount, total);
        UpdateUI(amount, total);
    }

    public void Clear() {
        basePointLbl.text = null;
    }

    private void UpdateUI(int amount, int total) {
        if (shieldImg) {
            var spr = GetShieldIcon(amount, total);
            shieldImg.sprite = spr;
            shieldImg.enabled = spr != null;
        }
    }
    
    private Sprite GetShieldIcon(int amount, int total) {
        var t = amount / (float) total;
        var spr = t switch {
            > 0.7f => shieldIcons[3],
            > 0.5f => shieldIcons[2],
            > 0.3f => shieldIcons[1],
            _ => shieldIcons[0]
        };
        return spr;
    }
}