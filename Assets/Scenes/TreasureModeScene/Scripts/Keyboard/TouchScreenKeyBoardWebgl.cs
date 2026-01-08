using System;

using App;

using DG.Tweening;

using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class TouchScreenKeyboardWebgl : MonoBehaviour
{
    [SerializeField] private GameObject display;
    [SerializeField] private Text displayTMPText;
    private bool _canPressDot = true;
    
    private string _inputText = string.Empty;
    public event Action<string> OnValueChanged;
    public event Action OnKeyboardClosed;

    private RectTransform _rectTransform;
    private float _hiddenPosition = -275f;
    private float _visiblePosition = 0;
    private bool _isInitialized = false;
    private ISoundManager _soundManager;
    private bool _firstInit;

    private void Init()
    {
        if(_firstInit)
            return;
        _firstInit = true;
        _rectTransform = GetComponent<RectTransform>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        gameObject.SetActive(false);
    }

    private void HandleInputChange(string newValue)
    {
        OnValueChanged?.Invoke(newValue);
        displayTMPText.text = newValue;

    }
    

    public void OnKeyPress(string key)
    {
        if (_isInitialized == false)
        {
            return;
        }
        _soundManager.PlaySound(Audio.Tap);
        if (key == "back")
        {
            if (_inputText.Length > 0)
            {
                _inputText = _inputText.Substring(0, _inputText.Length - 1);
                HandleInputChange(_inputText);
            }
        }
        else if (key == "." && !_inputText.Contains(".") && _inputText.Length > 0)
        {
            if(!_canPressDot)
                return;
            _inputText += key;
            HandleInputChange(_inputText);
        }
        else if (key == "0" && _inputText.Length == 1 && _inputText[0] == '0')
        {
            return;
            
        }
        else if (int.TryParse(key, out _))
        {
            _inputText += key;
            HandleInputChange(_inputText);
        }
    }
    
    public void OpenKeyboard(bool showDisplay = false, bool canPressDot = true) {
        _isInitialized = false;
        Init();
        display.SetActive(showDisplay);
        displayTMPText.text = "";
        _canPressDot = canPressDot;
        _rectTransform.DOAnchorPos3DY(_hiddenPosition, 0);
        gameObject.SetActive(true);
        _rectTransform.DOAnchorPos3DY(_visiblePosition, 0.15f).SetEase(Ease.Linear)
            .OnComplete(() => {
                _isInitialized = true;
            });
    }

    public void OnButtonClose() {
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        CloseKeyboard();
    }
    

    public void CloseKeyboard() {
        if(_isInitialized == false) {
            return;
        }
        OnKeyboardClosed?.Invoke();
        OnKeyboardClosed = null;
        
        _isInitialized = false;
        _rectTransform.DOAnchorPos3DY(_hiddenPosition, 0.15f).SetEase(Ease.Linear)
            .OnComplete(() => {
                gameObject.SetActive(false);
            });
    }
    
    public void ClearInput() {
        _inputText = string.Empty;
        HandleInputChange(_inputText);
    }
    
    public string GetInput() {
        return _inputText;
    }
}