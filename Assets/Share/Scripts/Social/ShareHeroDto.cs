using System.Collections.Generic;

using App;

namespace Share.Scripts.Social {
    /// <summary>
    /// Payload gửi sang React qua tag SHARE_ON_X. Là data-bag thuần — host nào mở dialog thì host đó
    /// dựng payload qua factory (ForHeroes / ForToken). Nút ShareOnXButton chỉ đóng dấu ảnh + gửi,
    /// KHÔNG biết PlayerData/TokenData → thêm share type mới không đụng tới nút.
    /// React tự map rarity/skin int -> tên (Quyết định A). KHÔNG gửi tên từ Unity.
    /// </summary>
    public class SharePayload {
        public string image;            // base64 JPG, không kèm tiền tố "data:"
        public string source;           // "new_hero" | "summary" | "token_reward" | "collection"
        public List<long[]> heroes;     // hero sources: mỗi hero = [heroId, rarity, skin]
        public string token;            // token_reward: tên token hiển thị (data.displayName)
        public double amount;           // token_reward: số lượng claim được

        public static SharePayload ForHeroes(string source, IReadOnlyList<PlayerData> heroes) {
            var list = new List<long[]>();
            if (heroes != null) {
                foreach (var p in heroes) {
                    if (p != null) {
                        list.Add(FromPlayerData(p));
                    }
                }
            }
            return new SharePayload { source = source, heroes = list };
        }

        public static SharePayload ForToken(string token, double amount) {
            return new SharePayload { source = "token_reward", token = token, amount = amount };
        }

        // Khoe collection BHero đang sở hữu: caption tĩnh bên React -> không cần gửi danh sách hero,
        // ảnh chụp lưới inventory là thứ duy nhất cần khoe.
        public static SharePayload ForCollection() {
            return new SharePayload { source = "collection" };
        }

        private static long[] FromPlayerData(PlayerData p) {
            return new[] { (long)p.heroId.Id, (long)p.rare, (long)(int)p.playerType };
        }
    }
}
