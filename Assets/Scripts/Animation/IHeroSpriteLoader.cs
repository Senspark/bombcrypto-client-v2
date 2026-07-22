using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Engine.Components;
using Engine.Entities;
using Senspark;
using UnityEngine;

namespace Animation {
    /// <summary>
    /// Nạp sprite hero theo <b>path/convention</b> (thay cho dict serialized trong AnimationResource).
    /// Phục vụ cả 2 nhu cầu từ <b>cùng 1 cache</b>:
    ///   - 1 frame (portrait/UI, kiểu <c>Avatar.cs</c>) qua <see cref="LoadPortrait"/>;
    ///   - cả clip (animation, kiểu <c>HeroAnimator</c>) qua <see cref="LoadClip"/> / <see cref="LoadDie"/>.
    ///
    /// <para><b>Cấp qua ServiceLocator</b> (<c>[Service]</c> + <c>Provide</c>, 1 instance dùng chung để
    /// cache không phân mảnh). Đây là <b>override có chủ đích</b> nguyên tắc "không global" cho riêng
    /// case <c>HeroAnimator</c> là MonoBehaviour (không có constructor để inject) — nhất quán codebase.
    /// Vì load qua Addressables giữ handle nên consumer <b>BẮT BUỘC</b> gọi
    /// <see cref="Release"/>/<see cref="ReleaseAll"/> khi despawn/unload để khỏi leak.</para>
    ///
    /// <para>Mô hình async↔per-frame: <see cref="Preload"/> async TRƯỚC khi spawn (cache ấm), rồi
    /// <c>HeroAnimator</c> đọc <see cref="GetClipCached"/>/<see cref="GetDieCached"/> <b>sync</b> mỗi frame.</para>
    ///
    /// Fallback màu gom 1 chỗ (requested → White → HeroTr) — xem impl.
    /// </summary>
    [Service(nameof(IHeroSpriteLoader))]
    public interface IHeroSpriteLoader : IService {
        /// <summary>1 frame portrait (icon nếu có, else <c>Front/player_front_01</c>). null nếu thiếu art.</summary>
        UniTask<Sprite> LoadPortrait(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Clip idle/move theo hướng (3 frame). Left dùng chung folder Right → consumer tự flipX.</summary>
        UniTask<Sprite[]> LoadClip(PlayerType type, PlayerColor color, FaceDirection face, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Clip die (4 frame). Rỗng nếu hero không có Die.</summary>
        UniTask<Sprite[]> LoadDie(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Đọc clip idle/move đã <see cref="Preload"/> từ cache (sync, không load). Rỗng nếu chưa ấm.</summary>
        Sprite[] GetClipCached(PlayerType type, PlayerColor color, FaceDirection face, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Đọc clip die đã <see cref="Preload"/> từ cache (sync, không load). Rỗng nếu chưa ấm.</summary>
        Sprite[] GetDieCached(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Nạp trước toàn bộ clip (Front/Back/Right + Die + portrait) của 1 hero để tránh pop-in.</summary>
        UniTask Preload(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common);

        /// <summary>Preload nhiều hero CÙNG LÚC (song song). Dùng khi biết trước danh sách hero của scene.</summary>
        UniTask PreloadMany(IEnumerable<(PlayerType type, PlayerColor color, HeroRarity rarity)> heroes);

        /// <summary>Thả handle của 1 hero (mọi rarity của cặp type+color đã nạp).</summary>
        void Release(PlayerType type, PlayerColor color);

        /// <summary>Thả toàn bộ handle (gọi khi unload scene).</summary>
        void ReleaseAll();
    }
}
