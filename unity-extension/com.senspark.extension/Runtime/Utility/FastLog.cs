using System.Diagnostics;

using UnityEngine;

namespace Senspark {
    public static class FastLog {
        private const int MAX_LOG_LENGTH = 900;
        
        [Conditional("DEBUG"),
         Conditional("DEVELOPMENT_BUILD"),
         Conditional("UNITY_EDITOR"),
         Conditional("BUILD_WITH_DEBUG_KEY")]
        public static void Debug(string message) {
            UnityEngine.Debug.Log(message);
        }

        public static void Info(string message) {
            Print(LogType.Log, message);
        }

        public static void Warn(string message) {
            Print(LogType.Warning, message);
        }

        public static void Error(string message) {
            Print(LogType.Error, message);
        }

        private static string SanityFormat(string message) {
            message = message.Replace("{", "{{");
            message = message.Replace("}", "}}");
            return message;
        }

        private static void Print(LogType logType, string message) {
            while (true) {
                if (message.Length > MAX_LOG_LENGTH) {
                    UnityEngine.Debug.LogFormat(logType, LogOption.NoStacktrace, null,
                        SanityFormat(message[..MAX_LOG_LENGTH]));
                    message = message[MAX_LOG_LENGTH..];
                    continue;
                }
                UnityEngine.Debug.LogFormat(logType, LogOption.NoStacktrace, null, SanityFormat(message));
                break;
            }
        }
    }
}