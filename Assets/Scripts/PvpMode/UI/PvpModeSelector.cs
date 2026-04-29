using UnityEngine;
using UnityEngine.UI;
using System;
using PvpMode.Entities;

namespace PvpMode.UI {
    public class PvpModeSelector : MonoBehaviour {
        [Serializable]
        public class ModeCard {
            public PvpGameMode mode;
            public Button button;
            public GameObject selectedOverlay;
            public Text modeNameText;
            public Text roomSizeText;
        }

        [SerializeField] private ModeCard[] modeCards;
        [SerializeField] private Toggle freeToggle;
        [SerializeField] private Toggle wagerToggle;
        [SerializeField] private GameObject wagerTiersPanel;
        [SerializeField] private Button[] wagerTierButtons;

        public event Action<PvpGameMode> OnModeSelected;
        public event Action<bool> OnWagerModeChanged;
        public event Action<int> OnWagerTierSelected;

        private PvpGameMode currentMode = PvpGameMode.FFA_1V1;
        private bool isWagered = false;
        private int currentWagerTierIndex = 0;

        private void Start() {
            foreach (var card in modeCards) {
                card.button.onClick.AddListener(() => SelectMode(card.mode));
            }

            freeToggle.onValueChanged.AddListener((val) => {
                if (val) SetWagerMode(false);
            });

            wagerToggle.onValueChanged.AddListener((val) => {
                if (val) SetWagerMode(true);
            });

            for (int i = 0; i < wagerTierButtons.Length; i++) {
                int index = i;
                wagerTierButtons[i].onClick.AddListener(() => SelectWagerTier(index));
            }

            UpdateUI();
        }

        public void SelectMode(PvpGameMode mode) {
            currentMode = mode;
            UpdateUI();
            OnModeSelected?.Invoke(mode);
        }

        private void SetWagerMode(bool wagered) {
            isWagered = wagered;
            wagerTiersPanel.SetActive(wagered);
            UpdateUI();
            OnWagerModeChanged?.Invoke(wagered);
        }

        private void SelectWagerTier(int index) {
            currentWagerTierIndex = index;
            UpdateUI();
            OnWagerTierSelected?.Invoke(index);
        }

        private void UpdateUI() {
            foreach (var card in modeCards) {
                if (card.selectedOverlay != null) {
                    card.selectedOverlay.SetActive(card.mode == currentMode);
                }
            }

            // Update wager tier buttons visuals (e.g. outline/scale)
            for (int i = 0; i < wagerTierButtons.Length; i++) {
                // Example: wagerTierButtons[i].GetComponent<Image>().color = (i == currentWagerTierIndex) ? Color.yellow : Color.white;
            }
        }

        public PvpGameMode GetCurrentMode() => currentMode;
        public bool IsWagered() => isWagered;
        public int GetCurrentWagerTierIndex() => currentWagerTierIndex;
    }
}
