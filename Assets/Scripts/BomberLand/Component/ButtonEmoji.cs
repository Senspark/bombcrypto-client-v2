using System;
using App;
using BomberLand.InGame;
using BomberLand.KeyboardInput;

using Engine.Input;

using Senspark;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEmoji : MonoBehaviour {
    [SerializeField]
    private RectTransform arrow;
    
    [SerializeField]
    private RectTransform scroll;
    
    [SerializeField]
    private EmojiIcon[] emojiIcons;
    
    [SerializeField]
    private Text hotKey;
    
    [SerializeField]
    private GameObject[] muteShadows;
    
    [SerializeField]
    private GameObject muteButton;
    
    [SerializeField]
    private GameObject muteIcon;
    
    [SerializeField]
    private GameObject unmuteIcon;
    
    [SerializeField]
    private Text coolDownText;

    [SerializeField]
    private Image progress;

    private const float EMOJI_COOLDOWN = 5;
    private const float EMOJI_HEIGHT_1_ITEM = 155f;
    private const float EMOJI_HEIGHT_2_ITEMS = 205f;
    private const float EMOJI_HEIGHT_3_ITEMS = 255f;
    private const float EMOJI_HEIGHT_4_ITEMS = 305f;

    private float _closeHeightContent;
    private float _openHeightContent;
    private bool _enable;
    private int _emojiIds;
    private Action<int> _onEmojiChoose;
    private KeyboardInputListener _keyboardListener;
    private IInputManager _inputManager;
    private ObserverHandle _handle;
    private float _elapsed;
    private bool _isCountDown;
    private bool _mute;

    public bool Mute => _mute;
    
    private void Awake() {
        ShowHotKey(true);
        _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        progress.fillAmount = 0;
        coolDownText.text = "";
        
        KeyCode keyCode = ServiceLocator.Instance.Resolve<IHotkeyControlManager>()
            .GetHotkeyCombo()
            .GetControl(ControlKey.Chat);
        hotKey.text = CustomHotKeyUI.GetStringName(keyCode);
    }

    private string SetHotKeyText(KeyCode keyCode) {
        switch (keyCode) {
            case KeyCode.Return:
                return "Enter";
            case KeyCode.LeftControl:
                return "Ctrl";
            case KeyCode.RightControl:
                return "Ctrl";
            case KeyCode.LeftAlt:
                return "Alt";
            case KeyCode.RightAlt:
                return "Alt";
            case KeyCode.LeftShift:
                return "Shift";
            case KeyCode.RightShift:
                return "Shift";
        }
        return keyCode.ToString();
    }
    
    public void InitEmojiButtons(int[] itemIds, Action<int> onEmojiChoose) {
        _emojiIds = itemIds.Length;
        _onEmojiChoose = onEmojiChoose;
        _keyboardListener = new KeyboardInputListener();
        for (var i = 0; i < emojiIcons.Length; i++) {
            if (i >= itemIds.Length) {
                emojiIcons[i].gameObject.SetActive(false);
            } 
            else {
                emojiIcons[i].gameObject.SetActive(true);
                emojiIcons[i].SetItemId(itemIds[i]);
                emojiIcons[i].SetClickCallback(OnEmojiClicked);
                var keyCodes = emojiIcons[i].GetHotKey(i);
                _keyboardListener.AddKeys(keyCodes, itemIds[i]);
            }
        }
        
        _closeHeightContent = scroll.sizeDelta.y;
        _openHeightContent = GetContentHeight();
        
        _handle = new ObserverHandle();
        _handle.AddObserver(_keyboardListener, new KeyInputObserver() {
            KeyDownOnItem = OnEmojiClicked
        });
        UpdateMuteState();
    }
    
    private float GetContentHeight() {
        switch (_emojiIds) {
            case 1:
                return EMOJI_HEIGHT_1_ITEM;
            case 2:
                return EMOJI_HEIGHT_2_ITEMS;
            case 3:
                return EMOJI_HEIGHT_3_ITEMS;
            case 4:
                return EMOJI_HEIGHT_4_ITEMS;
        }
        return 0;
    }
    
    private void OnEmojiClicked(int itemId) {
        if (_isCountDown || _mute) {
            return;
        }
        
        _onEmojiChoose(itemId);
        StartCooldown();
    }
    
    
    
    private void StartCooldown() {
        _elapsed = EMOJI_COOLDOWN;

        foreach (var item in emojiIcons) {
            item.Interactable = false;
            item.SetProgress(1);
        }
            
        _isCountDown = true;
    }
        
    private void Update() {
        if (_emojiIds == 0) {
            return;
        }
        CheckController();
        
        var delta = Time.deltaTime;
        _keyboardListener.OnProcess(delta);
            
        if (!_isCountDown) {
            return;
        }
        _elapsed -= delta;;
        if (_elapsed <= 0) {
            progress.fillAmount = 0;
            coolDownText.text = "";
            _isCountDown = false;
            OnEndCountDown();
            return;
        }
        var percent = (EMOJI_COOLDOWN - _elapsed) / EMOJI_COOLDOWN;
        progress.fillAmount = 1 - percent;
        coolDownText.text = $"{(int)_elapsed}";
        SetProgress(1 - percent);
    }

    private void CheckController() {
        if (_inputManager.ReadButton(ControllerButtonName.DPadUp)) {
            _keyboardListener.KeyDownOnItem(KeyCode.Keypad1);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.DPadRight)) {
            _keyboardListener.KeyDownOnItem(KeyCode.Keypad2);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.DPadDown)) {
            _keyboardListener.KeyDownOnItem(KeyCode.Keypad3);
            return;
        }
        if (_inputManager.ReadButton(ControllerButtonName.DPadLeft)) {
            _keyboardListener.KeyDownOnItem(KeyCode.Keypad4);
            return;
        }
        if(_inputManager.ReadJoystick(_inputManager.InputConfig.Back) || Input.GetKeyDown(KeyCode.M)) {
            OnMuteButtonClicked(!_mute);
        }
    }
    
    private void OnDestroy() {
        _keyboardListener?.ClearKeys();
        _handle?.Dispose();
    }
    
    private void OnEndCountDown() {
        foreach (var item in emojiIcons) {
            item.SetProgress(0);
            item.Interactable = true;
        }            
    }
        
    private void SetProgress(float value) {
        foreach (var item in emojiIcons) {
            item.SetProgress(value);
        }
    }
    
    public void OnMainButtonClicked() {
        _enable = !_enable;
        UpdateButtonState();
    }
    
    public void OnMuteButtonClicked(bool state) {
        _mute = state;
        UpdateMuteState();
    }

    private void UpdateButtonState() {
        var size = scroll.sizeDelta;
        if (_enable) {
            arrow.localScale = new Vector3(1f, 1f, 1f);
            size.y = _openHeightContent;
        } else {
            arrow.localScale = new Vector3(1f, -1f, 1f);
            size.y = _closeHeightContent;
            
        }
        scroll.sizeDelta = size;

        UpdateMuteState();
    }

    private void UpdateMuteState() {
        if (_emojiIds == 0) {
            muteButton.SetActive(false);
            return;
        }
        muteButton.SetActive(_enable);
        UpdateMuteShadows(_mute);
        UpdateMuteIcon(_mute);
    }

    private void UpdateMuteShadows(bool state) {
        foreach (var item in muteShadows) {
            item.SetActive(state);
        }
    }
    
    private void UpdateMuteIcon(bool state) {
        muteIcon.SetActive(state);
        unmuteIcon.SetActive(!state);
    }
    
    public void ShowHotKey(bool value) {
#if UNITY_WEBGL || FORCE_USE_KEYBOARD
        hotKey.gameObject.SetActive(value);
#else
        hotKey.gameObject.SetActive(false);
#endif            
    }
}
