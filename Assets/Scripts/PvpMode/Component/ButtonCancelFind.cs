using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Component {
    public class ButtonCancelFind : MonoBehaviour {
        [SerializeField]
        private Button buttonFind;

        [SerializeField]
        private Button buttonCancel;
        
        [SerializeField]
        private GameObject findingWait;

        public System.Action OnCancelCallback;

        public bool IsCancelOn { get; private set; } = false; 
        
        public void Show() {
            findingWait.SetActive(true);
        }

        public void Hide() {
            findingWait.SetActive(false);
            buttonCancel.gameObject.SetActive(false);
            IsCancelOn = false;
        }

        public void ShowButtonCancel() {
            buttonCancel.interactable = true;
            buttonCancel.gameObject.SetActive(true);
            IsCancelOn = true;
        }

        public void OnCancelClicked() {
            if (!buttonCancel.interactable) {
                return;
            }
            buttonCancel.interactable = false;
            OnCancelCallback?.Invoke();
        }
    }
}