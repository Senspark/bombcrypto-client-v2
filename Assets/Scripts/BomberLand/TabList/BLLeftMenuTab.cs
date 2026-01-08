using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI {
    public enum BLTabType {
        Null = -1,
        BombSkin = 0,
        Heroes,
        Booster,
        Misc,
        Chest,
        Wing,
        Trail,
        OnSell,
        FireSkin,
        BoxReward,
        Grind,
        Fuse,
        Costume,
        Emoji,
        Avatar,
    }

    public class BLLeftMenuTab : MonoBehaviour {
        [SerializeField]
        private Transform tabList;

        private Dictionary<BLTabType, BLLefTab> _dictTab;
        private Action<BLTabType> _onSelectTab;

        private Dictionary<BLTabType, BLLefTab> DictTab {
            get {
                if (_dictTab == null) {
                    _dictTab = new Dictionary<BLTabType, BLLefTab>();
                    var tabs = tabList.GetComponentsInChildren<BLLefTab>();
                    foreach (var tab in tabs) {
                        _dictTab[tab.Type] = tab;
                    }
                }
                return _dictTab;
            }
        }

        public void HideTab(BLTabType type) {
            DictTab[type].gameObject.SetActive(false);
        }
        
        public void SetOnSelectCallback(Action<BLTabType> callback) {
            _onSelectTab = callback;
            foreach (var tab in DictTab.Values) {
                tab.OnSelectMenu = SetSelectMenu;
            }
        }

        public void ForceSelected(BLTabType tabType) {
            // _dictTab[tabType].SetSelected();
            foreach (var tab in DictTab) {
                if (tab.Key == tabType) {
                    tab.Value.SetSelected(true);
                } else {
                    tab.Value.SetSelected(false);
                }
            }
        }

        private void SetSelectMenu(BLTabType tabType) {
            _onSelectTab?.Invoke(tabType);
        }
    }
}