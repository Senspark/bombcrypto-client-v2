using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class FusionCountItem : MonoBehaviour {
    [SerializeField]
    private GameObject selectOn;
    
    [SerializeField, CanBeNull]
    private GameObject selectOff;
    
    [SerializeField]
    private Text countTxt;

    public void SetValue(int value, int compare = 0, string colorOn = null, string colorOff = null) {
        var val = Mathf.Max(value, 0);
        if (selectOff != null) {
            selectOn.SetActive(val > compare);
            selectOff.SetActive(val <= compare);
        } else {
            selectOn.SetActive(val > compare);
        }
        countTxt.text = $"{val}";
        if (colorOn != null && colorOff != null) {
            if (selectOn.activeSelf) {
                countTxt.color = HexToColorConverter(colorOn);
            } else {
                countTxt.color = HexToColorConverter(colorOff);
            }
        }
    }
    
    public static Color HexToColorConverter(string hex)
    {
        // Remove '#' if present
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Ensure the string is 6 characters (RGB hex format)
        if (hex.Length == 6)
        {
            // Convert each pair of hex digits to a float between 0 and 1
            var r = Mathf.Clamp01(int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f);
            var g = Mathf.Clamp01(int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f);
            var b = Mathf.Clamp01(int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f);

            return new Color(r, g, b);
        }
        return Color.white;
    }
}
