using System;
using App;
using BLPvpMode.Data;
using BomberLand.Component;
using BomberLand.DirectionInput;
using Senspark;
using PvpMode.Manager;
using StoryMode.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BLPvpMode.UI {
    public class BLPlayerInputKey : MonoBehaviour, IBLInputKey {
        [SerializeField]
        private GameObject pvpModeDirectionInputs;

        [SerializeField]
        private ButtonUseBooster buttonShield;

        [SerializeField]
        private ButtonUseBooster buttonKey;

        [SerializeField]
        private ButtonBomb buttonBomb;
        
        [SerializeField]
        private ButtonEmoji buttonEmoji;

        [SerializeField]
        private GameObject dpadJoys;

        [SerializeField]
        private CombineJoystick joystick;

        [SerializeField]
        private JoyPad joyPad;

        [SerializeField]
        private Button buttonBack;
        
        [SerializeField]
        private Transform rightButtonGroup;

        private IStorageManager _storageManager;
        private MultiDirectionInput _multiDirectionInputs;
        private ButtonUseBooster _boosterButtonRequest;
        private JoyStickDirectionInput _joystickDirectionInput;
        private JoyStickDirectionInput _joyPadDirectionInput;

        private ObserverHandle _handle;
        private PlayerInputCallback _playerInputCallback;
        private IInputManager _inputManager;
        private HotkeyCombo _hotkeyCombo; 

        private void Awake() {
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_storageManager, new StoreManagerObserver() {
                OnJoystickChoice = OnJoystickChoice,
            });
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _hotkeyCombo = ServiceLocator.Instance.Resolve<IHotkeyControlManager>().GetHotkeyCombo();
            Init();
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        private void Init() {
            _multiDirectionInputs = new MultiDirectionInput();
            if (!Application.isMobilePlatform) {
                pvpModeDirectionInputs.SetActive(false);
                var position = buttonBomb.transform.position;
                buttonShield.transform.position = position;
                var transformKey = buttonKey.transform;
                position.x = transformKey.position.x;
                transformKey.position = position;
                var keyboardDirectionInput = new KeyboardDirectionInput();
                _multiDirectionInputs.AddInput(keyboardDirectionInput);
            } else {
                pvpModeDirectionInputs.SetActive(true);
                _joystickDirectionInput = new JoyStickDirectionInput(joystick);
                _joyPadDirectionInput = new JoyStickDirectionInput(joyPad);

                if (_storageManager.IsJoyStickChoice) {
                    joystick.gameObject.SetActive(true);
                    joyPad.gameObject.SetActive(false);
                    _multiDirectionInputs.AddInput(_joystickDirectionInput);
                } else {
                    joystick.gameObject.SetActive(false);
                    joyPad.gameObject.SetActive(true);
                    _multiDirectionInputs.AddInput(_joyPadDirectionInput);
                }

                var keyboardDirectionInput = new KeyboardDirectionInput();
                _multiDirectionInputs.AddInput(keyboardDirectionInput);
            }
           


            buttonKey.Interactable = false;
            buttonKey.ShowHotKey(false);
        }
        
        public void InitEmojiButtons(int[] itemIds, Action<int> onEmojiChoose) {
            buttonEmoji.InitEmojiButtons(itemIds, onEmojiChoose);
        }

        public bool GetMuteState() {
            return buttonEmoji.Mute;
        }

        private void OnJoystickChoice() {
            if (Application.isMobilePlatform) {
                _multiDirectionInputs.Clear();
                if (_storageManager.IsJoyStickChoice) {
                    joystick.gameObject.SetActive(true);
                    joyPad.gameObject.SetActive(false);
                    _multiDirectionInputs.AddInput(_joystickDirectionInput);
                } else {
                    joystick.gameObject.SetActive(false);
                    joyPad.gameObject.SetActive(true);
                    _multiDirectionInputs.AddInput(_joyPadDirectionInput);
                }
            }
        
        }

        private void SwapBoosterButton() {
            var transformShield = buttonShield.transform;
            var transformKey = buttonKey.transform;
            (transformKey.position, transformShield.position) = (transformShield.position, transformKey.position);
            (transformKey.localScale, transformShield.localScale) =
                (transformShield.localScale, transformKey.localScale);
        }

        public void SetupButtons(BoosterStatus boosterStatus, PlayerInputCallback callback) {
            _playerInputCallback = callback;
            buttonShield.SetQuantity(boosterStatus.GetQuantity(BoosterType.Shield));
            buttonShield.Interactable = boosterStatus.IsChooseBooster(BoosterType.Shield);
            buttonKey.SetQuantity(boosterStatus.GetQuantity(BoosterType.Key));
            buttonKey.Interactable = false;
        }

        public void UpdateButtons(BoosterStatus boosterStatus, float coolDown) {
            buttonShield.SetQuantity(boosterStatus.GetQuantity(BoosterType.Shield));
            buttonShield.Interactable = boosterStatus.IsChooseBooster(BoosterType.Shield);
            buttonKey.SetQuantity(boosterStatus.GetQuantity(BoosterType.Key));
            buttonKey.Interactable = false;
            buttonKey.SetCoolDown(coolDown);
            buttonShield.SetCoolDown(coolDown);
        }

        public void ResetButtons(BoosterStatus boosterStatus) {
            buttonShield.Reset();
            buttonShield.Interactable = boosterStatus.IsChooseBooster(BoosterType.Shield);
            buttonKey.Reset();
            buttonKey.Interactable = false;
        }

        public Image GetBlinkButton() {
            return joyPad.BlinkButton;
        }

        public Vector2 GetDirection() {
            return _multiDirectionInputs.GetDirection();
        }

        public void CheckInputKeyDown() {
            if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.PlaceBom)) || _inputManager.ReadButton(_inputManager.InputConfig.PlantBomb)) {
                OnBombButtonClicked();
            }

            if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.UseItem)) || _inputManager.ReadButton(_inputManager.InputConfig.UseBooster)) {
                OnShieldButtonClicked();
            }

            if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.UseItem)) || _inputManager.ReadButton(_inputManager.InputConfig.UseBooster)) {
                OnKeyButtonClicked();
            }
            
            if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.Chat)) || _inputManager.ReadButton(_inputManager.InputConfig.Emoji)) {
                OnEmojiButtonClicked();
            }
            
            if (_inputManager.ReadButton(_inputManager.InputConfig.Settings)) {
                OnBackButtonClicked();
            }
        }
        

        public void OnBombButtonClicked() {
            _playerInputCallback.OnBombButtonClicked?.Invoke();
        }
        

        public void OnShieldButtonClicked() {
            if (!buttonShield.Interactable) {
                return;
            }
            _boosterButtonRequest = buttonShield;
            buttonShield.Interactable = false;
            _playerInputCallback.OnBoosterButtonClicked?.Invoke(BoosterType.Shield);
        }
        public void OnKeyButtonClicked() {
            if (!buttonKey.Interactable) {
                return;
            }
            _boosterButtonRequest = buttonKey;
            buttonKey.Interactable = false;
            _playerInputCallback.OnBoosterButtonClicked?.Invoke(BoosterType.Key);
        }

        private void OnEmojiButtonClicked() {
            buttonEmoji.OnMainButtonClicked();
        }

        public void OnBackButtonClicked() {
            _playerInputCallback.OnBackButtonClicked?.Invoke();
        }

        public void BoosterButtonUsed(BoosterType type, BoosterStatus boosterStatus,
            Func<bool> isInJailChecker) {
            var button = type == BoosterType.Shield ? buttonShield : buttonKey;
            boosterStatus.RemoveChooseBooster(button.Type);
            button.SetQuantity(boosterStatus.GetQuantity(button.Type));

            var isChoose = boosterStatus.IsChooseBooster(button.Type);
            StartCoolDown(button, isInJailChecker, isChoose);
        }

        public void OnFailedToUseBooster() {
            _boosterButtonRequest.Interactable = true;
        }

        private void StartCoolDown(ButtonUseBooster button, Func<bool> isInJailChecker, bool isChoose) {
            button.StartCoolDown(() => { OnAfterCoolDown(button, isInJailChecker, isChoose); });
        }

        private void OnAfterCoolDown(ButtonUseBooster button, Func<bool> isInJailChecker, bool isChoose) {
            if (button.Type != BoosterType.Key) {
                button.Interactable = isChoose;
                return;
            }

            // active button key if player is in jail
            if (isInJailChecker()) {
                button.Interactable = isChoose;
            }
        }

        public void OnPlayerInJail(BoosterStatus boosterStatus) {
            SwapBoosterButton();
            buttonKey.Interactable = boosterStatus.IsChooseBooster(BoosterType.Key);
            buttonKey.ShowHotKey(true);
            buttonShield.Interactable = false;
            buttonShield.ShowHotKey(false);
        }

        public void OnPlayerEndInJail(BoosterStatus boosterStatus) {
            SwapBoosterButton();
            buttonShield.Interactable = boosterStatus.IsChooseBooster(BoosterType.Shield);
            buttonShield.ShowHotKey(true);
            buttonKey.Interactable = false;
            buttonKey.ShowHotKey(false);
        }

        public void SetEnableInputPlantBomb(bool isEnable) {
            buttonBomb.Interactable = isEnable;
        }

        public void SetEnableInputShield(bool isEnable) {
            buttonShield.Interactable = isEnable;
            buttonShield.SetQuantity(isEnable ? 1 : 0);
        }

        public void SetEnableInputKey(bool isEnable) {
            buttonKey.Interactable = isEnable;
            buttonKey.SetQuantity(isEnable ? 1 : 0);
        }

        public void SetQuantityBomb(int value) {
            buttonBomb.SetQuantity(value);
        }

        public void SetVisible(ElementGui element, bool value) {
            switch (element) {
                case ElementGui.Joystick:
                    dpadJoys.SetActive(value);
                    break;
                case ElementGui.ButtonBomb:
                    buttonBomb.SetVisible(value);
                    break;
                case ElementGui.ButtonKey:
                    buttonKey.SetVisible(value);
                    break;
                case ElementGui.ButtonShield:
                    buttonShield.SetVisible(value);
                    break;
                case ElementGui.BtBack:
                    buttonBack.gameObject.SetActive(value);
                    break;
                case ElementGui.ButtonEmoji:
                    buttonEmoji.gameObject.SetActive(value);
                    break;
                case ElementGui.StatDamage:
                case ElementGui.StatBombUp:
                case ElementGui.StatFireUp:
                case ElementGui.StatBoots:
                case ElementGui.Timer:
                case ElementGui.HeroGroup:
                case ElementGui.MatchId:
                default:
                    // do nothing
                    break;
            }
        }

        public Transform ButtonBombTransform => buttonBomb.transform;
        public Transform ButtonShieldTransform => buttonShield.transform;
        public Transform ButtonKeyTransform => buttonKey.transform;
        public JoyPad JoyPad => joyPad;
    }
}