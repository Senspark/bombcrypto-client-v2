using System;
using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Senspark;

using UnityEngine;

namespace App {
    public enum LandingMode {
        Treasure,
        Adventure
    }

    // Config non-secret, sửa được sau build (đọc từ StreamingAssets).
    // Độc lập hoàn toàn với AppConfig (vốn baked + chứa secret).
    // Thêm field mới: thêm property vào Data + một getter, không đụng plumbing.
    public static class RuntimeConfig {
        private const string RelativePath = "configs/runtime-config.json";

        public static bool IsLoaded { get; private set; }

        // Routing landing (luôn concrete: dùng để chọn scene FarmingScene/MainMenuScene).
        public static LandingMode Landing { get; private set; } = LandingMode.Treasure;

        // Giá trị gửi trong USER_LOGIN. null = wildcard → SvGameLoginHandler bỏ field
        // (giả lập client cũ, server collide mọi landing cùng (uid,dataType)).
        public static string LandingWire { get; private set; } = "treasure";

        public static async Task LoadAsync([NotNull] ILogManager logManager) {
            if (IsLoaded) {
                return;
            }
            try {
                // 1. URL ?landing= thắng (web; Application.absoluteURL rỗng trong Editor → bỏ qua).
                var fromUrl = ParseLandingFromUrl(Application.absoluteURL);
                if (fromUrl != null) {
                    ApplyLanding(fromUrl);
                    return;
                }
                // 2. Fallback runtime-config.json.
                var path = Path.Combine(Application.streamingAssetsPath, RelativePath);
                var json = await App.Utils.GetTextFile(logManager, path);
                var data = JsonConvert.DeserializeObject<Data>(json);
                if (data != null) {
                    ApplyLanding(data.landing);
                }
            } catch (Exception e) {
                logManager.Log($"CLIENT: RuntimeConfig load failed, use defaults: {e.Message}");
            } finally {
                IsLoaded = true;
            }
        }

        private static void ApplyLanding(string value) {
            switch (value?.Trim().ToLowerInvariant()) {
                case "adventure":
                    Landing = LandingMode.Adventure;
                    LandingWire = "adventure";
                    break;
                case "wildcard":
                case "none":
                case "null":
                    Landing = LandingMode.Treasure;
                    LandingWire = null;
                    break;
                default:
                    Landing = LandingMode.Treasure;
                    LandingWire = "treasure";
                    break;
            }
        }

        [CanBeNull]
        private static string ParseLandingFromUrl([CanBeNull] string url) {
            if (string.IsNullOrEmpty(url)) {
                return null;
            }
            var q = url.IndexOf('?');
            if (q < 0) {
                return null;
            }
            var query = url.Substring(q + 1);
            var hash = query.IndexOf('#');
            if (hash >= 0) {
                query = query.Substring(0, hash);
            }
            foreach (var pair in query.Split('&')) {
                var eq = pair.IndexOf('=');
                if (eq < 0) {
                    continue;
                }
                if (pair.Substring(0, eq) != "landing") {
                    continue;
                }
                var val = pair.Substring(eq + 1);
                return string.IsNullOrEmpty(val) ? null : val;
            }
            return null;
        }

        [Serializable]
        private class Data {
            public string landing;
        }
    }
}
