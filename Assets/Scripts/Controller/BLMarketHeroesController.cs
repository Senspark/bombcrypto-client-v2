using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Constant;
using Data;
using Engine.Entities;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services.Server;
using UnityEngine;

namespace Controller {
    public enum ItemMarketSort {
        ModifyDateDesc = 0,
        ModifyDateAsc,
        PriceDesc,
        PriceAsc,
        AmountAsc,
        AmountDesc
    }

    public class BLMarketHeroesController : MonoBehaviour {
        [SerializeField]
        private BLHeroesContent content;

        [SerializeField]
        private BLBarSort barSort;

        private IMarketplace _marketplace;

        private void Awake() {
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
        }
        
        public void ClearCacheData() {
            content.ClearCacheData();
        }

        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public async Task LoadData() {
            var result = await _marketplace.GetProductHeroAsync(
                0,
                content.ItemList.MaxItems,
                (int) barSort.CurrentSort);
            var heroes = new List<UIHeroData>();
            foreach (var iter in result.Products) {
                heroes.Add(ConvertFrom(iter));
            }
            content.SetPageData(result.Quantity);
            content.SetData(heroes, true);
        }

        public async Task UpdatePageData() {
            var result = await _marketplace.GetProductHeroAsync(
                content.ItemList.CurPage * content.ItemList.MaxItems,
                content.ItemList.MaxItems,
                (int) barSort.CurrentSort);
            var heroes = new List<UIHeroData>();
            foreach (var iter in result.Products) {
                heroes.Add(ConvertFrom(iter));
            }
            content.ItemList.UpdatePage();
            content.UpdateData(heroes);
        }

        public int GetInputAmount() {
            return content.GetInputAmount();
        }
        
        public void RefreshMinPrice() {
            content.RefreshMinPrice();
        }

        private static UIHeroData ConvertFrom(ProductHeroData data) {
            var range = (1, 10);
            var speed = (1, 10);
            var count = (1, 10);
            var health = (1, 10);
            var damage = (1, 10);

            foreach (var stat in data.Stats) {
                switch (stat.StatId) {
                    case (int)StatId.Range:
                        range = (stat.Value, stat.Max);
                        break;
                    case (int)StatId.Speed:
                        speed = (stat.Value, stat.Max);
                        break;
                    case (int)StatId.Count:
                        count = (stat.Value, stat.Max);
                        break;
                    case (int)StatId.Health:
                        health = (stat.Value, stat.Max);
                        break;
                    case (int)StatId.Damage:
                        damage = (stat.Value, stat.Max);
                        break;
                }
            }

            return new UIHeroData() {
                HeroData = data,
                HeroId = data.DataBase.ItemId,
                HeroType = UIHeroData.ConvertFromHeroId(data.DataBase.ItemId),
                HeroColor = PlayerColor.HeroTr,
                HeroName = data.DataBase.ProductName,
                Quantity = data.DataBase.Quantity,
                Price = data.DataBase.Price.Value,
                RewardType = BlockRewardType.Gem,
                BombRange = range,
                Speed = speed,
                BombNum = count,
                Health = health,
                Damage = damage
            };
        }
    }
}