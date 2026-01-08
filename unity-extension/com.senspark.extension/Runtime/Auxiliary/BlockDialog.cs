using System;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark {
    [AddComponentMenu("Senspark/Block Dialog")]
    public class BlockDialog : MonoBehaviour {
        [MustUseReturnValue]
        public static BlockDialog Show() {
            var key = $"Senspark/Prefabs/Auxiliary/{nameof(BlockDialog)}";
            var prefab = Resources.Load<BlockDialog>(key);
            var dialog = Instantiate(prefab);
            return dialog;
        }

        [MustUseReturnValue]
        public static async UniTask<T> Block<T>([NotNull] Func<UniTask<T>> action) {
            var dialog = Show();
            try {
                var result = await action();
                return result;
            } finally {
                dialog.Hide();
            }
        }

        [MustUseReturnValue]
        public static T Block<T>([NotNull] Func<T> action) {
            var dialog = Show();
            try {
                var result = action();
                return result;
            } finally {
                dialog.Hide();
            }
        }

        public void Hide() {
            Destroy(gameObject);
        }
    }
}