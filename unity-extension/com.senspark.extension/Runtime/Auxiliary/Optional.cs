using System;

using UnityEngine;

namespace Senspark {
    public static class Optional {
        public static T Get<T>(Func<T> getter, T defaultValue) {
            try {
                return getter();
            } catch (Exception e) {
#if UNITY_EDITOR
                Debug.LogException(e);
#else
                FastLog.Error($"Optional: error: {e.Message} - return ${defaultValue}");
#endif
                return defaultValue;
            }
        }
    }
}