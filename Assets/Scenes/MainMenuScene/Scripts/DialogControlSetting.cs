using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MainMenuScene.Scripts {
    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private List<TKey> keyData = new List<TKey>();
	
        [SerializeField, HideInInspector]
        private List<TValue> valueData = new List<TValue>();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
            {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (var item in this)
            {
                this.keyData.Add(item.Key);
                this.valueData.Add(item.Value);
            }
        }
    }
    [Serializable]
    public class ToggleDictionary : UnitySerializedDictionary<HotKeySet, Toggle> { }

  
    public class DialogControlSetting : Dialog {
        
        private ISoundManager _soundManager;
        private IHotkeyControlManager _hotkeyControlManager;
        private HotKeySet _currentHotkey;
        
        [SerializeField]
        private CustomHotKeyUI customHotKey;
        
        public ToggleDictionary listToggle;
        
        [SerializeField]
        private UnityEngine.Animation warningTextAnimation;
        
        [SerializeField]
        private TMP_Text warningText;

        private float ANIMATION_COOLDOWN = 1.5f;
        
        private bool _isAnimPlaying;

        public static UniTask<DialogControlSetting> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogControlSetting>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _hotkeyControlManager = ServiceLocator.Instance.Resolve<IHotkeyControlManager>();
            foreach (var item in listToggle) {
                item.Value.onValueChanged.AddListener(OnToggleChange);
            }
        }

        protected override void OnYesClick() {
            OnButtonAcceptClicked();
        }

        protected override void OnNoClick() {
            OnButtonCloseClicked();
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogSetting.Create().ContinueWith((dialog) => {
                dialog.Show(DialogCanvas);
            });
            
            Hide();
        }

        private void Start() {
            var set = _hotkeyControlManager.GetHotkeySet();
            _currentHotkey = set;
            listToggle[set].isOn = true;
            customHotKey.Init( _hotkeyControlManager.GetHotkeyCombo(HotKeySet.Custom));
        }

        public void OnButtonAcceptClicked() {
            if (listToggle[HotKeySet.Custom].isOn) {
                var combo = customHotKey.Combo;
                if (combo == null) {
                    if (!_isAnimPlaying) {
                        _isAnimPlaying = true;
                        warningText.text = "Certain control buttons are not valid";
                        warningTextAnimation.Play();
                    }
                    return;
                }
                if (combo.IsMissingControl()) {
                    if (!_isAnimPlaying) {
                        _isAnimPlaying = true;
                        warningText.text = "Fill all the boxes to choose this control type";
                        warningTextAnimation.Play();
                    }
                    return;
                }
                _hotkeyControlManager.SaveCustomHotkey(combo);
            }
            _hotkeyControlManager.SaveHotkeySet(_currentHotkey);
            
            OnButtonCloseClicked();
        }

        private void OnToggleChange(bool value) {
            foreach (var item in listToggle) {
                if (item.Value.isOn) {
                    _currentHotkey = item.Key;
                    break;
                }
            }
        }

        public void FinishAnimation() {
            _isAnimPlaying = false;
        }
    }
}