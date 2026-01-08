using UnityEngine;

namespace Game.UI {
    public class BLAltarBaseContent : MonoBehaviour {
        [SerializeField]
        private BLTabType type;

        [SerializeField]
        private UnityEngine.UI.Toggle toggle;
        
        [SerializeField]
        private GameObject noItemText;

        public BLTabType Type => type;

        protected System.Action<int, int, int, int> OnGrindCallback;
        protected System.Action<int, int, int, int, int> OnFuseCallback;

        public void SetGrindCallback(System.Action<int, int, int, int> callback) {
            OnGrindCallback = callback;
        }

        public void SetFuseCallback(System.Action<int, int, int, int, int> callback) {
            OnFuseCallback = callback;
        }
        
        
        public void SetSelected(bool value) {
            toggle.isOn = value;
            gameObject.SetActive(value);
        }
        
        public void OnSelect(bool isSelect) {
            gameObject.SetActive(isSelect);
        }

        public void SetNoItemText(bool state) {
            noItemText.SetActive(state);
        }
    }
}