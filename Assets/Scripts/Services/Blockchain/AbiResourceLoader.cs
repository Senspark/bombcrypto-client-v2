using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace App {
    public static class AbiResourceLoader {
        private static readonly Dictionary<string, string> ABI = new();
        private static readonly Dictionary<string, UniTask<Object>> TASKS = new();

        public static async UniTask<string> GetAbi(string name) {
            if (ABI.TryGetValue(name, out var abi)) {
                return abi;
            }
            if (!TASKS.TryGetValue(name, out var task)) {
                TASKS[name] = task = Resources.LoadAsync<TextAsset>($"Blockchain/{name}").ToUniTask();
            }
            
            var txt = await task as TextAsset;
            var text = txt?.text ?? "{}";
            ABI[name] = abi = text;
            return abi;
        }
    }
}