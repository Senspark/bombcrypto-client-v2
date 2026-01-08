using System;
using System.Collections.Generic;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.MarketplaceScene.Scripts {
    public class MarketplaceScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogHeroBuy;

        [SerializeField]
        private AssetReference dialogItemBuy;
        [SerializeField]
        private AssetReference dialogOrderItemMarket;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogHeroBuy), dialogHeroBuy },
                { typeof(DialogItemBuy), dialogItemBuy},
                { typeof(DialogOrderItemMarket), dialogOrderItemMarket},
            };
            base.Initialize();
        }
    }
}