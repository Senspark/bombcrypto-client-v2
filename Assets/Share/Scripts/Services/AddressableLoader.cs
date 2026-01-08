using Cysharp.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

//Support load addressable dễ dàng
public static class AddressableLoader {
    
    #region Load 1 asset

    /// <summary>
    /// Load tài nguyên lên bằng asset reference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetReference"></param>
    /// <returns></returns>
    public static async UniTask<T> LoadAsset<T>(AssetReference assetReference) {
        AsyncOperationHandle<T> handle = assetReference.LoadAssetAsync<T>();

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            return handle.Result;
        }
        #if UNITY_EDITOR
        Debug.LogError("Failed to load resource: " + assetReference);
        #endif
        return default;
    }

    /// <summary>
    /// Load tài nguyên lên bằng asset label
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetLabelReference"></param>
    /// <returns></returns>
    public static async UniTask<T> LoadAsset<T>(AssetLabelReference assetLabelReference) {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetLabelReference);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            return handle.Result;
        }
#if UNITY_EDITOR
        Debug.LogError("Failed to load resource: " + assetLabelReference);
#endif
        return default;
    }

    /// <summary>
    /// Load tài nguyên bằng dường dẫn
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reference"></param>
    /// <returns></returns>
    public static async UniTask<T> LoadAsset<T>(string reference) {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(reference);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            return handle.Result;
        }
#if UNITY_EDITOR
        Debug.LogError("Failed to load resource: " + reference);
#endif
        return default;
    }

    #endregion

    #region Load nhiều asset
    
    
    /// <summary>
    /// Load tất cả tài nguyên bằng asset label
    /// </summary>
    /// <param name="assetLabelString"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async UniTask<List<T>> LoadAllAssets<T>(string assetLabelString) {
        var label = new AssetLabelReference {
            labelString = assetLabelString
        };
        return await LoadAllAssets<T>(label);
    }

    public static async UniTask<List<T>> LoadAllAssets<T>(string[] assetLabelStrings) {
        List<AssetLabelReference> listLabel = new System.Collections.Generic.List<AssetLabelReference>();
        foreach (var label in assetLabelStrings) {
            listLabel.Add(new AssetLabelReference {
                labelString = label
            });
        }

        return await LoadAllAssets<T>(listLabel);
    }

    /// <summary>
    /// Load tất cả tài nguyên bằng asset label
    /// </summary>
    /// <param name="assetLabelReference"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async UniTask<List<T>> LoadAllAssets<T>(AssetLabelReference assetLabelReference) {
        var handle = Addressables.LoadAssetsAsync<T>(assetLabelReference, null);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            return handle.Result.ToList();
        }
#if UNITY_EDITOR
        Debug.LogError("Failed to load resource: " + assetLabelReference);
#endif
        return default;
    }

    /// <summary>
    /// Load tất cả tài nguyên bằng list các asset label
    /// </summary>
    /// <param name="assetLabelReferences"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async UniTask<List<T>> LoadAllAssets<T>(List<AssetLabelReference> assetLabelReferences) {
        var results = new List<T>();

        foreach (var assetLabelReference in assetLabelReferences) {
            var handle = Addressables.LoadAssetsAsync<T>(assetLabelReference, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                results.AddRange(handle.Result);
            } else {
#if UNITY_EDITOR
                Debug.LogError("Failed to load resources: " + assetLabelReference);
#endif
            }
        }

        return results;
    }

    /// <summary>
    /// Load tất cả tài nguyên bằng list các asset path
    /// </summary>
    /// <param name="assetPaths"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async UniTask<List<T>> LoadAllAssets<T>(List<string> assetPaths) {
        var results = new List<T>();

        foreach (var path in assetPaths) {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(path);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                results.Add(handle.Result);
            } else {
#if UNITY_EDITOR
                Debug.LogError("Failed to load resource: " + path);
#endif
            }
        }

        return results;
    }

    public static async UniTask<List<T>> LoadAllAssets<T>(List<AssetReference> assetReferences) {
        var results = new List<T>();

        foreach (var assetReference in assetReferences) {
            var handle = assetReference.LoadAssetAsync<T>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                results.Add(handle.Result);
            } else {
#if UNITY_EDITOR
                Debug.LogError("Failed to load resource: " + assetReference);
#endif
            }
        }

        return results;
    }

    #endregion
}