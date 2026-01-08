using System.Collections.Generic;
using App;
using Engine.Input;
using Senspark;
using Services;
using Services.RemoteConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMChangeMode : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private GameObject arrowLeft, controllerLeft;

        [SerializeField]
        private GameObject arrowRight, controllerRight;
        
        [SerializeField]
        private TextMeshProUGUI title;

        public GameModeType GameMode => _modes[_currentMode];
        private ISoundManager _soundManager;
        private IRankInfoManager _rankInfoManager;
        private IRemoteConfig _remoteConfig;
        private InputManager _inputManager;
        
        public bool Interactable {
            get => button.interactable;
            set {
                button.interactable = value;
                arrowLeft.SetActive(value);
                arrowRight.SetActive(value);
                controllerLeft.SetActive(value & AppConfig.IsWebGL());
                controllerRight.SetActive(value & AppConfig.IsWebGL());
            }
        }
        
        private readonly Dictionary<GameModeType, string> _modeTitle = new Dictionary<GameModeType, string>()  {
            { GameModeType.StoryMode, "ADVENTURE"},
            { GameModeType.PvpMode, "1 VS 1"}
        };
        private readonly GameModeType[] _modes = new GameModeType[] {GameModeType.PvpMode, GameModeType.StoryMode};

        private int _currentMode = 0;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _rankInfoManager = ServiceLocator.Instance.Resolve<IRankInfoManager>();
            _remoteConfig = ServiceLocator.Instance.Resolve<IRemoteConfig>();
            _inputManager = ServiceLocator.Instance.Resolve<InputManager>();
            var defaultMode = RemoteConfigHelper.GetGameMode(_remoteConfig);
            if (_rankInfoManager.LastPlayMode == GameModeType.UnKnown) {
                _currentMode = defaultMode == GameModeType.StoryMode ? 1 : 0;
            } else {
                for (var i = 0; i < _modes.Length; i++) {
                    if (_modes[i] == _rankInfoManager.LastPlayMode) {
                        _currentMode = i;
                    }
                }
            }
            button.onClick.AddListener(ToggleMode);
            UpdateTitle();
        }

        private void Update() {
            if(_inputManager.ReadButton(ControllerButtonName.RB)
               || _inputManager.ReadButton(ControllerButtonName.LB)
               || Input.GetKeyDown(KeyCode.LeftArrow)
               || Input.GetKeyDown(KeyCode.RightArrow)) {
                if(button.IsInteractable())
                    ToggleMode();
            }
        }
        
        private void ToggleMode() {
            _soundManager.PlaySound(Audio.Tap);
            _currentMode += 1;
            if (_currentMode >= _modes.Length) {
                _currentMode = 0;
            }
            _rankInfoManager.UpdateLastPlayMode(_modes[_currentMode]);
            UpdateTitle();
        }

        private void UpdateTitle() {
            var mode = _modes[_currentMode];
            title.text = _modeTitle[mode];
        }
    }
}