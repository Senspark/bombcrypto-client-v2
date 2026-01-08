using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Engine.Manager;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public enum InputControlName {
    None,
    RB,
    RT,
    LB,
    LT,
    A,
    B,
    X,
    Y,
    Emoji,
    UseBooster,
    PlantBomb,
    Enter,
    Back,
    DpadUp,
    DpadDown,
    DpadLeft,
    DpadRight,
}

public enum KeyboardControlName {
    None,
    Space,
    Esc,
    AKey,
    SKey,
    DKey,
    WKey,
    Left,
    Right,
    MKey,
    EKey,
}

public class UiControlChange : MonoBehaviour {
    [SerializeField]
    [CanBeNull]
    private GameObject keyBoardObj, controllerObj;

    [SerializeField]
    private InputControlName controller;

    [SerializeField]
    private KeyboardControlName keyboard;

    private Sprite _imageControllerXbox, _imageControllerPs, _imageKeyboard;
    private Image _controllerImage;

    private IInputManager _input;

    private async void Awake() {
        if (AppConfig.IsMobile()) {
            gameObject.SetActive(false);
            return;
        }
        _input = ServiceLocator.Instance.Resolve<IInputManager>();

        if (_input == null)
            return;

        InitializeImages();

        await LoadSprites();

        UpdateControllerImage();
        UpdateKeyboardImage();
    }

    private void InitializeImages() {
        if (controllerObj != null) {
            _controllerImage = controllerObj.GetComponent<Image>();
            if (_controllerImage != null)
                _controllerImage.enabled = false;
        }
        if (keyBoardObj != null && keyboard != KeyboardControlName.None) {
            var keyBoardImg = keyBoardObj.GetComponent<Image>();
            if (keyBoardImg != null)
                keyBoardImg.enabled = false;
        }
    }

    private async UniTask LoadSprites() {
        if (keyboard != KeyboardControlName.None)
            _imageKeyboard = await _input.GetImage(keyboard.ToString());
        if (controller != InputControlName.None) {
            _imageControllerXbox = await _input.GetImage(MapToString(controller));
            _imageControllerPs = await _input.GetImage(MapXboxToPs(MapToString(controller)));
        }
    }

    private void UpdateControllerImage() {
        if (_controllerImage != null) {
            _controllerImage.enabled = true;
            if (_imageControllerXbox != null && _input.ControllerType == ControllerType.Xbox)
                _controllerImage.sprite = _imageControllerXbox;
            else if (_imageControllerPs != null && _input.ControllerType == ControllerType.PlayStation)
                _controllerImage.sprite = _imageControllerPs;
            else {
                _controllerImage.sprite = _imageControllerXbox;
            }
        }
    }

    private void UpdateKeyboardImage() {
        if (_imageKeyboard != null && keyBoardObj != null) {
            var keyBoardImg = keyBoardObj.GetComponent<Image>();
            if (keyBoardImg != null) {
                keyBoardImg.enabled = true;
                keyBoardImg.sprite = _imageKeyboard;
            }
        }
    }

    private void OnEnable() {
        if (AppConfig.IsMobile()) {
            return;
        }
        OnChangeInput(_input.InputType);
        OnChangeController(_input.ControllerType);
        EventManager<InputType>.Add(InputEvent.OnChangeInput, OnChangeInput);
        EventManager<ControllerType>.Add(InputEvent.OnchangeController, OnChangeController);
    }

    private void OnDisable() {
        if (AppConfig.IsMobile()) {
            return;
        }
        EventManager<InputType>.Remove(InputEvent.OnChangeInput, OnChangeInput);
        EventManager<ControllerType>.Remove(InputEvent.OnchangeController, OnChangeController);
    }

    private void OnChangeInput(InputType type) {
        if (keyBoardObj != null)
            keyBoardObj.SetActive(type == InputType.Keyboard);
        if (controllerObj != null)
            controllerObj?.SetActive(type == InputType.Controller);
    }

    private void OnChangeController(ControllerType type) {
        if (_controllerImage == null || _imageControllerXbox == null || _imageControllerPs == null)
            return;

        _controllerImage.sprite = type == ControllerType.PlayStation ? _imageControllerPs : _imageControllerXbox;
    }

    private string MapToString(InputControlName input) {
        return input switch {
            InputControlName.RB => InputControlName.RB.ToString(),
            InputControlName.RT => InputControlName.RT.ToString(),
            InputControlName.LB => InputControlName.LB.ToString(),
            InputControlName.LT => InputControlName.LT.ToString(),
            InputControlName.A => InputControlName.A.ToString(),
            InputControlName.B => InputControlName.B.ToString(),
            InputControlName.X => InputControlName.X.ToString(),
            InputControlName.Y => InputControlName.Y.ToString(),
            InputControlName.DpadDown => InputControlName.DpadDown.ToString(),
            InputControlName.DpadLeft => InputControlName.DpadLeft.ToString(),
            InputControlName.DpadRight => InputControlName.DpadRight.ToString(),
            InputControlName.DpadUp => InputControlName.DpadUp.ToString(),
            InputControlName.Emoji => _input.InputConfig.Emoji,
            InputControlName.UseBooster => _input.InputConfig.UseBooster,
            InputControlName.PlantBomb => _input.InputConfig.PlantBomb,
            InputControlName.Enter => _input.InputConfig.Enter,
            InputControlName.Back => _input.InputConfig.Back,
            _ => string.Empty,
        };
    }

    private string MapXboxToPs(string xboxName) {
        return xboxName switch {
            "X" => "Square",
            "Y" => "Triangle",
            "A" => "Cross",
            "B" => "Circle",
            "RB" => "R1",
            "RT" => "R2",
            "LB" => "L1",
            "LT" => "L2",
            _ => xboxName,
        };
    }
}