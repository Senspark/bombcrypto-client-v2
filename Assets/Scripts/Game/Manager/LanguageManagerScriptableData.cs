using System;
using System.Collections.Generic;

using UnityEngine;

namespace App.Language {
    // [CreateAssetMenu(fileName = FILE_NAME, menuName = "ScriptableObjects/LanguageManager")]
    public class LanguageManagerScriptableData : ScriptableObject {
        public List<Data> data;
    }
    
    [Serializable]
    public class Data {
        public NetworkType networkType;
        public List<LanguagePath> languagePaths;
    }
            
    [Serializable]
    public class LanguagePath {
        public LocalizeLang languageType;
        public TextAsset jsonFile;
    }
}