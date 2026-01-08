using System;
using System.Collections.Generic;

using Scenes.HeroesScene.Scripts;
using Scenes.MainMenuScene.Scripts;
using Scenes.MarketplaceScene.Scripts;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.AdventureMenuScene.Scripts {
    public class AdventureMenuScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogAdventureHero;

        [SerializeField]
        private AssetReference dialogEquipment;

        [SerializeField]
        private AssetReference dialogNotificationBoosterToMarket;

        [SerializeField]
        private AssetReference dialogHeroSelection;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogAdventureHero), dialogAdventureHero },
                { typeof(DialogEquipment), dialogEquipment },
                { typeof(DialogNotificationBoosterToMarket), dialogNotificationBoosterToMarket },
                { typeof(DialogHeroSelection), dialogHeroSelection }
            };
            base.Initialize();
        }
    }
}