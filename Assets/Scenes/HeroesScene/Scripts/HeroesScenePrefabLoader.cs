using System;
using System.Collections.Generic;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.HeroesScene.Scripts {
    public class HeroesScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogEquipment;

        [SerializeField]
        private AssetReference dialogUpgradeStat;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogEquipment), dialogEquipment },
                { typeof(DialogUpgradeStat), dialogUpgradeStat },
            };
            base.Initialize();
        }
    }
}