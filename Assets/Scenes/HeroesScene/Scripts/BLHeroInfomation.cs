using System.Collections.Generic;
using App;
using BomberLand.Component;
using Constant;
using Data;
using Game.Dialog.BomberLand;
using Senspark;
using Services;
using UnityEngine;

namespace Game.Dialog {
    public class BLHeroInfomation : BLHeroStats {
        [SerializeField]
        private  AnimationAvatar hero;
        
        private IHeroStatsManager _heroStatsManager;
        private IProductItemManager _productItemManager;
        private System.Action _onHeroClickedCallback;

        private void Awake() {
            _heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            hero.SetClickedCallback(OnHeroClicked);
        }

        public void SetHeroClickedCallback(System.Action callback) {
            _onHeroClickedCallback = callback;
        }

        public void UpdateHero(PlayerData data, ProductItemData productItem) {
            hero.ShowHero(data, productItem);
        }
        
        public void UpdateHero(PlayerData data) {
            if (data == null) {
                hero.HideHero();
                SetEmptyStats();
                return;
            }

            var productItem = _productItemManager.GetItem(data.itemId);
            hero.ShowHero(data, productItem);
            data.MaximumStats = new Dictionary<StatId, int>() {
                {StatId.Range, 10},
                {StatId.Speed, 10},
                {StatId.Count, 10},
                {StatId.Health, 10},
                {StatId.Damage, 10}
            };

            var stats = _heroStatsManager.GetStats(data.itemId);
            UpdateHeroFromStatDataWithUpgrade(stats, data.UpgradeStats);
        }

        public void ShowWing(int itemId) {
            hero.ShowWing(itemId);
        }

        private void OnHeroClicked() {
            _onHeroClickedCallback?.Invoke();
        }
    }
}