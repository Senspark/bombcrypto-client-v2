using System;
using System.Collections;
using System.Collections.Generic;

using App;

using TMPro;

using UnityEngine;

public class TestServerPvp : MonoBehaviour {
    private List<string> _listZone = new List<string>() {
        "none", "sg", "br", "us", "jp", "de"
    };

    [SerializeField]
    private TMP_Dropdown dropDown;

    public static string CurrentZone;
    
    public static bool FightBot;
    
    private void Awake() {
        if (AppConfig.IsProduction) {
            gameObject.SetActive(false);
        } 
        else {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string zone in _listZone) {
                options.Add(new TMP_Dropdown.OptionData(zone));
            }
            dropDown.options = options;
            CurrentZone = _listZone[0];
        }
    }
    
    public void OnFoundBotChanged(bool value) {
        FightBot = value;
    }
    public void OnDropDownValueChanged(int value) {
        CurrentZone = _listZone[value];
    }
}