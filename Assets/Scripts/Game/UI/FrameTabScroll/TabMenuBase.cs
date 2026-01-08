using System;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.FrameTabScroll {
    public class TabMenuBase<TabMenu> : MonoBehaviour {
        [SerializeField]
        private TabMenu typeMenu;

        [SerializeField]
        private GameObject objOn;

        [SerializeField]
        private GameObject textOn;
        
        // [SerializeField]
        // private GameObject objOff;

        private ISoundManager _soundManager;
        public Action<TabMenu> OnSelectMenu;
        private Toggle _toggle;
        private ToggleGroup _toggleGroup;

        public TabMenu TypeMenu => typeMenu;

        private void Awake() {
            _toggleGroup = gameObject.transform.parent.GetComponent<ToggleGroup>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _toggle = GetComponent<Toggle>();
        }

        public void ForceSelect() {
            if (_toggle.isOn) {
                return;
            }
            _toggle.isOn = true;
            // _toggle.Select();
        }

        public void UiSelect(bool isSelected) {
            if (objOn) {
                objOn.SetActive(!isSelected);
                if (textOn) {
                    textOn.SetActive(isSelected);
                }
            }
            // if (this.objOff) this.objOff.SetActive(!isSelected);
        }

        public virtual void OnSelect(bool isSelect) {
            // if (!isSelect) {
            //     return;
            // }
            _soundManager.PlaySound(Audio.Tap);
            OnSelectMenu?.Invoke(typeMenu);
        }
    }
}