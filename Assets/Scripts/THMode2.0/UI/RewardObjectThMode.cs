using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class RewardObjectThMode : MonoBehaviour {
    [SerializeField]
    private Image icon;
    
    [SerializeField]
    private TMP_Text amount;
    
    private RectTransform _rect;
    private CanvasGroup _canvas;
    
    public void Init() {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponent<CanvasGroup>();
    }
    
    public Image Icon {
        get => icon;
        set => icon = value;
    }
    
    public TMP_Text Amount {
        get => amount;
        set => amount = value;
    }
    
    public RectTransform Rect => _rect;
    public CanvasGroup Canvas => _canvas;
}
