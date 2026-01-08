using System;

using Senspark;

namespace App {
    [Service(nameof(ILanguageManager))]
    public interface ILanguageManager : IService, IObserverManager<LanguageObserver> {
        LocalizeLang CurrentLanguage { get; }
        string CurrentLanguageSymbol { get; }
        void SetLanguage(LocalizeLang id, bool dispatch = true);
        string GetValue(LocalizeKey key);
        string GetValue(string key);
    }

    public class LanguageObserver {
        public Action OnLanguageChanged;
    } 
    
    [Serializable]
    public enum LocalizeLang {
        English,
        VietNam,
        Philippine,
        Brazil,
        Spanish,
        Thai
    }
}