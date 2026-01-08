using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Share.Scripts.PrefabsManager {
    public class SharedPrefabLoader : TemplatePrefabLoader {
        public AssetReference dialogWaiting;
        public AssetReference dialogOk;
        public AssetReference dialogError;
        public AssetReference dialogErrorDetails;
        public AssetReference dialogInformation;
        public AssetReference dialogConfirm;

        public AssetReference dialogNotificationToShop;
        public AssetReference dialogNotificationToMarket;
        public AssetReference dialogNotificationToAltar;
        public AssetReference dialogNotificationBoosterToMarket;
        
        private const string SharePrefabLoaderName = "SharePrefabLoader";
        private const string SharePrefabLoaderNameTon = "SharePrefabLoaderTon";
        private const string SharePrefabLoaderNameSol = "SharePrefabLoaderSol";
        private const string SharePrefabLoaderNameAirdrop = "SharePrefabLoaderAirdrop";

        public static async UniTask<SharedPrefabLoader> Load() {
            string path;
            switch (AppConfig.GamePlatform) {
                case GamePlatform.TON:
                    path = SharePrefabLoaderNameTon;
                    break;
                case GamePlatform.SOL:
                    path = SharePrefabLoaderNameSol;
                    break;
                default:
                    path = SharePrefabLoaderName;
                    break;
            }
            //DevHoang: No need this because load before user choose to bsc/pol or ronin
            // if (AppConfig.IsRonin()) {
            //     path = SharePrefabLoaderNameAirdrop;
            // }
            // how  to know SharePrefabLoaderName exist or not?
            var o = Addressables.LoadAssetAsync<GameObject>(path);
            var t = await o.Task;
            if (o.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed) {
                throw new Exception($"SharedPrefabLoader: No asset reference found for {path}");
            }

            var prefabLoaderObj = Object.Instantiate(t);
            var prefabLoader = prefabLoaderObj.GetComponent<SharedPrefabLoader>();
            prefabLoader.Initialize();
            return prefabLoader;
        }

        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference> {
                {typeof(DialogWaiting), dialogWaiting},
                {typeof(DialogOK), dialogOk},
                {typeof(DialogError), dialogError},
                {typeof(DialogErrorDetails), dialogErrorDetails},
                {typeof(DialogInformation), dialogInformation},
                {typeof(DialogConfirm), dialogConfirm},
                {typeof(DialogNotificationToShop), dialogNotificationToShop},
                {typeof(DialogNotificationToMarket), dialogNotificationToMarket},
                {typeof(DialogNotificationToAltar), dialogNotificationToAltar},
                {typeof(DialogNotificationBoosterToMarket), dialogNotificationBoosterToMarket}
            };
            base.Initialize();
        }
    }

    public class SharePrefabLoaderWrapper : IPrefabLoader {
        private Dictionary<Type, AssetReference> _map;

        public async UniTask<IPrefabLoader> Load() {
            var r = await SharedPrefabLoader.Load();
            await UniTask.WaitUntil(() => r.Map != null);
            _map = r.Map;
            return this;
        }

        public void Initialize() {
        }

        public bool Contains<T>() where T : MonoBehaviour {
            return _map.ContainsKey(typeof(T));
        }

        public async UniTask<T> Instantiate<T>() where T : MonoBehaviour {
            if (!_map.TryGetValue(typeof(T), out var assetReference)) {
                throw new Exception($"PrefabLoader: No asset reference found for type {typeof(T)}");
            }
            var handle = assetReference.InstantiateAsync();
            await handle.Task;
            return handle.Result.GetComponent<T>();
        }
    }
}