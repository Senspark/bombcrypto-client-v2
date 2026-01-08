using System;
using System.Collections.Generic;

using Scenes.ShopScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.InventoryScene.Scripts {
    public class InventoryScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogChestInfo;

        [SerializeField]
        private AssetReference dialogHeroSell;

        [SerializeField]
        private AssetReference dialogItemEdit;

        [SerializeField]
        private AssetReference dialogItemSell;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogChestInfo), dialogChestInfo },
                { typeof(DialogHeroSell), dialogHeroSell},
                { typeof(DialogItemEdit), dialogItemEdit},
                { typeof(DialogItemSell), dialogItemSell}
            };
            base.Initialize();
        }
    }
}