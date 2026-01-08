using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Engine.Input {
    public class InputCacheImage {
        private readonly Dictionary<string, Sprite> _cacheImage = new();
        
        public async UniTask<Sprite> GetImage(string name) {
            if (!_cacheImage.TryGetValue(name, out var image)) {
                try {
                    var spr = await AddressableLoader.LoadAsset<Sprite>(name);
                    if(spr != null) {
                        _cacheImage.TryAdd(name, spr);
                    }
                    return spr;
                }
                catch {
                    Debug.LogError($"Can't load image {name}");
                    return null;
                }
               
            }
            return image;
        }
    }
}