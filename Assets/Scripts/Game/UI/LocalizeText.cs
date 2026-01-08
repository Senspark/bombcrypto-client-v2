using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class LocalizeText : MonoBehaviour {
        [SerializeField]
        private bool upperCase;

        [SerializeField]
        private string key;

        private ObserverHandle _handle;
        private Font _font;
        private static Font _unicodeFont;
        private Text _text;

        private INetworkConfig _networkConfig;

        private void Awake() {
            _text = GetComponent<Text>();
            _font = _text.font;
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _handle = new ObserverHandle();
            _handle.AddObserver(languageManager, new LanguageObserver {
                OnLanguageChanged = OnLanguageChanged,
            });
        }

        private void Start() {
            RefreshText();
        }

        private void OnDestroy() {
            if (_handle != null) {
                _handle.Dispose();
            }
        }

        public void SetNewText(string value) {
            _text.text = value;
        }

        public void SetNewKey(LocalizeKey localizeKey) {
            key = localizeKey.ToString();
            RefreshText();
        }

        public static void ToUnicodeFontIfNotSupport(Text text, LocalizeLang lang) {
            if (lang != LocalizeLang.English && lang != LocalizeLang.Philippine) {
                CheckUnicodeFont(lang);
                text.font = _unicodeFont;
            }
        }

        private static void CheckUnicodeFont(LocalizeLang lang) {
            if (_unicodeFont == null) {
                if (lang == LocalizeLang.Thai) {
                    _unicodeFont = Resources.Load<Font>("Fonts/Mali-Bold");
                } else {
                    _unicodeFont = Resources.Load<Font>("Fonts/Roboto-Bold");
                }
            }
        }

        private void RefreshText() {
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();

            if (!string.IsNullOrWhiteSpace(key) && key != LocalizeKey.NONE.ToString()) {
                var val = languageManager.GetValue(key);
                if (upperCase) {
                    val = val.ToUpperInvariant();
                }

                _text.text = val;
            }
            if (_networkConfig.NetworkType == NetworkType.Binance) {
                AutoFont(languageManager.CurrentLanguage);
            }
        }

        private void OnLanguageChanged() {
            _unicodeFont = null;
            RefreshText();
        }

        private void AutoFont(LocalizeLang lang) {
            if (lang == LocalizeLang.English || lang == LocalizeLang.Philippine) {
                _text.font = _font;
            } else {
                CheckUnicodeFont(lang);
                _text.font = _unicodeFont;
            }
        }
    }
}