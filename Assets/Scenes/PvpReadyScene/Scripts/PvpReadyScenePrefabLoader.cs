using System;
using System.Collections.Generic;

using Scenes.MainMenuScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.PvpReadyScene.Scripts {
    public class PvpReadyScenePrefabLoader : TemplatePrefabLoader {
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
            };
            base.Initialize();
        }
    }
}