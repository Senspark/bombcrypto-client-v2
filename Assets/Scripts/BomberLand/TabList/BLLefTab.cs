using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLLefTab : MonoBehaviour {
        [SerializeField]
        private BLTabType tabType;
        public BLTabType Type => tabType;
        
        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private Image shadow;
        
        [SerializeField]
        private GameObject tagNew;
        
        public System.Action<BLTabType> OnSelectMenu;

        private void Start() {
            if (tagNew == null) {
                return;
            }
            var isClicked = PlayerPrefs.GetInt($"clickedKey_Inv_{tabType.ToString()}", 0) == 1;
            tagNew.SetActive(!isClicked);
        }
        
        public void SetSelected(bool isSelect = true) {
            // toggle.isOn = true;
            toggle.SetIsOnWithoutNotify(isSelect);
            OnSelect(isSelect);
        }
        
        private void OnSelect(bool isSelect) {
            shadow.gameObject.SetActive(!isSelect);
            if (!isSelect) {
                return;
            }
            OnSelectMenu?.Invoke(tabType);
            if (tagNew == null) {
                return;
            }
            tagNew.SetActive(false);
            PlayerPrefs.SetInt($"clickedKey_Inv_{tabType.ToString()}", 1);
            PlayerPrefs.Save();
        }
    }
}