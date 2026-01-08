using System;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace Utils {
    public static class Logger {
        [Conditional("UNITY_EDITOR")]
        public static void LogEditorError(Exception e) {
            Debug.LogError(e);
        }
    }
}