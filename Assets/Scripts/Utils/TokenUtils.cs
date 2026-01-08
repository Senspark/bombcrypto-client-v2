using System.Collections.Generic;

using App;

using UnityEngine;

namespace Utils {
    public static class TokenUtils {
        private static readonly Dictionary<int, string> TokenSpriteNames = new() {
            [4] = "BcoinToken",
            [8] = "SensparkToken"
        };

        public static Sprite GetTokenSprite(int type) =>
            Resources.Load<Sprite>($"Tokens/{TokenSpriteNames[type]}");
    }
}