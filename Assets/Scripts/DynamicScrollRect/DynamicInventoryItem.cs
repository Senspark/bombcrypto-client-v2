using System.Collections.Generic;

using App;

using Scenes.FarmingScene.Scripts;
using Cysharp.Threading.Tasks;

using UnityEngine;

namespace DynamicScrollRect {
    public class DynamicInventoryItem : MonoBehaviour {
        [SerializeField]
        private InventoryItem item;
        public InventoryItem Item => item;
        
        public class DynamicInfo {
            public PlayerData Player;
            public InventoryItem.InventoryItemCallback Callback;
            public List<PlayerData> HeroesId;
            public DialogInventory.ChooseMode ChooseMode;
            public bool ShowBattery;
            public bool IsClicked;
            public Canvas Canvas;
            public HeroDetailsDisplay HeroDetailsDisplay;
        }
        public DynamicInfo Info { get; private set; }
        
        public async UniTask<List<PlayerData>> SetInfo(PlayerData player, InventoryItem.InventoryItemCallback callback, 
            DialogInventory.ChooseMode chooseMode,
            List<PlayerData> heroesId, bool showBattery = false, bool isClicked = false, Canvas canvas = null,
            HeroDetailsDisplay heroDetailsDisplay = null) {
            Info = new DynamicInfo() {
                Player = player,
                Callback = callback,
                HeroesId = heroesId,
                ChooseMode = chooseMode,
                ShowBattery = showBattery,
                IsClicked = isClicked,
                Canvas = canvas,
                HeroDetailsDisplay = heroDetailsDisplay
            };
            await SetItemInfo(Info);
            return heroesId;
        }

        private async UniTask SetItemInfo(DynamicInfo info) {
            info.HeroesId = await Item.SetInfo(
                info.Player,
                info.Callback,
                info.ChooseMode,
                info.HeroesId,
                info.ShowBattery,
                info.IsClicked,
                info.Canvas,
                info.HeroDetailsDisplay
            );
        }
        
        public void UpdateDynamicInfo(DynamicInventoryItem other) {
            SetItemInfo(other.Info);
        }        

        public void UpdateUILockHero(PlayerData player) {
            item.UpdateUILockHero(player);
        }

        public void SetHighLight(bool value) {
            item.SetHighLight(value);
        }
        
        public void UpdateLockedHeroes(bool state) {
            item.UpdateLockedHeroes(state);
        }
    }
}
