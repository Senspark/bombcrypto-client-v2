using UnityEngine;

namespace Game.UI.FrameTabScroll {
    public class SlotBase <TabMenu> : MonoBehaviour
    {
        [SerializeField]
        protected TabMenu typeMenu;
        
        [SerializeField]
        public GameObject frameContent;

        public TabMenu TypeMenu => typeMenu;
        
        protected virtual void Awake() {
            this.frameContent.SetActive(false);
        }

        public void ShowContent() {
            this.frameContent.SetActive(true);
        }

        public void HideContent() {
            this.frameContent.SetActive(false);
        }
    }
}
