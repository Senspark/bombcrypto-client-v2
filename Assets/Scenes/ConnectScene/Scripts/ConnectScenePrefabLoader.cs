using System;
using App;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Scenes.ConnectScene.Scripts {
    public class ConnectScenePrefabLoader : TemplatePrefabLoader {
        public AssetReference dialogPrefabTon;
        public AssetReference dialogPrefabSolana;
        public AssetReference dialogPrefabWebAirdrop;
        public AssetReference dialogPrefabWeb;

        private AssetReference _assetReference;
        private DialogPrefab _dialogPrefab;

        public override async void Initialize() {
            switch (AppConfig.GamePlatform) {
                case GamePlatform.TON:
                    _assetReference = dialogPrefabTon;
                    break;
                case GamePlatform.SOL:
                    _assetReference = dialogPrefabSolana;
                    break;
                case GamePlatform.WEBGL or GamePlatform.MOBILE or GamePlatform.TOURNAMENT:
                    if (AppConfig.IsWebAirdrop()) {
                        _assetReference = dialogPrefabWebAirdrop;
                    } else {
                        _assetReference = dialogPrefabWeb;
                    }
                    break;
                default:
                    throw new Exception($"{AppConfig.GamePlatform} Game platform not supported");
            }

            var obj = await AddressableLoader.LoadAsset<GameObject>(_assetReference);
            _dialogPrefab = obj.GetComponent<DialogPrefab>();
            Map = _dialogPrefab.GetAllDialogUsed();
            base.Initialize();
        }
    }
}