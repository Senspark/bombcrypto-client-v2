using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Engine.Components;
using Engine.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Animation {
    /// <summary>
    /// Path-load impl của <see cref="IHeroSpriteLoader"/> dùng Addressables + <see cref="HeroSpriteCatalog"/>.
    ///
    /// <para>Addressing = <b>Cách 1 (no-label)</b>: folder <c>Assets/Data/CharactersSprites</c> đã mark
    /// addressable nên address mỗi frame = full path. Loader ghép address theo convention
    /// (face → subfolder/prefix/frame-count) thay vì enumerate.</para>
    ///
    /// <para>Cache: 1 <see cref="AsyncOperationHandle{Sprite}"/> / address, chia sẻ cho cả portrait lẫn clip.
    /// Giữ handle → consumer phải <see cref="Release"/>/<see cref="ReleaseAll"/>. Chính sách mặc định:
    /// <b>scene-scoped</b> — preload khi vào scene, <see cref="ReleaseAll"/> khi unload. (Chưa làm LRU;
    /// xem open-question release-policy trong report.)</para>
    /// </summary>
    public sealed class HeroSpriteLoader : IHeroSpriteLoader {
        private const int ClipFrames = 3; // idle/move: _00.._02
        private const int DieFrames = 4;  // die: _00.._03

        private static readonly Sprite[] Empty = Array.Empty<Sprite>();

        // address → handle (1 lần load / address; dùng lại cho mọi caller).
        private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _handles = new();
        // (type,color) → các address đã nạp dưới hero này, để Release gom đúng chỗ.
        private readonly Dictionary<(PlayerType, PlayerColor), HashSet<string>> _owned = new();

        // IService lifecycle (đăng ký qua ServiceLocator).
        public Task<bool> Initialize() => Task.FromResult(true);

        public void Destroy() => ReleaseAll();

        public async UniTask<Sprite> LoadPortrait(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.TryGet(type, out var e)) {
                return null;
            }
            var owner = (type, color);
            foreach (var (c, r) in ColorPlan(e, color, rarity)) {
                if (e.HasIcon && c != PlayerColor.Skin) {
                    var icon = await LoadAddr($"{HeroSpriteCatalog.BasePath}/{e.Folder}/{ColorSeg(c, r)}/icon.png", owner);
                    if (icon) {
                        return icon;
                    }
                }
                var front = await LoadAddr(FrameAddr(e, c, r, "Front", "player_front", 1), owner);
                if (front) {
                    return front;
                }
            }
            return null;
        }

        public async UniTask<Sprite[]> LoadClip(PlayerType type, PlayerColor color, FaceDirection face, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.TryGet(type, out var e)) {
                return Empty;
            }
            var (sub, prefix) = FaceFolder(face);
            var owner = (type, color);
            foreach (var (c, r) in ColorPlan(e, color, rarity)) {
                var frames = await LoadFrames(e, c, r, sub, prefix, ClipFrames, owner);
                if (frames != null) {
                    return frames;
                }
            }
            return Empty;
        }

        public async UniTask<Sprite[]> LoadDie(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.TryGet(type, out var e) || !e.HasDie) {
                return Empty;
            }
            var owner = (type, color);
            foreach (var (c, r) in ColorPlan(e, color, rarity)) {
                var frames = await LoadFrames(e, c, r, "Die", "die", DieFrames, owner);
                if (frames != null) {
                    return frames;
                }
            }
            return Empty;
        }

        public UniTask Preload(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.Has(type)) {
                return UniTask.CompletedTask;
            }
            // Nạp 5 nhóm (3 hướng + die + portrait) SONG SONG thay vì nối tiếp → giảm round-trip/hero.
            return UniTask.WhenAll(
                Discard(LoadClip(type, color, FaceDirection.Down, rarity)),
                Discard(LoadClip(type, color, FaceDirection.Up, rarity)),
                Discard(LoadClip(type, color, FaceDirection.Right, rarity)),
                Discard(LoadDie(type, color, rarity)),
                Discard(LoadPortrait(type, color, rarity))
            );
        }

        // Preload NHIỀU hero cùng lúc (song song) — dùng khi biết trước danh sách hero của scene
        // (vd FirstInitPlayerPVE) để tránh tuần tự N hero × nhiều round-trip gây khựng.
        public UniTask PreloadMany(IEnumerable<(PlayerType type, PlayerColor color, HeroRarity rarity)> heroes) {
            var tasks = new List<UniTask>();
            foreach (var h in heroes) {
                tasks.Add(Preload(h.type, h.color, h.rarity));
            }
            return UniTask.WhenAll(tasks);
        }

        // UniTask<T> → UniTask (bỏ kết quả) để gộp vào WhenAll cùng các task khác kiểu.
        private static async UniTask Discard<T>(UniTask<T> task) => await task;

        public Sprite[] GetClipCached(PlayerType type, PlayerColor color, FaceDirection face, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.TryGet(type, out var e)) {
                return Empty;
            }
            var (sub, prefix) = FaceFolder(face);
            foreach (var (c, r) in ColorPlan(e, color, rarity)) {
                var frames = GetFramesCached(e, c, r, sub, prefix, ClipFrames);
                if (frames != null) {
                    return frames;
                }
            }
            return Empty;
        }

        public Sprite[] GetDieCached(PlayerType type, PlayerColor color, HeroRarity rarity = HeroRarity.Common) {
            if (!HeroSpriteCatalog.TryGet(type, out var e) || !e.HasDie) {
                return Empty;
            }
            foreach (var (c, r) in ColorPlan(e, color, rarity)) {
                var frames = GetFramesCached(e, c, r, "Die", "die", DieFrames);
                if (frames != null) {
                    return frames;
                }
            }
            return Empty;
        }

        public void Release(PlayerType type, PlayerColor color) {
            var owner = (type, color);
            if (!_owned.TryGetValue(owner, out var set)) {
                return;
            }
            foreach (var addr in set) {
                if (_handles.TryGetValue(addr, out var h)) {
                    if (h.IsValid()) {
                        Addressables.Release(h);
                    }
                    _handles.Remove(addr);
                }
            }
            _owned.Remove(owner);
        }

        public void ReleaseAll() {
            foreach (var h in _handles.Values) {
                if (h.IsValid()) {
                    Addressables.Release(h);
                }
            }
            _handles.Clear();
            _owned.Clear();
        }


        // Thứ tự thử màu, gom fallback 1 chỗ: requested → White → HeroTr (Skin có nhánh riêng).
        private static IEnumerable<(PlayerColor color, HeroRarity rarity)> ColorPlan(
            HeroSpriteCatalog.Entry e, PlayerColor requested, HeroRarity rarity) {
            if (requested == PlayerColor.Skin) {
                if (e.HasSkin) {
                    yield return (PlayerColor.Skin, rarity);
                }
                yield return (e.DefaultColor, rarity);
                if (e.DefaultColor != PlayerColor.HeroTr && e.Supports(PlayerColor.HeroTr)) {
                    yield return (PlayerColor.HeroTr, rarity);
                }
                yield break;
            }

            var resolved = e.Supports(requested) ? requested : e.DefaultColor;
            yield return (resolved, rarity);
            if (resolved != PlayerColor.White && e.Supports(PlayerColor.White)) {
                yield return (PlayerColor.White, rarity);
            }
            if (resolved != PlayerColor.HeroTr && e.Supports(PlayerColor.HeroTr)) {
                yield return (PlayerColor.HeroTr, rarity);
            }
        }

        // Nạp 1 cụm frame; null nếu frame[0] thiếu (→ caller fallback màu khác).
        private async UniTask<Sprite[]> LoadFrames(HeroSpriteCatalog.Entry e, PlayerColor color, HeroRarity rarity,
            string sub, string prefix, int count, (PlayerType, PlayerColor) owner) {
            var first = await LoadAddr(FrameAddr(e, color, rarity, sub, prefix, 0), owner);
            if (!first) {
                return null;
            }
            var frames = new Sprite[count];
            frames[0] = first;
            for (var i = 1; i < count; i++) {
                frames[i] = await LoadAddr(FrameAddr(e, color, rarity, sub, prefix, i), owner);
            }
            return frames;
        }

        // Đọc 1 cụm frame từ cache đã preload; null nếu thiếu bất kỳ frame nào (→ caller fallback màu khác).
        private Sprite[] GetFramesCached(HeroSpriteCatalog.Entry e, PlayerColor color, HeroRarity rarity,
            string sub, string prefix, int count) {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++) {
                var addr = FrameAddr(e, color, rarity, sub, prefix, i);
                if (!_handles.TryGetValue(addr, out var h) || !h.IsDone
                    || h.Status != AsyncOperationStatus.Succeeded || !h.Result) {
                    return null;
                }
                frames[i] = h.Result;
            }
            return frames;
        }

        private async UniTask<Sprite> LoadAddr(string addr, (PlayerType, PlayerColor) owner) {
            if (_handles.TryGetValue(addr, out var existing)) {
                if (!existing.IsDone) {
                    await existing.Task;
                }
                return existing.Status == AsyncOperationStatus.Succeeded ? existing.Result : null;
            }

            var handle = Addressables.LoadAssetAsync<Sprite>(addr);
            _handles[addr] = handle;
            Track(owner, addr);

            await handle.Task;
            if (handle.Status != AsyncOperationStatus.Succeeded || !handle.Result) {
                // miss: thả handle hỏng + bỏ cache để fallback path khác thử lại.
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
                _handles.Remove(addr);
                Untrack(owner, addr);
                return null;
            }

            var spr = handle.Result;
            if (spr.texture) {
                spr.texture.filterMode = FilterMode.Point;
            }
            return spr;
        }

        private void Track((PlayerType, PlayerColor) owner, string addr) {
            if (!_owned.TryGetValue(owner, out var set)) {
                set = new HashSet<string>();
                _owned[owner] = set;
            }
            set.Add(addr);
        }

        private void Untrack((PlayerType, PlayerColor) owner, string addr) {
            if (_owned.TryGetValue(owner, out var set)) {
                set.Remove(addr);
            }
        }

        private static (string sub, string prefix) FaceFolder(FaceDirection face) => face switch {
            FaceDirection.Up => ("Back", "player_back"),
            FaceDirection.Down => ("Front", "player_front"),
            _ => ("Right", "player"), // Left dùng chung folder Right (consumer flipX)
        };

        private static string ColorSeg(PlayerColor color, HeroRarity rarity) =>
            color == PlayerColor.Skin ? $"Skin/{rarity}" : color.ToString();

        private static string FrameAddr(HeroSpriteCatalog.Entry e, PlayerColor color, HeroRarity rarity,
            string sub, string prefix, int n) =>
            $"{HeroSpriteCatalog.BasePath}/{e.Folder}/{ColorSeg(color, rarity)}/{sub}/{prefix}_{n:D2}.png";
    }
}
