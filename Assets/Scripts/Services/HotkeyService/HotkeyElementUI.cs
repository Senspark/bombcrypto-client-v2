using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotkeyElementUI : MonoBehaviour {

    [SerializeField]
    private TMP_Text keycodeTxt;

    [SerializeField]
    public Toggle toggle;
    
    [SerializeField]
    public ControlKey keyControl;
    
    private KeyCode _keyCodeSelected;

    private void Reset() {
        keycodeTxt = GetComponentInChildren<TMP_Text>();
        toggle = GetComponentInChildren<Toggle>();
    }

    private void Awake() {
        keycodeTxt.text = "";
        toggle.isOn = false;
    }

    public void Init() {
        keycodeTxt.text = "";
        _keyCodeSelected = KeyCode.None;
    }

    public void Set(KeyCode keycode) {
        keycodeTxt.text = CustomHotKeyUI.GetStringName(keycode);
        _keyCodeSelected = keycode;
    }

    public KeyCode GetKeyCode() {
        return _keyCodeSelected;
    }

    public void Warning() {
        keycodeTxt.color = Color.red;
    }

    public void Normal() {
        keycodeTxt.color = Color.white;
    }
}
