using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TokenButton : MonoBehaviour {
    [SerializeField]
    private PoolType poolType;
    private Image _button;
    private PoolButton _poolButton;
    private RectTransform _rectTransform;
    [HideInInspector]
    public bool isChosen;
    public RectTransform RectTransform => _rectTransform;
    public PoolType PoolType => poolType;
    
    public void Init(PoolButton poolButton) {
        _poolButton = poolButton;
        _button = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void OnBtnClick() {
        if (_poolButton.isChanging)
            return;
        
        PerformClick();
    }
    
    private void PerformClick() {
        _poolButton.BtnTokenClick(this);
    }
    
    public void ChangeBtnUi(Sprite sprite, bool choose) {
        _button.sprite = sprite;
        isChosen = choose;
    }
}