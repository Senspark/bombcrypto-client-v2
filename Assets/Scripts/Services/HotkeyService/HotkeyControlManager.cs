using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Newtonsoft.Json;

using UnityEngine;

public class HotkeyControlManager : IHotkeyControlManager
{
    private readonly IDataManager _dataManager;
    private HotKeySet _currentSet;
    private Dictionary<HotKeySet, HotkeyCombo> _hotkeyCombos;

    public HotkeyControlManager(IDataManager dataManager) {
        _dataManager = dataManager;
    }
    public Task<bool> Initialize() {
        var combo1 = new HotkeyCombo(new Dictionary<ControlKey, KeyCode>()
        {
            { ControlKey.MoveLeft , KeyCode.LeftArrow},
            { ControlKey.MoveRight , KeyCode.RightArrow},
            { ControlKey.MoveUp , KeyCode.UpArrow},
            { ControlKey.MoveDown , KeyCode.DownArrow},
            { ControlKey.PlaceBom , KeyCode.Space},
            { ControlKey.UseItem , KeyCode.D},
            { ControlKey.Chat , KeyCode.E},
        });
        var combo2 = new HotkeyCombo(new Dictionary<ControlKey, KeyCode>()
        {
            { ControlKey.MoveLeft , KeyCode.A},
            { ControlKey.MoveRight , KeyCode.D},
            { ControlKey.MoveUp , KeyCode.W},
            { ControlKey.MoveDown , KeyCode.S},
            { ControlKey.PlaceBom , KeyCode.Space},
            { ControlKey.UseItem , KeyCode.F},
            { ControlKey.Chat , KeyCode.E},
        });
        HotkeyCombo customCombo;
        var comboDataString = _dataManager.GetString(HotKeySet.Custom.ToString(), "");
        
        if (string.IsNullOrEmpty(comboDataString)) {
            customCombo = new HotkeyCombo();
        } else {
            var comboData = JsonConvert.DeserializeObject<HotkeyCombo>(comboDataString);
            customCombo =  comboData ?? new HotkeyCombo();
        }
        
        _hotkeyCombos = new Dictionary<HotKeySet, HotkeyCombo>() {
            {HotKeySet.Set1, combo1},
            { HotKeySet.Set2 ,combo2},
            { HotKeySet.Custom ,customCombo},
        };
        _currentSet = GetHotkeySet();
        return Task.FromResult(true);
    }

    public void Destroy() {
    }

    public void SaveHotkeySet(HotKeySet set) {
        _currentSet = set;
        _dataManager.SetString("set",set.ToString());
    }

    public HotKeySet GetHotkeySet() {
        string setString = _dataManager.GetString("set", "Set1");
        
        if (Enum.TryParse(setString, out HotKeySet _currentSet)) {
            return _currentSet;
        }
        return HotKeySet.Set1; 
        
    }

    public void SaveCustomHotkey(HotkeyCombo combo) {
        try
        {
            var jsonData = JsonConvert.SerializeObject(combo);
            _dataManager.SetString(HotKeySet.Custom.ToString(), jsonData);
            _hotkeyCombos[HotKeySet.Custom] = combo;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save hotkey combo: " + ex.Message);
        }
    }

    public HotkeyCombo GetHotkeyCombo(HotKeySet hotKeySet) {
        return _hotkeyCombos[hotKeySet];
    } public HotkeyCombo GetHotkeyCombo() {
        return _hotkeyCombos[_currentSet];
    }

}

