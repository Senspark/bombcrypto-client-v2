using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using App;

using Senspark;

using JetBrains.Annotations;

using Newtonsoft.Json;

using UnityEngine;

namespace Services.RemoteConfig {
    [Serializable]
    public class InterstitialConfig {
        public CategoryData[] categories;
        public int secondBetween;

        [Serializable]
        public class CategoryData {
            public string category;
            public bool disable;
        }
    }

    public class MainMenuConfig {
        public readonly ModeType Mode;

        public enum ModeType {
            Default,
            PvpOnly
        }

        [JsonConstructor]
        public MainMenuConfig(string mode) {
            if (Enum.TryParse(mode, out ModeType result)) {
                Mode = result;
            }
        }

        public MainMenuConfig() {
            Mode = ModeType.Default;
        }
    }

    public static class RemoteConfigHelper {
        private const string REMOTE_CONFIG_DEFAULT_VALUES_FILE = "remote_config_defaults.json";

        private static class RemoteKey {
            public const string CaptchaMinutes = "captcha_minutes";
            public const string Interstitial = "interstitial";
            public const string MainMenuConfig = "main_menu_config";
            public const string EnableTutorial = "enable_tutorial";
            public const string DefaultMode = "default_mode";
        }

        public static async Task<Dictionary<string, object>> ReadRemoteConfigDefaultValues(ILogManager logManager) {
            try {
                var path = Path.Combine(Application.streamingAssetsPath, REMOTE_CONFIG_DEFAULT_VALUES_FILE);
                var raw = await App.Utils.GetTextFile(logManager, path);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                return data;
            } catch (Exception e) {
                logManager.Log(e.Message);
                return new Dictionary<string, object>();
            }
        }

        [CanBeNull]
        public static InterstitialConfig GetInterstitialConfig(IRemoteConfig remoteConfig) {
            try {
                var raw = remoteConfig.GetString(RemoteKey.Interstitial);
                var data = JsonConvert.DeserializeObject<InterstitialConfig>(raw);
                return data;
            } catch (Exception e) {
                return null;
            }
        }

        [NotNull]
        public static MainMenuConfig GetMainMenuConfig(IRemoteConfig remoteConfig) {
            try {
                var raw = remoteConfig.GetString(RemoteKey.MainMenuConfig);
                var data = JsonConvert.DeserializeObject<MainMenuConfig>(raw);
                return data;
            } catch (Exception e) {
                return new MainMenuConfig();
            }
        }
        
        [NotNull]
        public static bool IsEnableTutorial(IRemoteConfig remoteConfig) {
            return remoteConfig.GetBool (RemoteKey.EnableTutorial);
        }

        public static GameModeType GetGameMode(IRemoteConfig remoteConfig) {
            var defaultMode = remoteConfig.GetInt(RemoteKey.DefaultMode);
            return defaultMode switch {
                0 => GameModeType.PvpMode,
                1 => GameModeType.StoryMode,
                _ => GameModeType.PvpMode
            };
        }
   }
}