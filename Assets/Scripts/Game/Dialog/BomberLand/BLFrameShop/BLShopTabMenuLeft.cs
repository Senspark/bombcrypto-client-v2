using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    
    public class BLShopTabMenuLeft : FrameTabScroll.CTabMenu {
        [SerializeField]
        private GameObject tagNew;

        private void Start() {
            if (tagNew == null) {
                return;
            }
            var isClicked = PlayerPrefs.GetInt($"clickedKey_{TypeMenu.ToString()}", 0) == 1;
            tagNew.SetActive(!isClicked);
        }

        public override void OnSelect(bool isSelect) {
            base.OnSelect(isSelect);
            if (tagNew == null) {
                return;
            }
            tagNew.SetActive(false);
            PlayerPrefs.SetInt($"clickedKey_{TypeMenu.ToString()}", 1);
            PlayerPrefs.Save();
        }
    }
}