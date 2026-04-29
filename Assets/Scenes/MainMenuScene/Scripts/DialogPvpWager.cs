using System;
using System.Collections.Generic;
using Constant;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainMenuScene.Scripts {
    public class DialogPvpWager : Dialog {
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnPlay;
        [SerializeField] private ToggleGroup tokenToggleGroup;
        [SerializeField] private ToggleGroup tierToggleGroup;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtFeeNote;

        private Action<PvpWagerToken, PvpWagerTier> _onConfirm;
        private PvpWagerToken _selectedToken = PvpWagerToken.BCOIN_BSC;
        private PvpWagerTier _selectedTier = PvpWagerTier.TIER_1;

        public static async UniTask<DialogPvpWager> Create() {
            var loader = ServiceLocator.Instance.Resolve<IPrefabLoaderManager>();
            var prefab = await loader.LoadAsync<GameObject>("DialogPvpWager");
            return Instantiate(prefab).GetComponent<DialogPvpWager>();
        }

        private void Awake() {
            btnClose.onClick.AddListener(Hide);
            btnPlay.onClick.AddListener(OnPlayClick);
            txtTitle.text = "SELECT PVP WAGER";
            txtFeeNote.text = "* A 5% fee will be deducted from the total prize pool.";
        }

        public void Init(Action<PvpWagerToken, PvpWagerTier> onConfirm) {
            _onConfirm = onConfirm;
        }

        private void OnPlayClick() {
            // In a real implementation, we would read the values from the toggles
            // For now, we use the default or selected values
            _onConfirm?.Invoke(_selectedToken, _selectedTier);
            Hide();
        }

        public void SelectToken(int tokenId) {
            _selectedToken = (PvpWagerToken)tokenId;
        }

        public void SelectTier(int amount) {
            _selectedTier = (PvpWagerTier)amount;
        }
    }
}
