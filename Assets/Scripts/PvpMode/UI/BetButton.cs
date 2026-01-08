using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class BetButton : MonoBehaviour {
        [SerializeField]
        private Text rewardText;
        
        [SerializeField]
        private Text ticketText;
        
        [SerializeField]
        private Image tick;
        
        [SerializeField]
        private GameObject locked;

        [SerializeField]
        private Button button;

        public void SetRewardAndTicket(float reward, int ticket) {
            rewardText.text = $"+{reward:0.##}";
            ticketText.text = $"x{ticket}";
        }

        public void SetActive(bool value) {
            tick.gameObject.SetActive(value);
        }

        public void SetLock(bool value) {
            locked.SetActive(value);
            button.interactable = !value;
            if (value) {
                SetActive(false);
            }
        }
        
        public bool GetActive() {
            return tick.gameObject.activeSelf;
        }
    }
}
