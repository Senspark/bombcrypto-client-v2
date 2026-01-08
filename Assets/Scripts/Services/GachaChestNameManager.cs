using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Services {
    public class GachaChestNameManager : IGachaChestNameManager {
        private readonly Dictionary<int, GachaChestNameData> _data = new();

        public GachaChestNameManager() {
            UniTask.Void(async () => {
                var res = await Resources.LoadAsync("configs/GachaChestName") as TextAsset;
                if (!res) {
                    return;
                }
                var parsed = JArray.Parse(res.text);
                var data = parsed.Select(it => new GachaChestNameData(it));
                foreach (var it in data) {
                    _data?.Add(it.ChestType, it);
                }
            });
        }

        public void Destroy() {
        }

        public string GetChestName(int chestType) {
            return _data.TryGetValue(chestType, out var value)
                ? value.ChestName
                : throw new Exception($"[{nameof(GachaChestNameManager)}] Could not find chest type: {chestType}");
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}