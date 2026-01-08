using System;
using System.Collections.Generic;
using Game.Dialog;

using Scenes.MainMenuScene.Scripts;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class HotkeyDic : UnitySerializedDictionary<ControlKey, HotkeyElementUI> { }
public class CustomHotKeyUI : MonoBehaviour {
    public HotkeyDic hotkeyList;

    [SerializeField]
    private ToggleGroup toggleGroup;
    
    private HotkeyElementUI _hotkeySelected;
    private Dictionary<KeyCode, List<HotkeyElementUI>> _listHotkeyConflict;
    private HotkeyCombo _combo;
    private List<KeyCode> _acceptableKeys;
    private List<KeyCode> _inValidKey;
    public HotkeyCombo Combo {
        get {
            if (IsConflict()) return null;
            return _combo;
        }
    }
    
    public void Init(HotkeyCombo combo) {
        _combo = new HotkeyCombo(combo);
        _listHotkeyConflict = new Dictionary<KeyCode, List<HotkeyElementUI>>();
        
        foreach (var item in hotkeyList) {
            if (_combo.HasControl(item.Key)) {
                item.Value.Set(_combo.GetControl(item.Key));
                _listHotkeyConflict.Add(_combo.GetControl(item.Key),new List<HotkeyElementUI>(){item.Value});
            } else {
                item.Value.Init();
            }
        }
        _acceptableKeys = new List<KeyCode> {
            // Alphabet keys
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
            KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
            KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
            KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,

            // Number keys
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,

            // Special keys
            KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter, // Enter
            KeyCode.LeftShift, KeyCode.RightShift, // Shifts
            KeyCode.LeftAlt, KeyCode.RightAlt, // Alts
            KeyCode.LeftControl, KeyCode.RightControl, // Controls
            KeyCode.Escape,KeyCode.Backspace,KeyCode.Menu,
            KeyCode.LeftWindows,KeyCode.RightWindows,
            KeyCode.Tab,
                    
            // Arrow keys
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,

            // Numpad keys
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
            KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.KeypadDivide,KeyCode.Numlock,KeyCode.KeypadEquals,KeyCode.KeypadMinus,KeyCode.KeypadMultiply,KeyCode.KeypadPlus,
            
            // F1 - F15
            KeyCode.F1,KeyCode.F2,KeyCode.F3,KeyCode.F4,KeyCode.F5,KeyCode.F6,KeyCode.F7,KeyCode.F8,KeyCode.F9,KeyCode.F10,KeyCode.F11,
            KeyCode.F12,KeyCode.F13,KeyCode.F14,KeyCode.F15,
            
            KeyCode.PageUp, KeyCode.PageDown, KeyCode.End, KeyCode.Home, KeyCode.Insert, KeyCode.Delete, KeyCode.ScrollLock
            
            
        };
        _inValidKey = new List<KeyCode>() {
            KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4,
            
            KeyCode.Keypad1,KeyCode.Keypad2,KeyCode.Keypad3,KeyCode.Keypad4,
            
            KeyCode.PageUp, KeyCode.PageDown, KeyCode.End, KeyCode.Home, KeyCode.Insert, KeyCode.Delete, KeyCode.ScrollLock
            
            
        };
    }
    
    private void Update() {
        if (_hotkeySelected == null) return;
        
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key)) {
                
                if (key.Equals(KeyCode.Mouse0) ||
                    key.Equals(KeyCode.Mouse1) ||
                    key.Equals(KeyCode.Mouse2) ||
                    key.Equals(KeyCode.Mouse3) ||
                    key.Equals(KeyCode.Mouse4) ||
                    key.Equals(KeyCode.Mouse5) ||
                    key.Equals(KeyCode.Mouse6)) return;

                if (!_acceptableKeys.Contains(key))
                {
                    return;
                }
                
                var previousKey = _hotkeySelected.GetKeyCode();
                
                if (_listHotkeyConflict.ContainsKey(previousKey)) {
                    _listHotkeyConflict[previousKey].Remove(_hotkeySelected);
                    CheckConflict(previousKey);
                }
                
                _hotkeySelected.Set(key);
                
                if (_listHotkeyConflict.ContainsKey(key)) {
                    _listHotkeyConflict[key].Add(_hotkeySelected);
                } else {
                    _listHotkeyConflict.Add(key,new List<HotkeyElementUI>(){_hotkeySelected});
                }
                CheckConflict(key);
                _combo.AddControl(_hotkeySelected.keyControl,key);
                
                // After setting new custom hotkey, move to the next hotkey
                var keyValuePairs = new List<KeyValuePair<ControlKey, HotkeyElementUI>>(hotkeyList);
                for (int i = 0; i < hotkeyList.Count; i++)
                {
                    var element = keyValuePairs[i].Value;
                    if (element.toggle.isOn)
                    {
                        // Find the next item and set its toggle to on
                        if (i + 1 < keyValuePairs.Count)
                        {
                            var nextElement = keyValuePairs[i + 1].Value;
                            nextElement.toggle.isOn = true;
                            OnToggleChange(nextElement);
                        }
                        else 
                        {
                            toggleGroup.SetAllTogglesOff();
                        }
                        break;
                    }
                }
            }
        }
    }

    private bool IsConflict(KeyCode key) {
        if (_inValidKey.Contains(key) || 
            !_listHotkeyConflict.ContainsKey(key)||
            _listHotkeyConflict[key].Count > 1) {
            return true;
        }
        return false;
    }

    private bool IsConflict() {
        foreach (var item in hotkeyList) {
            if (IsConflict(item.Value.GetKeyCode())) {
                return true;
            }
        }
        return false;
    }

    private void CheckConflict(KeyCode key) {
        if (IsConflict(key)) {
            foreach (var item in _listHotkeyConflict[key]) {
                item.Warning();
            }
        } else {
            foreach (var item in _listHotkeyConflict[key]) {
                item.Normal();
            }
        }
    }

    public void OnToggleChange(HotkeyElementUI hotkeyElementUI) {
        if (hotkeyElementUI.toggle.isOn) {
            _hotkeySelected = hotkeyElementUI;
            toggleGroup.allowSwitchOff = false;
        }
    }

    public static string GetStringName(KeyCode key) {
        switch (key) {
            case KeyCode.Return: return "Enter";
            
            case KeyCode.LeftArrow: return "Left";
            case KeyCode.RightArrow: return "Right";
            case KeyCode.UpArrow: return "Up";
            case KeyCode.DownArrow: return "Down";
            
            case KeyCode.LeftWindows: return "LWin";
            case KeyCode.RightWindows: return "RWin";
            
            case KeyCode.LeftAlt: return "LAlt";
            case KeyCode.RightAlt: return "RAlt";
            
            case KeyCode.LeftShift: return "LShift";
            case KeyCode.RightShift: return "RShift";
            
            case KeyCode.LeftControl: return "LCtrl";
            case KeyCode.RightControl: return "RCtrl";
        }
        return key.ToString();
    }
}
