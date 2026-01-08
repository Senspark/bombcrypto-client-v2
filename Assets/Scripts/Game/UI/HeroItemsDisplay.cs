using System;
using System.Collections.Generic;

using App;

using Senspark;

using Scenes.FarmingScene.Scripts;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class HeroItemsDisplay : MonoBehaviour {
        [SerializeField]
        private InventoryItem inventoryItemPrefab;

        [SerializeField]
        private ScrollRect scroller;
        
        private int _targetNeedUpgrade;
        private List<InventoryItem> _items;
        private Action<InventoryItem> _callBackDisplayHeroItem;

        private IPlayerStorageManager _playerStore;
        private List<PlayerData> _heroesIdBurn = new();

        private void Start() {
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
        }

        public void Init(int targetNeedUpgrade, Action<InventoryItem> displayHeroItem) {
            _targetNeedUpgrade = targetNeedUpgrade;
            _callBackDisplayHeroItem = displayHeroItem;
            ShowHeroLevelBelowTarget(1);
        }

        private async void ShowHeroLevelBelowTarget(int levelBelow) {
            var players = _playerStore.GetPlayerDataList(HeroAccountType.Nft);
            var container = scroller.content.transform;
            InventoryItem.InventoryItemCallback inventoryItemCallback = default;
            inventoryItemCallback.OnClicked = _callBackDisplayHeroItem;

            foreach (var player in players) {
                if (player.rare == _targetNeedUpgrade - levelBelow) {
                    var item = Instantiate(inventoryItemPrefab, container, false);
                    _heroesIdBurn = await item.SetInfo(player, inventoryItemCallback, DialogInventory.ChooseMode.InventoryBurn,
                        _heroesIdBurn, false, true);
                    _items.Add(item);
                }
            }
        }

    }
}
