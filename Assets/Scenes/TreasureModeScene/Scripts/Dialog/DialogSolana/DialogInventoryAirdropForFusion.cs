using Scenes.FarmingScene.Scripts;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Dialog {
    public class DialogInventoryAirdropForFusion : DialogInventory {
        [SerializeField]
        private GameObject selectAll;

        private bool _isSelectAll = false;

        public void OnSelectAllBtn() {
            UpdateSelectAll(!_isSelectAll);
            OnSelectAllItems(_isSelectAll);
        }

        private void UpdateSelectAll(bool value) {
            _isSelectAll = value;
            selectAll.SetActive(_isSelectAll);
        }

        private void Start() {
            UpdateSelectAll(false);
            InitForFusion(UpdateSelectAll);
        }
    }
}
