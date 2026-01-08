using System;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

public class IntValueButton : MonoBehaviour {
    public bool Selected {
        get => _selected;
        set {
            _selected = value;
            btnSprite.sprite = _selected ? selectedSprite : unselectedSprite;
        }
    }

    public int Value => value;

    [SerializeField]
    private int value;

    [SerializeField]
    private Button btn;

    [SerializeField]
    private Image btnSprite;

    [SerializeField]
    private Text btnText;

    [SerializeField]
    private Sprite selectedSprite;

    [SerializeField]
    private Sprite unselectedSprite;

    private bool _selected;
    private Action<IntValueButton> _onSelected;

    private void Awake() {
        SetValue(value);
    }

    public void SetCallback(Action<IntValueButton> onSelected) {
        _onSelected = onSelected;
    }

    public void SetInteractable(bool interactable) {
        btn.interactable = interactable;
    }

    public void SetValue(int newValue) {
        value = newValue;
        btnText.text = value.ToString();
    }

    public void OnBtnClicked() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _onSelected?.Invoke(this);
    }
}