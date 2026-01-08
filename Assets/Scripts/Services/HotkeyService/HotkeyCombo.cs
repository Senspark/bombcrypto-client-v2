using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeyCombo {
    public Dictionary<ControlKey, KeyCode> ListHotKey;

    public HotkeyCombo(Dictionary<ControlKey, KeyCode> listHotKey) {
        ListHotKey = listHotKey;
    }

    public HotkeyCombo() {
        ListHotKey = new Dictionary<ControlKey, KeyCode>();
    }

    public HotkeyCombo(HotkeyCombo combo) {
        ListHotKey = new Dictionary<ControlKey, KeyCode>();
        foreach (var hotkey in combo.ListHotKey) {
            ListHotKey.Add(hotkey.Key, hotkey.Value);
        }
    }
    public KeyCode GetControl(ControlKey key) {
        return ListHotKey[key];
    }

    public bool HasControl(ControlKey key) {
        if (ListHotKey.ContainsKey(key)) return true;
        return false;
    }
    public bool IsMissingControl() {
        if (ListHotKey.Count < Enum.GetValues(typeof(ControlKey)).Length)
            return true;
        return false;
    }
    public void AddControl(ControlKey key, KeyCode value) {
        if (ListHotKey.ContainsKey(key)) {
            ListHotKey[key] = value;
        } else {
            ListHotKey.Add(key,value);
        }
    }
}