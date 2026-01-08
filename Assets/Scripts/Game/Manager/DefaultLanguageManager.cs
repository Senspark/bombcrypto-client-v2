using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App.Language;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace App {
    public class DefaultLanguageManager : ObserverManager<LanguageObserver>, ILanguageManager {
        private const string ENUM_CLASS_NAME = "LocalizeKey";
        private static readonly string ENUM_CLASS_PATH = $"{Application.dataPath}/Scripts/Utils/";
        private const string PLAYER_PREF_LANGUAGE_ID = "LanguageIndex";
        private readonly NetworkType _networkType;
        private readonly Dictionary<LocalizeKey, string> _dictionary = new();
        public LocalizeLang CurrentLanguage { get; private set; } = LocalizeLang.English;
        public string CurrentLanguageSymbol => LocalizeLangToFileName(CurrentLanguage);

        #region PUBLIC METHODS

        public DefaultLanguageManager() {
            _networkType = NetworkType.Polygon;
            var index = PlayerPrefs.GetInt(PLAYER_PREF_LANGUAGE_ID, 0);
            UniTask.Void(async () => {
                var lang = Parse(index);
                SetLanguage(lang, false);
                await UniTask.Yield();
            });
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void SetLanguage(LocalizeLang lang, bool dispatch = true) {
            var dict = GetLocalizeData(_networkType, lang);
            if (dict == null) {
                return;
            }
            CurrentLanguage = lang;
            PlayerPrefs.SetInt(PLAYER_PREF_LANGUAGE_ID, (int)lang);
            PlayerPrefs.Save();
            _dictionary.Clear();

            var englishDict = GetLocalizeData(_networkType, LocalizeLang.English);
            SetDictionary(dict, englishDict, _dictionary);
            if (dispatch) {
                DispatchEvent(e => e.OnLanguageChanged?.Invoke());
            }
        }

        public string GetValue(LocalizeKey key) {
            return _dictionary.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string GetValue(string key) {
            return Enum.TryParse(key, out LocalizeKey @enum) ? GetValue(@enum) : key;
        }

        private static void SetDictionary(Dictionary<string, string> source,
            Dictionary<string, string> defaultSource, Dictionary<LocalizeKey, string> target) {
            var t = typeof(LocalizeKey);
            foreach (var kv in source) {
                if (Enum.IsDefined(t, kv.Key)) {
                    var k = (LocalizeKey)Enum.Parse(t, kv.Key);
                    var str = kv.Value;
                    if (string.IsNullOrWhiteSpace(str)) {
                        str = defaultSource[kv.Key];
                    }
                    target.Add(k, kv.Value ?? str);
                }
            }
        }

        private static Dictionary<string, string> GetLocalizeData(NetworkType networkType, LocalizeLang lang) {
            try {
                var obj = Resources.Load<LanguageManagerScriptableData>("configs/LanguageManager");
                var asset = obj
                    .data.FirstOrDefault(e => e.networkType == networkType)
                    .languagePaths.FirstOrDefault(e => e.languageType == lang)
                    .jsonFile;
                var json = asset.text;
                return json == null ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            } catch (Exception e) {
                Debug.Log(e.Message);
                return null;
            }
        }

        private static string LocalizeLangToFileName(LocalizeLang lang) {
            return lang switch {
                LocalizeLang.English => "en",
                LocalizeLang.VietNam => "vi",
                LocalizeLang.Philippine => "ph",
                LocalizeLang.Brazil => "br",
                LocalizeLang.Spanish => "es",
                LocalizeLang.Thai => "th",
                _ => string.Empty
            };
        }

        private static LocalizeLang Parse(int languageIndex) {
            if (!Enum.IsDefined(typeof(LocalizeLang), languageIndex)) {
                return LocalizeLang.English;
            }

            return (LocalizeLang)languageIndex;
        }

        #endregion

#if UNITY_EDITOR
        [MenuItem("Tools/Regenerate Localize Enum")]
        public static void _ReGenerateEnum() {
            var sb = new StringBuilder();

            sb.AppendLine("[System.Serializable]");
            sb.AppendLine($"public enum {ENUM_CLASS_NAME} {{");

            var data = GetLocalizeData(NetworkType.Binance, LocalizeLang.English);
            if (data == null) {
                return;
            }

            sb.AppendLine($"\tNONE,"); // default Key

            foreach (var key in data.Keys) {
                sb.AppendLine($"\t{key},");
            }

            sb.AppendLine("}");

            var filePath = Path.Combine(ENUM_CLASS_PATH, $"{ENUM_CLASS_NAME}.cs");
            using TextWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));
            writer.Write(sb.ToString());
            writer.Flush();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Change Language To English")]
        public static void _ChangeLanguageToEnglish() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.English);
        }

        [MenuItem("Tools/Change Language To VietNam")]
        public static void _ChangeLanguageToVietNam() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.VietNam);
        }

        [MenuItem("Tools/Change Language To Brazil")]
        public static void _ChangeLanguageToBrazil() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.Brazil);
        }

        [MenuItem("Tools/Change Language To Philippine")]
        public static void _ChangeLanguageToPhilippine() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.Philippine);
        }

        [MenuItem("Tools/Change Language To Spanish")]
        public static void _ChangeLanguageToSpanish() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.Spanish);
        }

        [MenuItem("Tools/Change Language To Thai")]
        public static void _ChangeLanguageToThai() {
            ServiceLocator.Instance.Resolve<ILanguageManager>().SetLanguage(LocalizeLang.Thai);
        }
#endif
    }
}