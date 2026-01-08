using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using Data;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLAltarFuseController : MonoBehaviour {
        [SerializeField]
        private BLAltarFuseContent content;
        
        [SerializeField]
        private Button showMoreButton;

        private IServerRequester _serverRequester;
        private ConfigUpgradeCrystalData[] _configs;
        public Action OnShowDialogInfo { set; private get; }
        
        private void Awake() {
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
        }

        public async Task LoadData() {
            content.SetEnableShowMore(EnableShowMore);
            var crystals = await _serverRequester.GetCrystal();
            var upgradeConfig = await _serverRequester.GetUpgradeConfig();
            _configs = upgradeConfig.Crystals;
            var result = new List<IUiCrystalData>();
            foreach (var crystal  in crystals) {
                if (crystal.Quantity <= 0) {
                    continue;
                }
                var config = GetConfig(crystal.ItemId);
                if (config != null) {
                    result.Add(new UiCrystalData(crystal, config));
                }
            }
            content.SetData(result);
        }

        private ConfigUpgradeCrystalData GetConfig(int itemId) {
            return _configs.FirstOrDefault(iter => iter.SourceItemID == itemId);
        }
        
        public void OnInfoButtonClicked() {
            OnShowDialogInfo?.Invoke();
        }
        
        private void EnableShowMore(bool state) {
            showMoreButton.gameObject.SetActive(state);
        }

        public void OnShowMoreButtonClicked() {
            content.UpdatePage();
        }
    }
}