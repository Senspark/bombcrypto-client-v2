using Senspark;

using UnityEngine;

namespace App {
    [Service(nameof(IHotkeyControlManager))]
    public interface IHotkeyControlManager : IService {
        void SaveHotkeySet(HotKeySet set);
        HotKeySet GetHotkeySet();
        void SaveCustomHotkey(HotkeyCombo combo);
        HotkeyCombo GetHotkeyCombo(HotKeySet hotKeySet);
        HotkeyCombo GetHotkeyCombo();
    }
}
