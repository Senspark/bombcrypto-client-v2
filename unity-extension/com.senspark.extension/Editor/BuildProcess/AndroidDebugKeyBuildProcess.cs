using UnityEditor;
using Utility;

namespace Senspark.Editor.BuildProcess {
    public static class AndroidDebugKeyBuildProcess {
        public static void AddDefineSymbol() {
#if UNITY_ANDROID
            var buildWithDebugKey = !PlayerSettings.Android.useCustomKeystore;
            const string symbol = CustomDefineSymbols.BuildWithDebugKey;
            if (buildWithDebugKey) {
                AddDefineSymbol(BuildTargetGroup.Android, symbol);
            }
            else {
                RemoveDefineSymbol(BuildTargetGroup.Android, symbol);
            }
#endif
        }

        public static void RemoveDefineSymbol() {
#if UNITY_ANDROID
            const string symbol = CustomDefineSymbols.BuildWithDebugKey;
            RemoveDefineSymbol(BuildTargetGroup.Android, symbol);
#endif
        }

        private static void AddDefineSymbol(BuildTargetGroup group, string symbol) {
            var defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (!defined.Contains(symbol)) {
                defined = $"{defined};{symbol}";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defined);
        }

        private static void RemoveDefineSymbol(BuildTargetGroup group, string symbol) {
            var defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            defined = defined.Replace(symbol, "");
            defined = defined.Replace(";;", ";");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defined);
        }
    }
}